# Stage 1: Build the Angular application
FROM node:18-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json and install dependencies
COPY package.json package-lock.json ./
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the Angular application for production
# Adjust 'chats96App' to your Angular project's name as defined in angular.json "projects" section
RUN npm run build --output-path=./dist --configuration=production

# Stage 2: Serve the application with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html # Copy the built files to Nginx's serving directory
EXPOSE 80 # Nginx runs on port 80 inside the container
CMD ["nginx", "-g", "daemon off;"]