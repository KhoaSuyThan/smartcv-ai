# --- Giai đoạn 1: Build Frontend (Vue) ---
FROM node:20-alpine AS frontend-build
WORKDIR /app
COPY CVBuilderApp/package*.json ./CVBuilderApp/
RUN cd CVBuilderApp && npm ci
COPY CVBuilderApp/ ./CVBuilderApp/
RUN cd CVBuilderApp && npm run build

# --- Giai đoạn 2: Build Backend (.NET) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY ["DoAnCS.csproj", "."]
RUN dotnet restore "DoAnCS.csproj"
COPY . .
# Copy kết quả build của frontend vào thư mục wwwroot của backend
COPY --from=frontend-build /app/wwwroot/cvbuilder ./wwwroot/cvbuilder
RUN dotnet publish "DoAnCS.csproj" -c Release -o /app/publish

# --- Giai đoạn 3: Runtime ---
# Sử dụng base image aspnet nhẹ nhàng (~100MB) để tránh lỗi hết dung lượng đĩa (no space left on device)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish .

# Cài đặt các thư viện hệ thống tối thiểu để chạy Chromium (không dùng --with-deps để tránh seccomp/exit code 131)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libasound2t64 \
    libpango-1.0-0 \
    libcairo2 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libxtst6 \
    libxi6 \
    fonts-liberation \
    && PLAYWRIGHT_SH=$(find .playwright/node/ -name "playwright.sh" | head -n 1) \
    && chmod +x "$PLAYWRIGHT_SH" \
    && "$PLAYWRIGHT_SH" install chromium \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Mở port 8080 (mặc định của App Runner)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "DoAnCS.dll"]
