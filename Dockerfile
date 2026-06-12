# ============================================================
# Stage 1: Build - dùng .NET SDK 8.0 để restore và publish
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj trước để tận dụng Docker layer cache khi restore
COPY FapWeb.csproj ./
RUN dotnet restore FapWeb.csproj

# Copy toàn bộ source và publish bản Release
COPY . .
RUN dotnet publish FapWeb.csproj -c Release -o /app/publish /p:UseAppHost=false

# ============================================================
# Stage 2: Runtime - chỉ chứa ASP.NET Core runtime, image nhẹ
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# ASP.NET Core 8 mặc định lắng nghe cổng 8080 trong container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FapWeb.dll"]
