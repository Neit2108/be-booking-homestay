# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sao chép csproj và restore dependencies trước - tận dụng layer caching
COPY ["HomestayBookingAPI/HomestayBookingAPI.csproj", "HomestayBookingAPI/"]
RUN dotnet restore "HomestayBookingAPI/HomestayBookingAPI.csproj"

# Sao chép toàn bộ source code
COPY . .

# Build project
RUN dotnet build "HomestayBookingAPI/HomestayBookingAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "HomestayBookingAPI/HomestayBookingAPI.csproj" -c Release -o /app/publish

# Tạo template cho file cấu hình
WORKDIR /app/publish
COPY HomestayBookingAPI/appsettings.template.json ./appsettings.template.json
RUN apt-get update && apt-get install -y gettext-base

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Cài đặt các công cụ debug
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       gettext-base \
       wget \
       curl \
       nano \
       procps \
       grep \
       jq \
    && rm -rf /var/lib/apt/lists/*

# Sao chép từ publish stage
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/appsettings.template.json ./appsettings.template.json

# Tạo script khởi động nâng cao
RUN echo '#!/bin/bash \n\
set -e \n\
\n\
echo "========== DEBUG INFORMATION ==========" \n\
echo "Container hostname: $(hostname)" \n\
echo "Container working directory: $(pwd)" \n\
echo "Date and time: $(date)" \n\
echo "========== FILE LIST ==========" \n\
ls -la \n\
echo \n\
\n\
echo "========== ENVIRONMENT VARIABLES ==========" \n\
echo "Environment variables related to JWT:" \n\
printenv | grep -i jwt || echo "No JWT variables found" \n\
echo \n\
echo "Environment variables related to Cloudinary:" \n\
printenv | grep -i cloudinary || echo "No Cloudinary variables found" \n\
echo \n\
echo "Environment variables related to Connection:" \n\
printenv | grep -i connection || echo "No Connection variables found" \n\
echo \n\
echo "Environment variables related to SMTP:" \n\
printenv | grep -i smtp || echo "No SMTP variables found" \n\
echo \n\
echo "Environment variables related to VNPAY:" \n\
printenv | grep -i vnpay || echo "No VNPAY variables found" \n\
echo \n\
echo "Environment variables related to IMGUR:" \n\
printenv | grep -i imgur || echo "No IMGUR variables found" \n\
echo \n\
\n\
echo "========== TEMPLATE CONTENT ==========" \n\
echo "Content of appsettings.template.json:" \n\
grep -v "Secret\|Password\|ConnectionString\|ApiKey" appsettings.template.json || echo "Template file not found" \n\
echo \n\
\n\
echo "========== GENERATING CONFIGURATION ==========" \n\
echo "Generating appsettings.Development.json from template..." \n\
envsubst < appsettings.template.json > appsettings.Development.json \n\
\n\
echo "========== GENERATED CONFIGURATION ==========" \n\
echo "Generated appsettings.Development.json structure:" \n\
grep -v "Secret\|Password\|ConnectionString\|ApiKey" appsettings.Development.json | grep -v "^\s*$" || echo "Failed to generate configuration" \n\
echo \n\
\n\
echo "========== JWT SECTION CHECK ==========" \n\
jq -e ".JwtSettings" appsettings.Development.json > /dev/null 2>&1 && echo "JwtSettings section exists" || echo "JwtSettings section MISSING" \n\
jq -e ".JwtSettings.Secret" appsettings.Development.json > /dev/null 2>&1 && echo "JwtSettings.Secret exists" || echo "JwtSettings.Secret MISSING" \n\
echo \n\
\n\
echo "========== CLOUDINARY SECTION CHECK ==========" \n\
jq -e ".Cloudinary" appsettings.Development.json > /dev/null 2>&1 && echo "Cloudinary section exists" || echo "Cloudinary section MISSING" \n\
jq -e ".Cloudinary.CloudName" appsettings.Development.json > /dev/null 2>&1 && echo "Cloudinary.CloudName exists" || echo "Cloudinary.CloudName MISSING" \n\
echo \n\
\n\
echo "========== EXECUTING APPLICATION ==========" \n\
echo "Starting ASP.NET Core application..." \n\
exec dotnet HomestayBookingAPI.dll \n\
' > /app/start.sh \
&& chmod +x /app/start.sh

# Sao chép biến môi trường mặc định cho phát triển (chỉ khi không có biến môi trường thực)
# Các giá trị này sẽ được sử dụng khi không có biến môi trường được cung cấp
ENV JWT_SECRET="default_jwt_secret_value_for_development_only" \
    CLOUDINARY_CLOUD_NAME="default_cloud_name" \
    CLOUDINARY_API_KEY="default_api_key" \
    CLOUDINARY_API_SECRET="default_api_secret"

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/weatherforecast || exit 1

ENTRYPOINT ["/app/start.sh"]