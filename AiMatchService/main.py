from fastapi import FastAPI
from pydantic import BaseModel
from typing import List
from sentence_transformers import SentenceTransformer, util
import uvicorn

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

@app.post("/api/filter-top-cvs")
async def filter_top_cvs(req: FilterRequest):
    if not req.cv_list:
        return {"top_cvs": []}

    # 1. Tạo Vector cho Job Description (Tính toán cực nhanh bằng C++)
    jd_embedding = model.encode(req.jd_text, convert_to_tensor=True)
    
    # 2. Tạo Vector cho toàn bộ danh sách CV
    cv_texts = [cv.text for cv in req.cv_list]
    cv_embeddings = model.encode(cv_texts, convert_to_tensor=True)
    
    # 3. Tính toán Cosine Similarity cho TẤT CẢ CV cùng một lúc
    cosine_scores = util.cos_sim(jd_embedding, cv_embeddings)[0]
    
    # 4. Lọc và xếp hạng
    results = []
    for i, score in enumerate(cosine_scores):
        sim_score = score.item()
        # Hạ ngưỡng xuống 0.30: Nới lỏng tối đa để chọn được 6 ứng viên
        if sim_score >= 0.30:  
            results.append({
                "resume_id": req.cv_list[i].id,
                "similarity": sim_score
            })
            
    # Sắp xếp điểm từ cao xuống thấp và chỉ lấy Top 10
    results.sort(key=lambda x: x["similarity"], reverse=True)
    return {"top_cvs": results[:10]}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
