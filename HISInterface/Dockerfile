#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
CMD ["dotnet", "HISInterface.dll"]

#设置时区（部署docker必须加上去）
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
 

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HISInterface/HISInterface.csproj", "HISInterface/"]
RUN dotnet restore "HISInterface/HISInterface.csproj"
COPY . .
WORKDIR "/src/HISInterface"
RUN dotnet build "HISInterface.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HISInterface.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HISInterface.dll"]