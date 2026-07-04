# ====================================================
# Dockerfile cho AiMatchService (Python FastAPI)
# ====================================================

# Dùng Python 3.11 slim để giảm kích thước image
FROM python:3.11-slim

# Thư mục làm việc bên trong container
WORKDIR /app

# Copy requirements trước (tận dụng Docker layer cache — chỉ re-install khi requirements thay đổi)
COPY requirements.txt .

# Cài đặt dependencies (torch CPU-only để nhẹ hơn, không cần GPU trên server)
RUN pip install --no-cache-dir torch --index-url https://download.pytorch.org/whl/cpu \
    && pip install --no-cache-dir -r requirements.txt

# Copy toàn bộ source code
COPY . .

# Pre-download model vào image khi build → container khởi động nhanh hơn
# Model sẽ được cache trong /root/.cache/huggingface bên trong image
RUN python -c "from sentence_transformers import SentenceTransformer; SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')"

# Expose port nội bộ (chỉ trong Docker network, không expose ra internet)
EXPOSE 8000

# Khởi động FastAPI bằng Uvicorn
CMD ["python", "-m", "uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
