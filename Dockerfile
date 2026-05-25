# =====================================================
# Stage 1: BUILD
# Dùng SDK image để compile code
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy từng file .csproj trước (tận dụng Docker cache)
# Nếu .csproj không đổi → Docker bỏ qua bước restore
COPY Shared/NovaStaff.Shared.csproj                           Shared/
COPY NovaStaff.Model/NovaStaff.Models.csproj                  NovaStaff.Model/
COPY NovaStaff.DataLayes/NovaStaff.DataLayers.csproj          NovaStaff.DataLayes/
COPY NovaStaff.BusinessLayers/NovaStaff.BusinessLayers.csproj NovaStaff.BusinessLayers/
COPY NovaStaff.Infrastructure/NovaStaff.Infrastructure.csproj NovaStaff.Infrastructure/
COPY NovaStaff.Admin/NovaStaff.API.csproj                     NovaStaff.Admin/
COPY NovaStaff.sln .

# Restore packages
RUN dotnet restore NovaStaff.sln

# Copy toàn bộ code còn lại
COPY . .

# Build và publish
RUN dotnet publish NovaStaff.Admin/NovaStaff.API.csproj \
    -c Release \                  
    -o /app/publish \             
    --no-restore                     

# =====================================================
# Stage 2: RUNTIME
# Chỉ copy kết quả build, không cần SDK
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy kết quả từ stage build
COPY --from=build /app/publish .

# Port app lắng nghe
EXPOSE 8080

# Lệnh chạy khi container start
ENTRYPOINT ["dotnet", "NovaStaff.API.dll"]