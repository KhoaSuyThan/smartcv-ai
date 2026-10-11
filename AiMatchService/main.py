from fastapi import FastAPI, UploadFile, File
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
from sentence_transformers import SentenceTransformer, util
import uvicorn
import torch
import numpy as np
import json
import os
import time
from pii_extractor import extract_text_adaptive, anonymize_text
from ats_scorer import score_ats_candidate

app = FastAPI()

# Tải model AI Đa Ngôn Ngữ (Hỗ trợ cực tốt Tiếng Việt)
print("Đang khởi động AI Model (Có thể tốn 1-2 phút để tải model Multilingual)...")
model = SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')
print("✅ AI Model đã khởi động và sẵn sàng nhận yêu cầu!")

class CVItem(BaseModel):
    id: int
    text: str

class FilterRequest(BaseModel):
    jd_text: str
    cv_list: List[CVItem]

# --- Endpoint mới: Encode 1 CV thành vector (dùng cho caching) ---
class EmbedRequest(BaseModel):
    text: str

class EmbedResponse(BaseModel):
    vector: List[float]

@app.post("/api/embed-cv", response_model=EmbedResponse)
async def embed_cv(req: EmbedRequest):
    """Encode một đoạn text CV thành vector 384 chiều để lưu cache."""
    embedding = model.encode(req.text, convert_to_tensor=False)
    return {"vector": embedding.tolist()}

# --- Endpoint mới: Tính similarity từ vector có sẵn (bỏ qua encode lại) ---
class CachedCV(BaseModel):
    id: int
    vector: List[float]  # Vector đã encode sẵn, đọc từ DB cache

class SimilarityRequest(BaseModel):
    jd_text: str
    cached_cvs: List[CachedCV]

@app.post("/api/similarity-from-vectors")
async def similarity_from_vectors(req: SimilarityRequest):
    """Tính cosine similarity giữa JD và các CV đã được vector hóa sẵn.
    Bỏ qua bước encode CV → nhanh hơn đáng kể khi cache tồn tại."""
    if not req.cached_cvs:
        return {"top_cvs": []}

    # Encode JD (vẫn cần encode mỗi lần vì JD thay đổi theo từng Job)
    jd_embedding = model.encode(req.jd_text, convert_to_tensor=True)

    # Chuyển vectors từ DB (list[float]) sang tensor
    cv_tensors = torch.tensor([cv.vector for cv in req.cached_cvs])

    # Tính cosine similarity batch
    cosine_scores = util.cos_sim(jd_embedding, cv_tensors)[0]

    results = []
    for i, score in enumerate(cosine_scores):
        sim_score = score.item()
        if sim_score >= 0.30:
            results.append({
                "resume_id": req.cached_cvs[i].id,
                "similarity": sim_score
            })

    results.sort(key=lambda x: x["similarity"], reverse=True)
    return {"top_cvs": results[:10]}

# --- Endpoint gốc: Vẫn giữ để tương thích (encode + similarity cùng lúc) ---
@app.post("/api/filter-top-cvs")
async def filter_top_cvs(req: FilterRequest):
    if not req.cv_list:
        return {"top_cvs": []}

    jd_embedding = model.encode(req.jd_text, convert_to_tensor=True)
    cv_texts = [cv.text for cv in req.cv_list]
    cv_embeddings = model.encode(cv_texts, convert_to_tensor=True)
    cosine_scores = util.cos_sim(jd_embedding, cv_embeddings)[0]

    results = []
    for i, score in enumerate(cosine_scores):
        sim_score = score.item()
        if sim_score >= 0.30:
            results.append({
                "resume_id": req.cv_list[i].id,
                "similarity": sim_score
            })

    results.sort(key=lambda x: x["similarity"], reverse=True)
    return {"top_cvs": results[:10]}

# --- Endpoint: Khử PII từ văn bản thô ---
class AnonymizeTextRequest(BaseModel):
    text: str

class AnonymizeTextResponse(BaseModel):
    clean_text: str
    language: str
    word_count: int

@app.post("/api/pii/anonymize-text", response_model=AnonymizeTextResponse)
async def api_anonymize_text(req: AnonymizeTextRequest):
    """Ẩn danh thông tin định danh cá nhân (PII) từ chuỗi văn bản."""
    return anonymize_text(req.text)

