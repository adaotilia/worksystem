# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY worksystem.csproj .
RUN dotnet restore worksystem.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish worksystem.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT
ENTRYPOINT ["dotnet", "worksystem.dll"]
