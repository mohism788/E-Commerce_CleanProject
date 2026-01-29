FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 5172  # Match VS HTTP port
COPY --from=build /app/build .
# Tell app to use port 5172
ENV ASPNETCORE_URLS=http://+:5172
ENTRYPOINT ["dotnet", "E-Commerce.dll"]