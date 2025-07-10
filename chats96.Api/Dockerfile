# chats96.Api/Dockerfile (Path is now relative to the CHATS96 root)

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file first. Use the exact casing from your file system.
# Your screenshot shows 'chats.sln' in lowercase.
COPY ["chats.sln", "."]

# Copy the project file(s) for dependency restoration.
# Path is relative to the CHATS96 root.
COPY ["chats96.Api/chats96.Api.csproj", "chats96.Api/"]

# Restore NuGet packages for the entire solution.
# Use the exact casing for the solution file name.
RUN dotnet restore "chats.sln" 
# <--- IMPORTANT: Changed to "chats.sln" (all lowercase)

# Copy all the remaining project files for chats96.Api.
COPY chats96.Api/. chats96.Api/

# Build the application
# Specify the project path relative to the WORKDIR /src
RUN dotnet build "chats96.Api/chats96.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
# Publish the specific project from the solution
RUN dotnet publish "chats96.Api/chats96.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "chats96.Api.dll"]