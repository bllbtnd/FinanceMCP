FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore FinanceMCP.csproj
RUN dotnet publish FinanceMCP.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app/publish .

# Make sure the DLL is present
RUN ls -la /app && test -f /app/FinanceMCP.dll

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "/app/FinanceMCP.dll"]