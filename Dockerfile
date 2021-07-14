FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./

RUN dotnet restore "./IoBTAdapterDotNet.csproj"

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim 
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "IoBTAdapterDotNet.dll"]


# docker build -t iobtweb .
# docker run -d -p 8080:80 --name IoBTAdapterDotNet iobtweb
# docker run --rm -it iobtweb:latest

# docker exec -t -i login1 /bin/bash