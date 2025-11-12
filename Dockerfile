# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["manage products/manage products.csproj", "manage products/"]
RUN dotnet restore "manage products/manage products.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/manage products"
RUN dotnet build "manage products.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "manage products.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose port (Render will set PORT environment variable at runtime)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
# Render automatically sets PORT, app will read it via ASPNETCORE_URLS
# We'll configure this in Program.cs or via Render's environment variables

# Run the app
ENTRYPOINT ["dotnet", "manage products.dll"]

