FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy the main project file and restore dependencies
COPY ["Sports Analysis/Sports Analysis.csproj", "Sports Analysis/"]
RUN dotnet restore "Sports Analysis/Sports Analysis.csproj"

# Copy the rest of the source and build
COPY . .
WORKDIR "/src/Sports Analysis"
RUN dotnet build "Sports Analysis.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sports Analysis.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sports Analysis.dll"]

