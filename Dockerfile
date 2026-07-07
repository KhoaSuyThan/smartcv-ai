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
# Sử dụng image Playwright chính thức của Microsoft đã cài đặt sẵn các trình duyệt và thư viện liên quan
FROM mcr.microsoft.com/playwright/dotnet:v1.61.0-noble AS final
WORKDIR /app

# Sao chép .NET 10.0 runtime từ image chính thức để đảm bảo chạy được ứng dụng .NET 10
COPY --from=mcr.microsoft.com/dotnet/aspnet:10.0 /usr/share/dotnet /usr/share/dotnet

# Sao chép kết quả build của backend
COPY --from=backend-build /app/publish .

# Mở port 8080 (mặc định của App Runner)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "DoAnCS.dll"]
