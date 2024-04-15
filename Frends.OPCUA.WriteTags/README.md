# Frends.OPCUA.WriteTags
FRENDS Tasks for OPC UA Write Data operation

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.OPCUA/actions/workflows/WriteTags_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.OPCUA/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.OPCUA/Frends.OPCUA.WriteTags|main)

- [Installing](#installing)
- [License](#license)
- [Building](#building)
- [Contributing](#contributing)

Installing
==========

You can install the task via FRENDS UI Task view, by searching for packages.

License
=======
This project is licensed under the MIT License - see the LICENSE file for details

Building
========

Clone a copy of the repo

`git clone https://github.com/FrendsPlatform/Frends.OPCUA.git`

Restore dependencies

`dotnet restore`

Rebuild the project

`dotnet build`

Run tests

Create a reference OPC UA server docker:

`docker-compose up`

then run tests with

`dotnet test`

Create a nuget package

`dotnet pack --configuration Release`

Contributing
============
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!