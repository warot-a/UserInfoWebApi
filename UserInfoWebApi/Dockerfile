FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . /src
RUN dotnet restore "./UserInfoWebApi.csproj"
RUN dotnet build "./UserInfoWebApi.csproj" -c Release -o /app/build
RUN dotnet publish "./UserInfoWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UserInfoWebApi.dll"]