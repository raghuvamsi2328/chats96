# Use a smaller base image for .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file first to leverage Docker layer caching
# The 'context' for this build is already 'chats96.Api/'
# So, 'chats96.Api.csproj' is directly at the root of the context.
COPY ["chats96.Api.csproj", "."] # This copies chats96.Api/chats96.Api.csproj to /src/

# Copy all other project files and restore
COPY . .
RUN dotnet restore "chats96.Api.csproj"

# Publish the application
FROM build AS publish
RUN dotnet publish "chats96.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "chats96.Api.dll"]