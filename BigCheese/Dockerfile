FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build

WORKDIR /src/BlueCheese
COPY ./BlueCheese/BlueCheese.csproj .
RUN dotnet restore "./BlueCheese.csproj"
COPY ./BlueCheese .

WORKDIR /src/BigCheese
COPY ./BigCheese/BigCheese.csproj .
RUN dotnet restore "./BigCheese.csproj"
COPY ./BigCheese .

RUN dotnet build "BigCheese.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BigCheese.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BigCheese.dll"]