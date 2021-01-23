#!/bin/sh
API_KEY = $1

dotnet nuget push ./Libs/YY.DBTools.Core/bin/Release/YY.DBTools.Core.*.nupkg -k $1 -s https://api.nuget.org/v3/index.json --skip-duplicate
dotnet nuget push ./Libs/YY.DBTools.SQLServer.XEvents/bin/Release/YY.DBTools.SQLServer.XEvents.*.nupkg -k $1 -s https://api.nuget.org/v3/index.json --skip-duplicate
dotnet nuget push ./Libs/YY.DBTools.SQLServer.XEvents.ToClickHouse/bin/Release/YY.DBTools.SQLServer.XEvents.ToClickHouse.*.nupkg -k $1 -s https://api.nuget.org/v3/index.json --skip-duplicate