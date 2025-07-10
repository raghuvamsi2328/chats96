# 👷 Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["chats96.sln", "."]
COPY ["chats96.Api/chats96.Api.csproj", "chats96.Api/"]
RUN apt-get update && apt-get install -y ca-certificates
RUN dotnet restore "chats96.sln" --verbosity detailed
COPY chats96.Api/. chats96.Api/
RUN dotnet build "chats96.Api/chats96.Api.csproj" -c Release -o /app/build

# 🚀 Stage 2: Publish
FROM build AS publish
RUN dotnet publish "chats96.Api/chats96.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 🌐 Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "chats96.Api.dll"]
