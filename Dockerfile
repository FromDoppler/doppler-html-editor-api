# cSpell: enableCompoundWords
FROM node:24 AS verify-format
WORKDIR /src
COPY package.json yarn.lock ./
RUN yarn
COPY . .
RUN yarn verify-format && yarn verify-spell

FROM koalaman/shellcheck-alpine:v0.11.0 as verify-sh
WORKDIR /src
COPY ./*.sh ./
RUN shellcheck -e SC1091,SC1090 ./*.sh

FROM mcr.microsoft.com/dotnet/sdk:7.0.410-bullseye-slim AS restore
WORKDIR /src
COPY ./*.sln ./
# Using `Doppler.` prefix to avoid docker confuse symlink with directories
# cSpell: disable-next-line
# "ERROR: error from sender: readdir: readdirent Jenkinsfile: not a directory"
COPY Doppler.*/*.csproj ./
# Take into account using the same name for the folder and the .csproj and only one folder level
RUN for file in *.csproj; do mkdir -p -- "${file%.*}/" && mv -- "$file" "${file%.*}/"; done
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet format -v diagnostic --verify-no-changes \
    && dotnet build -c Release

FROM build AS test
RUN dotnet test

FROM build AS publish
RUN dotnet publish "./Doppler.HtmlEditorApi/Doppler.HtmlEditorApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0.20-bullseye-slim AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ARG version=unknown
RUN echo $version > /app/wwwroot/version.txt
ENTRYPOINT ["dotnet", "Doppler.HtmlEditorApi.dll"]
LABEL name="doppler-html-editor-api" version="$version"