# --- Endpoint: Nhận file PDF, bóc tách 2 cột adaptive, che PII & sinh vector embedding ---
@app.post("/api/pii/extract-and-anonymize-pdf")
async def extract_and_anonymize_pdf(
    file: UploadFile = File(...),
    include_vector: bool = True
):
    """
    Bóc tách text từ file PDF (hỗ trợ layout 1 cột và 2 cột thích ứng),
    che giấu thông tin định danh cá nhân (PII), phân loại ngôn ngữ,
    và sinh vector embedding 384 chiều phục vụ tìm kiếm SmartMatch.
    """
    try:
        pdf_bytes = await file.read()
        raw_text = extract_text_adaptive(pdf_bytes)
        clean_result = anonymize_text(raw_text)

        vector = None
        if include_vector and clean_result["clean_text"]:
            embedding = model.encode(clean_result["clean_text"], convert_to_tensor=False)
            vector = embedding.tolist()

        return {
            "success": True,
            "filename": file.filename,
            "clean_text": clean_result["clean_text"],
            "language": clean_result["language"],
            "word_count": clean_result["word_count"],
            "vector": vector
        }
    except Exception as e:
        return {
            "success": False,
            "error": str(e),
            "clean_text": "",
            "language": "unknown",
            "word_count": 0,
            "vector": None
        }

# =========================================================================
# KIẾN TRÚC HYBRID 2 VÒNG: FAST RETRIEVAL + DEEP ATS SCORING
# =========================================================================

DATA_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Data"))
NPY_PATH = os.path.join(DATA_DIR, "CV_Vector_Cache_384d.npy")
IDX_PATH = os.path.join(DATA_DIR, "CV_Vector_Index.json")
CVS_PATH = os.path.join(DATA_DIR, "Clean_Anonymized_CVs.json")

cached_matrix = None
cached_index = None
cached_cv_texts = None


def load_vector_assets():
    """Tải trước Vector Cache và metadata chỉ mục vào RAM để truy vấn siêu tốc."""
    global cached_matrix, cached_index, cached_cv_texts
    if cached_matrix is None and os.path.exists(NPY_PATH) and os.path.exists(IDX_PATH):
        try:
            cached_matrix = np.load(NPY_PATH)
            with open(IDX_PATH, "r", encoding="utf-8") as f:
                cached_index = json.load(f)
            print(f"✅ Đã nạp thành công Vector Cache: {cached_matrix.shape} ({len(cached_index)} CVs)")
        except Exception as e:
            print(f"⚠️ Lỗi nạp Vector Cache: {e}")

    if cached_cv_texts is None and os.path.exists(CVS_PATH):
        try:
            with open(CVS_PATH, "r", encoding="utf-8") as f:
                cvs = json.load(f)
            cached_cv_texts = [c.get("raw_clean_text", "") for c in cvs]
            print(f"✅ Đã nạp bộ văn bản sạch {len(cached_cv_texts)} CVs cho Vòng 2")
        except Exception as e:
            print(f"⚠️ Lỗi nạp văn bản CV: {e}")


# Tự động nạp cache khi module được khởi tạo
load_vector_assets()


class FastFilterRequest(BaseModel):
    jd_text: str
    top_k: int = 25


@app.post("/api/match/fast-filter")
async def fast_filter(req: FastFilterRequest):
    """
    VÒNG 1: Phễu lọc Cosine Similarity siêu tốc trên Vector Cache 3.320 CV (< 0.05 giây).
    """
    load_vector_assets()
    if cached_matrix is None or cached_index is None:
        return {"success": False, "message": "Vector cache chưa sẵn sàng trên đĩa."}

    t0 = time.time()
    # Mã hóa JD thành vector chuẩn hóa L2
    jd_vector = model.encode(req.jd_text, normalize_embeddings=True)

    # Cosine Similarity với vector đã chuẩn hóa = Tích vô hướng Dot Product
    scores = np.dot(cached_matrix, jd_vector)

    # Trích xuất Top-K chỉ số có điểm số cao nhất
    top_k_indices = np.argsort(scores)[::-1][:req.top_k]

    results = []
    for row_idx in top_k_indices:
        meta = cached_index[row_idx]
        results.append({
            "matrix_row": int(row_idx),
            "id": meta["id"],
            "filename": meta["filename"],
            "language": meta["language"],
            "similarity_score": round(float(scores[row_idx]), 4)
        })

    latency_ms = round((time.time() - t0) * 1000, 2)
    return {
        "success": True,
        "round": 1,
        "latency_ms": latency_ms,
        "total_scanned": len(cached_matrix),
        "top_candidates": results
    }


