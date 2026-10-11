from fastapi import FastAPI, UploadFile, File
from pydantic import BaseModel
from typing import List, Optional
from sentence_transformers import SentenceTransformer, util
import uvicorn
import torch
from pii_extractor import extract_text_adaptive, anonymize_text

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

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)

