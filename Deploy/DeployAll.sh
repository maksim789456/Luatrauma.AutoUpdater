#!/bin/sh

dotnet publish ../Luatrauma.AutoUpdater/Luatrauma.AutoUpdater.csproj -c Release --self-contained -r win-x64 -o bin/win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=True -p:TrimMode=Link
dotnet publish ../Luatrauma.AutoUpdater/Luatrauma.AutoUpdater.csproj -c Release --self-contained -r linux-x64 -o bin/linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=True -p:TrimMode=Link
dotnet publish ../Luatrauma.AutoUpdater/Luatrauma.AutoUpdater.csproj -c Release --self-contained -r osx-x64 -o bin/osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=True -p:TrimMode=Link