class HybridMatchRequest(BaseModel):
    jd_text: str
    hard_skills: Optional[List[str]] = []
    experience_years_required: Optional[float] = 2.0
    level: Optional[str] = "Middle"
    title: Optional[str] = "Software Engineer"
    filter_top_k: Optional[int] = 25
    final_top_k: Optional[int] = 10


@app.post("/api/match/hybrid-match")
async def hybrid_match(req: HybridMatchRequest):
    """
    ĐỘNG CƠ HYBRID TOÀN DIỆN:
    - Vòng 1: Quét 3.320 CV bằng Vector Cache -> Lấy Top 25 ứng viên tiềm năng nhất (< 0.05s).
    - Vòng 2: Chấm điểm ATS chuyên sâu (chống nhồi từ khóa, Knockout rule, số năm KN, học vấn).
    - Tái xếp hạng (Re-rank) và trả về Top 10 ứng viên chính xác nhất.
    """
    load_vector_assets()
    if cached_matrix is None or cached_index is None or cached_cv_texts is None:
        return {"success": False, "message": "Dữ liệu Vector Cache hoặc tệp CV sạch chưa sẵn sàng."}

    t0 = time.time()

    # --- BƯỚC 1: VÒNG 1 FAST FILTER ---
    jd_vector = model.encode(req.jd_text, normalize_embeddings=True)
    scores = np.dot(cached_matrix, jd_vector)
    top_indices = np.argsort(scores)[::-1][:req.filter_top_k]
    v1_latency_ms = round((time.time() - t0) * 1000, 2)

    # --- BƯỚC 2: VÒNG 2 DEEP ATS SCORING ---
    t_v2 = time.time()
    jd_dict = {
        "title": req.title,
        "hard_skills": req.hard_skills or [],
        "experience_years_required": req.experience_years_required or 2.0,
        "level": req.level or "Middle"
    }

    v2_candidates = []
    for row_idx in top_indices:
        meta = cached_index[row_idx]
        cv_text = cached_cv_texts[row_idx]
        semantic_sim = float(scores[row_idx])

        # Chạy thuật toán chấm điểm ATS thế hệ mới với 5 cơ chế thông minh
        ats_eval = score_ats_candidate(cv_text, jd_dict, semantic_sim)

        v2_candidates.append({
            "matrix_row": int(row_idx),
            "id": meta["id"],
            "filename": meta["filename"],
            "language": meta["language"],
            "semantic_score": round(semantic_sim, 4),
            "ats_score": ats_eval["match_score"],
            "matched_skills": ats_eval["matched_skills"],
            "missing_skills": ats_eval["missing_skills"],
            "proven_skills": ats_eval["proven_skills"],
            "unverified_skills": ats_eval["unverified_skills"],
            "knockout_passed": ats_eval["knockout_passed"],
            "experience_eval": ats_eval["experience_eval"],
            "seniority_fit": ats_eval["seniority_fit"],
            "strengths": ats_eval["strengths"],
            "suggestions": ats_eval["suggestions"],
            "summary": ats_eval["summary"],
            "recommendation": ats_eval["recommendation"]
        })

    # Tái xếp hạng (Re-ranking) theo ats_score giảm dần
    v2_candidates.sort(key=lambda x: (x["knockout_passed"], x["ats_score"]), reverse=True)
    final_top = v2_candidates[:req.final_top_k]

    v2_latency_ms = round((time.time() - t_v2) * 1000, 2)
    total_latency_ms = round((time.time() - t0) * 1000, 2)

    return {
        "success": True,
        "total_scanned": len(cached_matrix),
        "v1_passed_count": len(top_indices),
        "final_count": len(final_top),
        "latency": {
            "v1_vector_cache_ms": v1_latency_ms,
            "v2_ats_scoring_ms": v2_latency_ms,
            "total_latency_ms": total_latency_ms
        },
        "top_candidates": final_top
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)


