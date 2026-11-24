FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore FinanceMCP.csproj
RUN dotnet publish FinanceMCP.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

# Verify the DLL exists during build
RUN ls -la /app

CMD ["dotnet", "FinanceMCP.dll"]