# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies for the whole solution
COPY ["chats96.sln", "."]
COPY ["chats96.Api/chats96.Api.csproj", "chats96.Api/"]
# If you have other projects (e.g., chats96.Data.csproj), add them here:
# COPY ["chats96.Data/chats96.Data.csproj", "chats96.Data/"]

# Restore dependencies for all projects in the solution
RUN dotnet restore "chats96.Api/chats96.Api.csproj"

# Copy the rest of the source code
COPY . .

# Change to the API project directory
WORKDIR "/src/chats96.Api"

# Build the project for release configuration
RUN dotnet build "chats96.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /src/chats96.Api/. /app

# The previous build step already outputs to /app/build, let's copy directly from there
# Or, even better, run `dotnet publish` in the build stage
# Let's adjust the build stage to publish directly to /app/publish for cleaner copy

# Corrected Stage 1 to publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["chats96.sln", "."]
COPY ["chats96.Api/chats96.Api.csproj", "chats96.Api/"]
RUN dotnet restore "chats96.Api/chats96.Api.csproj"
COPY . .
WORKDIR "/src/chats96.Api"
RUN dotnet publish "chats96.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "chats96.Api.dll"]