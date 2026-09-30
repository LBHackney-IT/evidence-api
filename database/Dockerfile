FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Install dotnet-ef globally
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef --version 10.0.12

# Copy csproj and restore as distinct layers
COPY ./EvidenceApi/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./EvidenceApi ./
COPY ./.editorconfig ./
RUN dotnet build

CMD ["dotnet", "ef", "database", "update", "--no-build"]
