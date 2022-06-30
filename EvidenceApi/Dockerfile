FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./EvidenceApi/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./EvidenceApi ./
COPY ./.editorconfig ./
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/EvidenceApi.dll
