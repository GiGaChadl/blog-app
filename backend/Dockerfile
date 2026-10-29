#================================
# Giai đoạn 1: BUILD & TEST (SDK)
#================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app


#1. Copy file solution và csproj trước để tận dụng Docker Cache
COPY BlogApp.slnx ./
COPY src/BlogApp.Domain/*.csproj ./src/BlogApp.Domain/
COPY src/BlogApp.Application/*.csproj ./src/BlogApp.Application/
COPY src/BlogApp.Infrastructure/*.csproj ./src/BlogApp.Infrastructure/
COPY src/BlogApp.Api/*.csproj ./src/BlogApp.Api/
COPY tests/BlogApp.Application.Tests/*.csproj ./tests/BlogApp.Application.Tests/

#2. Restore các thư viện NuGet
RUN dotnet restore

#3. Copy toàn bộ mã nguồn vào
COPY . ./

#4. Chạy unit test trước khi build (nếu test fail thì dừng buid ngay)
RUN dotnet test tests/BlogApp.Application.Tests/BlogApp.Application.Tests.csproj --no-restore --verbosity normal

#5. Publish bản Release
RUN dotnet publish src/BlogApp.Api/BlogApp.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

#================================
# Giai đoạn 2: RUNTIME (Chỉ chứa môi trường chạy, siêu nhẹ)
#================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Tạo thư mục lưu file upload
RUN mkdir -p /app/wwwroot/uploads

# Copy kết quả biên dịch từ stage build sang
COPY --from=build /app/publish .

# Cổng mặc định của .NET 8+ là 8080
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "BlogApp.Api.dll"]