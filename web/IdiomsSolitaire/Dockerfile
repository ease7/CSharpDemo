

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
WORKDIR /build
COPY . .
RUN dotnet restore IdiomsSolitaire.Blazor/IdiomsSolitaire.Blazor.csproj
RUN dotnet publish IdiomsSolitaire.Blazor/IdiomsSolitaire.Blazor.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
WORKDIR /app
EXPOSE 5000
COPY --from=publish /app .
ENV ASPNETCORE_URLS http://*:5000

ENTRYPOINT ["dotnet", "IdiomsSolitaire.Blazor.dll"]

