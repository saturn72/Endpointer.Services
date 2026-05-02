FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl unzip \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

EXPOSE 5134 7022
ENV ASPNETCORE_ENVIRONMENT=Development