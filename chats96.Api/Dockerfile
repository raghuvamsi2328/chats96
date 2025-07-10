# chats96.Api/Dockerfile

# Stage 1: Build the application
# Use the SDK image for building and restoring NuGet packages
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container for this stage
WORKDIR /src

# Copy the .csproj file first to leverage Docker layer caching.
# Since the build context is 'chats96.Api', 'chats96.Api.csproj' is directly available.
COPY ["chats96.Api.csproj", "."]

# Restore NuGet packages. This step runs only if csproj or restore settings change.
RUN dotnet restore "chats96.Api.csproj"

# Copy the rest of the application's source code into the working directory.
# The first '.' refers to the current build context (./chats96.Api on your host).
# The second '.' refers to the WORKDIR /src inside the container.
COPY . .

# Build the application in Release configuration
RUN dotnet build "chats96.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
# Use the 'build' stage as the base for this stage
FROM build AS publish

# Publish the application for deployment.
# It creates a self-contained deployment in /app/publish
RUN dotnet publish "chats96.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
# Use the ASP.NET runtime image, which is lighter than the SDK image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the working directory in the final image
WORKDIR /app

# Copy the published application files from the 'publish' stage to the final image
COPY --from=publish /app/publish .

# Expose port 80. This tells Docker that the container listens on this port.
# Nginx Proxy Manager will forward to this internal container port.
EXPOSE 80

# Define the entry point for the container.
# This command will be executed when the container starts.
ENTRYPOINT ["dotnet", "chats96.Api.dll"]