# Frends.Opcua.Write

Task for writing data to OPCUA Server

[![Read_build](https://github.com/FrendsPlatform/Frends.Opcua/actions/workflows/Write_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.Opcua/actions/workflows/Write_test_on_main.yml)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.Opcua/Frends.Opcua.Write|main)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.Opcua.git`

### Build the project

`dotnet build`

### Run tests

`cd Frends.Opcua.Write.Tests`

`./generate-certs.sh`

`docker-compose up -d`

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`

### StyleCop.Analyzers Version
This project uses StyleCop.Analyzers 1.2.0-beta.556, as recommended by the author, to get the latest fixes and improvements not available in the last stable release.
