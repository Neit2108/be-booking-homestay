#!/bin/bash
# Hiển thị các biến môi trường (debugging)
echo "JWT_SECRET is set: $(if [ -n "$JWT_SECRET" ]; then echo "YES"; else echo "NO"; fi)"

# Thực hiện thay thế biến
envsubst < appsettings.template.json > appsettings.Development.json

# Hiển thị file được tạo (debugging)
echo "Generated appsettings.Development.json:"
cat appsettings.Development.json

# Chạy ứng dụng
exec dotnet HomestayBookingAPI.dll