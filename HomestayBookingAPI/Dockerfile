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

# Tạo người dùng không phải root để chạy ứng dụng
RUN adduser --disabled-password --gecos "" appuser

# Cài đặt gettext-base cho envsubst
RUN apt-get update \
    && apt-get install -y --no-install-recommends gettext-base \
    && rm -rf /var/lib/apt/lists/*

# Sao chép từ publish stage
COPY --from=publish /app/publish .
COPY --from=publish /app/publish/appsettings.template.json ./appsettings.template.json

# Tạo script khởi động
RUN echo '#!/bin/bash \n\
echo "Checking environment variables..." \n\
echo "JWT_SECRET is set: $(if [ -n \"\$JWT_SECRET\" ]; then echo \"YES\"; else echo \"NO\"; fi)" \n\
echo "Generating configuration file from template..." \n\
envsubst < appsettings.template.json > appsettings.Development.json \n\
echo "Generated appsettings.Development.json structure:" \n\
grep -v "Secret\|Password\|ConnectionString" appsettings.Development.json | head -20 \n\
echo "Starting application..." \n\
exec dotnet HomestayBookingAPI.dll \n\
' > /app/start.sh \
&& chmod +x /app/start.sh

# Cấp quyền cho thư mục app
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/weatherforecast || exit 1

ENTRYPOINT ["/app/start.sh"]