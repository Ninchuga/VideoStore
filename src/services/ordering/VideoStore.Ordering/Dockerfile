#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["services/ordering/VideoStore.Ordering/VideoStore.Ordering.csproj", "services/ordering/VideoStore.Ordering/"]
COPY ["shared/VideoStore.Bus/VideoStore.Bus.csproj", "shared/VideoStore.Bus/"]
COPY ["shared/VideoStore.Shared/VideoStore.Shared.csproj", "shared/VideoStore.Shared/"]
RUN dotnet restore "services/ordering/VideoStore.Ordering/VideoStore.Ordering.csproj"
COPY . .
WORKDIR "/src/services/ordering/VideoStore.Ordering"
RUN dotnet build "VideoStore.Ordering.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoStore.Ordering.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoStore.Ordering.dll"]