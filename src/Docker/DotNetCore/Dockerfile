FROM microsoft/dotnet:2.1.302-sdk AS build-env
WORKDIR /app

COPY . ./

RUN dotnet publish ./GitVersionExe/GitVersionExe.csproj -c Release -o bin/Publish -f netcoreapp2.0

# Build runtime image
FROM microsoft/dotnet:2.1.2-runtime-alpine 
LABEL maintainers="GitTools Maintainers"

WORKDIR /app
COPY --from=build-env /app/GitVersionExe/bin/Publish/*.dll ./
COPY --from=build-env /app/GitVersionExe/bin/Publish/*.json ./
COPY --from=build-env /app/GitVersionExe/bin/Publish/runtimes/alpine-x64/. ./runtimes/alpine-x64/

# Prepare alpine needed dependencies
RUN set -ex; apk add --no-cache libssl1.0 libgit2

RUN mkdir /repo
VOLUME /repo

ENTRYPOINT ["dotnet", "GitVersion.dll", "/repo"]