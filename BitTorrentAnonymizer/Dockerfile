#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
ENV ASPNETCORE_ENVIRONMENT="Development"
ENV BitTorrentAnonymizer_AllowedHosts="*"
ENV BitTorrentAnonymizer_Logging__LogLevel__Default="Information"
ENV BitTorrentAnonymizer_Logging__LogLevel__Microsoft="Warning"
ENV BitTorrentAnonymizer_Logging__LogLevel__Microsoft.Hosting.Lifetime="Information"
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyProtocol="None"
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyHost=""
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyPort="0"
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyAuthentication="false"
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyUsername=""
ENV BitTorrentAnonymizer_ProxyConfiguration__ProxyPassword=""
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["BitTorrentAnonymizer/BitTorrentAnonymizer.csproj", "BitTorrentAnonymizer/"]
RUN dotnet restore "BitTorrentAnonymizer/BitTorrentAnonymizer.csproj"
COPY . .
WORKDIR "/src/BitTorrentAnonymizer"
RUN dotnet build "BitTorrentAnonymizer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BitTorrentAnonymizer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BitTorrentAnonymizer.dll"]
