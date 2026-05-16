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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish .
# Mở port 8080 (mặc định của App Runner)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "DoAnCS.dll"]
