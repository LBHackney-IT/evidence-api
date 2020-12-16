FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./nuget.config ./
COPY ./.env.example ./.env
COPY ./EvidenceApi.sln ./
COPY ./EvidenceApi/EvidenceApi.csproj ./EvidenceApi/
COPY ./EvidenceApi.Tests/EvidenceApi.Tests.csproj ./EvidenceApi.Tests/

RUN dotnet restore ./EvidenceApi/EvidenceApi.csproj
RUN dotnet restore ./EvidenceApi.Tests/EvidenceApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out EvidenceApi.Tests/EvidenceApi.Tests.csproj

CMD dotnet test
