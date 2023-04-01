#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["services/movies/VideoStore.Movies/VideoStore.Movies.csproj", "services/movies/VideoStore.Movies/"]
RUN dotnet restore "services/movies/VideoStore.Movies/VideoStore.Movies.csproj"
COPY . .
WORKDIR "/src/services/movies/VideoStore.Movies"
RUN dotnet build "VideoStore.Movies.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoStore.Movies.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoStore.Movies.dll"]