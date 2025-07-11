#!/bin/bash

# Script to create EF Core migrations using Docker
# This will create migration files that can be committed to Git

echo "Creating Entity Framework migrations using Docker..."

# Navigate to the project directory
cd chats96.Api

# Run dotnet ef migrations add using Docker
docker run --rm -v "$(pwd)":/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
    # Install EF Core tools
    dotnet tool install --global dotnet-ef --version 8.0.0
    export PATH=\"\$PATH:/root/.dotnet/tools\"
    
    # Add migration
    dotnet ef migrations add InitialCreate --output-dir Migrations
    
    echo 'Migrations created successfully!'
    ls -la Migrations/
"

echo "Migration files created. You can now commit these to Git."
echo "When deployed via Portainer, the migrations will be applied automatically on startup."
