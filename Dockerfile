# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /Code/ODataBatching8
RUN apt-get -y update && apt-get -y upgrade
RUN apt-get -y install wget

# copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /Code/ODataBatching8
COPY ["ODataBatching8/ODataBatching8.csproj", "ODataBatching8/"]
COPY . .
RUN dotnet restore "ODataBatching8/ODataBatching8.csproj"

WORKDIR "/Code/ODataBatching8"
# WORKDIR /Code/ODataBatching8
RUN dotnet build "ODataBatching8/ODataBatching8.csproj" -c Release -o /build

FROM build AS publish
RUN dotnet publish "ODataBatching8/ODataBatching8.csproj" -c RELEASE -o /publish

# final stage/image
FROM base AS final
WORKDIR /publish
EXPOSE 8080
EXPOSE 443
EXPOSE 1433
COPY --from=publish /publish .
ENTRYPOINT ["dotnet", "ODataBatching8.dll"]