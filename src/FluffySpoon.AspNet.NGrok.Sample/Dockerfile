#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["FluffySpoon.AspNet.NGrok.Sample/FluffySpoon.AspNet.NGrok.Sample.csproj", "FluffySpoon.AspNet.NGrok.Sample/"]
COPY ["FluffySpoon.AspNet.NGrok/FluffySpoon.AspNet.NGrok.csproj", "FluffySpoon.AspNet.NGrok/"]
RUN dotnet restore "FluffySpoon.AspNet.NGrok.Sample/FluffySpoon.AspNet.NGrok.Sample.csproj"
COPY . .
WORKDIR "/src/FluffySpoon.AspNet.NGrok.Sample"
RUN dotnet build "FluffySpoon.AspNet.NGrok.Sample.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluffySpoon.AspNet.NGrok.Sample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluffySpoon.AspNet.NGrok.Sample.dll"]
