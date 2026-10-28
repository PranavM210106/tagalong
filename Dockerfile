FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CACHEBUST=1
WORKDIR /src
COPY ["Tagalong.csproj", "."]
RUN dotnet restore "Tagalong.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Tagalong.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tagalong.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tagalong.dll"]
