FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./DocumentsApi.sln ./
COPY ./DocumentsApi/DocumentsApi.csproj ./DocumentsApi/
COPY ./DocumentsApi.Tests/DocumentsApi.Tests.csproj ./DocumentsApi.Tests/

RUN dotnet restore ./DocumentsApi/DocumentsApi.csproj
RUN dotnet restore ./DocumentsApi.Tests/DocumentsApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out DocumentsApi.Tests/DocumentsApi.Tests.csproj

CMD dotnet test
