FROM mcr.microsoft.com/dotnet/sdk:11.0
WORKDIR /app

COPY . .
RUN dotnet restore FinanceMCP.csproj
RUN dotnet publish FinanceMCP.csproj -c Release -o /app/out

WORKDIR /app/out
EXPOSE 8080

CMD ["dotnet", "FinanceMCP.dll"]