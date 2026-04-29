# Use the official .NET SDK image for development
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

# Copy the project file and restore dependencies
COPY ["Endpointer.Json.Remapping.csproj", "./"]
RUN dotnet restore "Endpointer.Json.Remapping.csproj"

# Copy the rest of the source code
COPY . .

# Expose the ports
EXPOSE 5134 7022

# Set environment to Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the app with watch for hot reload
CMD ["dotnet", "watch", "run", "--urls=http://0.0.0.0:5134;https://0.0.0.0:7022"]