FROM microsoft/dotnet:2.1.401-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY wait-time.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM microsoft/dotnet:2.1.4-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "wait-time.dll"]