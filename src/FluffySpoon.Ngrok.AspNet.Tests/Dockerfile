#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["FluffySpoon.AspNet.Ngrok.Tests/FluffySpoon.AspNet.Ngrok.Tests.csproj", "FluffySpoon.AspNet.Ngrok.Tests/"]
COPY ["FluffySpoon.AspNet.Ngrok/FluffySpoon.AspNet.Ngrok.csproj", "FluffySpoon.AspNet.Ngrok/"]
RUN ls
RUN dotnet restore "FluffySpoon.AspNet.Ngrok.Tests/FluffySpoon.AspNet.Ngrok.Tests.csproj"
COPY . .
WORKDIR "/src/FluffySpoon.AspNet.Ngrok.Tests"
ENTRYPOINT ["dotnet", "test"]
#CMD tail -f /dev/null