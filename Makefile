.PHONY: dotnet/test dotnet/cover

## Generate generated code
dotnet/generate:
	srgen -c Carbonfrost.Commons.DotNet.Resources.SR \
		-r Carbonfrost.Commons.DotNet.Automation.SR \
		--resx \
		dotnet/src/Carbonfrost.Commons.DotNet/Automation/SR.properties

## Execute dotnet unit tests
dotnet/test: dotnet/publish -dotnet/test

-dotnet/test:
	fspec -i dotnet/test/Carbonfrost.UnitTests.DotNet/Content \
		-i dotnet/test/Carbonfrost.UnitTests.DotNet.Documentation/Content \
		dotnet/test/Carbonfrost.UnitTests.DotNet/bin/$(CONFIGURATION)/netcoreapp3.0/publish/Carbonfrost.UnitTests.DotNet.dll

## Run unit tests with code coverage
dotnet/cover: dotnet/publish -check-command-coverlet
	coverlet \
		--target "make" \
		--targetargs "-- -dotnet/test" \
		--format lcov \
		--output lcov.info \
		--exclude-by-attribute 'Obsolete' \
		--exclude-by-attribute 'GeneratedCode' \
		--exclude-by-attribute 'CompilerGenerated' \
		dotnet/test/Carbonfrost.UnitTests.DotNet/bin/$(CONFIGURATION)/netcoreapp3.0/publish/Carbonfrost.UnitTests.DotNet.dll

-include eng/.mk/*.mk
