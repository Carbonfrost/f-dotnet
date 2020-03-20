#
# Various commands for the Engineering platform itself
#
.PHONY: release/requirements eng/update

release/requirements:
	@ eng/release_requirements

eng_update_file:=$(shell mktemp)

eng/update: -download-eng-archive -check-command-curl
	@ (tar -xvf "$(eng_update_file)" --strip-components=1 'eng-commons-dotnet-master/eng/*'; \
		tar -xvf "$(eng_update_file)" --strip-components=2 'eng-commons-dotnet-master/integration/*'; \
	)

ifeq ($(ENG_DEV_UPDATE), 1)
-download-eng-archive:
	git archive --format=zip --prefix=eng-commons-dotnet-master/ --remote=file://$(HOME)/source/eng-commons-dotnet master -o $(eng_update_file)

else
-download-eng-archive: -check-command-curl
	curl -o "$(eng_update_file)" -sL https://github.com/Carbonfrost/eng-commons-dotnet/archive/master.zip

endif