FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
#COPY *.sln .
#COPY OceanOfCode/*.csproj ./OceanOfCode/
#COPY OceanOfCode.Test/*.csproj ./OceanOfCode.Test/
#RUN dotnet restore

# copy everything else and build app
COPY . .
RUN dotnet build

FROM build AS testrunner
ENTRYPOINT ["dotnet", "test", "--logger:trx"]

FROM build AS test
RUN dotnet test

FROM build AS publish
WORKDIR /app/OceanOfCode
RUN dotnet publish -c Release -o out

#FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
#WORKDIR /app
#COPY --from=publish /app/OceanOfCode/out ./
#ENTRYPOINT ["OceanOfCode.exe"]