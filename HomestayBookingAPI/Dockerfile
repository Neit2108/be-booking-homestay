# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HomestayBookingAPI/HomestayBookingAPI.csproj", "./"]
RUN dotnet restore "HomestayBookingAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "HomestayBookingAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "HomestayBookingAPI/HomestayBookingAPI.csproj" -c Release -o /app/publish

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENTRYPOINT ["dotnet", "HomestayBookingAPI.dll"]