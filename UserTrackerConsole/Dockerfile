# Base stage (runtime environment)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Set default value for APP_CONFIG_FILE unless overridden
ARG APP_CONFIG_FILE=App.Live.config
ENV APP_CONFIG_FILE=${APP_CONFIG_FILE}

RUN echo "Base stage: APP_CONFIG_FILE=${APP_CONFIG_FILE}"

ENV TZ=Europe/Amsterdam

# Build stage (compiles the code)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_CONFIG_FILE  # Pass APP_CONFIG_FILE again in build stage
ENV APP_CONFIG_FILE=${APP_CONFIG_FILE}

WORKDIR /src
COPY ["UserTrackerConsole/UserTrackerConsole.csproj", "UserTrackerConsole/"]
RUN dotnet restore "./UserTrackerConsole/UserTrackerConsole.csproj"

COPY . .
WORKDIR "/src/UserTrackerConsole"

RUN echo "Build stage: APP_CONFIG_FILE=${APP_CONFIG_FILE}"
RUN dotnet build "./UserTrackerConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage (creates final output)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_CONFIG_FILE
ENV APP_CONFIG_FILE=${APP_CONFIG_FILE}

RUN echo "Publish stage: APP_CONFIG_FILE=${APP_CONFIG_FILE}"
RUN dotnet publish "./UserTrackerConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN echo "Final stage: APP_CONFIG_FILE=${APP_CONFIG_FILE}"

ENTRYPOINT ["dotnet", "UserTrackerConsole.dll"]
