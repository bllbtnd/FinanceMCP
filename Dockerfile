FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore FinanceMCP.csproj
RUN dotnet publish FinanceMCP.csproj -c Release -o /app/publish --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Run the executable directly
ENTRYPOINT ["/app/FinanceMCP"]