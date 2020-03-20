#
# A standard system for building a dotnet assembly.  This should be included from the
# main Makefile
#
# Structure of the repository should be:
#
#    dotnet
#    ├── Build.props   -- (Optional) Shared build attributes controlling version, etc.
#    ├── solution.sln
#    ├── src
#    │   └── Project1
#    │       ├── Automation  -- (Optional) String resources, text templates, etc.
#    │       │   ├── SR.properties
#    │       │   └── TextTemplates
#    │       ├── Project1.csproj
#    │       └── (source files)
#    └── test
#        └── Project1.UnitTests
#            ├── Project1.UnitTests.csproj
#            ├── Content    -- (Optional) Fixture files
#            │   └── ...
#            └── (source files)
#
.PHONY: dotnet/restore dotnet/build dotnet/test dotnet/publish dotnet/push

# The configuration to build (probably "Debug" or "Release")
CONFIGURATION?=Release

# The location of the NuGet configuration file
NUGET_CONFIG_FILE?=./nuget.config

## Set up dotnet configuration for NuGet
dotnet/configure: -requirements-dotnet -check-env-NUGET_SOURCE_URL -check-env-NUGET_PASSWORD -check-env-NUGET_USER_NAME -check-env-NUGET_CONFIG_FILE
	@ test -e $(NUGET_CONFIG_FILE) || echo "<configuration />" > $(NUGET_CONFIG_FILE)
	@ nuget sources add -Name "Carbonfrost" \
		-Source $(NUGET_SOURCE_URL) \
		-Password $(NUGET_PASSWORD) \
		-Username $(NUGET_USER_NAME) \
		-StorePasswordInClearText \
		-ConfigFile $(NUGET_CONFIG_FILE)

## Restore package dependencies
dotnet/restore: -requirements-dotnet
	@ dotnet restore ./dotnet

## Build the dotnet solution
dotnet/build: dotnet/restore -dotnet/build

## Pack the dotnet build into a NuGet package
dotnet/pack: dotnet/build -dotnet/pack

## Push the dotnet build into package repository
dotnet/push: dotnet/pack -dotnet/push

## Executes dotnet publish
dotnet/publish: dotnet/build -dotnet/publish

## Executes dotnet clean
dotnet/clean:
	@ rm -rdf dotnet/{src,test}/*/{bin,obj}/*

-dotnet/build: -requirements-dotnet -check-env-CONFIGURATION
	@ eval $(shell eng/build_env); \
		dotnet build --configuration $(CONFIGURATION) --no-restore ./dotnet

-dotnet/pack: -requirements-dotnet -check-env-CONFIGURATION
	@ eval $(shell eng/build_env); \
		dotnet pack --configuration $(CONFIGURATION) --no-build ./dotnet

-dotnet/publish: -requirements-dotnet -check-env-CONFIGURATION
	@ eval $(shell eng/build_env); \
		dotnet publish --configuration $(CONFIGURATION) --no-build ./dotnet

# Nuget CLI doesn't work with GitHub package registry for some reason, so we're using a curl directly
-dotnet/push: -requirements-dotnet -check-env-NUGET_PASSWORD -check-env-NUGET_USER_NAME -check-env-NUGET_UPLOAD_URL
	@ for f in dotnet/src/*/bin/Release/*.nupkg; do \
		curl -X PUT -u "$(NUGET_USER_NAME):$(NUGET_PASSWORD)" -F package=@$$f $(NUGET_UPLOAD_URL); \
	done

-requirements-dotnet: -check-command-dotnet
