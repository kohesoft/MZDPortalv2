# Build stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /app

# Copy the entire solution
COPY . .

# Restore NuGet packages
RUN nuget restore

# Build the project
RUN msbuild /p:Configuration=Release

# Final stage
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8
WORKDIR /inetpub/wwwroot

# Copy the build output from the build stage
COPY --from=build /app .

# Expose port 80
EXPOSE 80
