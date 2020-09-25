# Automatically detect whether Go is in use
ENG_AUTODETECT_USING_GO = $(shell [ ! -f go.mod ] ; echo $$?)
ENG_AVAILABLE_RUNTIMES += go

# User can define ENG_USING_GO themselves to avoid autodeteciton
ifdef ENG_USING_GO
_ENG_ACTUALLY_USING_GO = $(ENG_USING_GO)
else
_ENG_ACTUALLY_USING_GO = $(ENG_AUTODETECT_USING_GO)
endif

.PHONY: \
	-go/init \
	-go/build \
	-hint-unsupported-go \
	-use/go-mod \
	go/init \
	go/build \
	use/go \

## Add support for Go to the project
use/go: | -go/init -use/go-mod

fetch: go/get
build: go/build

# Enable the tasks if we are using Go
ifeq (1,$(ENG_USING_GO))
ENG_ENABLED_RUNTIMES += go

## Install Go and project dependencies
go/init: -go/init
go/build: -go/build
go/get: -go/get
else
go/init: -hint-unsupported-go
go/build: -hint-unsupported-go
go/get: -hint-unsupported-go
endif

-go/init:
	@    echo "$(_GREEN)Installing Go and Go dependencies...$(_RESET)"
	$(Q) $(OUTPUT_COLLAPSED) eng/brew_bundle_inject go
	$(Q) $(OUTPUT_COLLAPSED) brew bundle

-go/build:
	$(Q) go build

-go/get:
	$(Q) go get

-use/go-mod: -check-command-go
	$(Q) [ -f go.mod ] && $(OUTPUT_HIDDEN) go mod

-hint-unsupported-go:
	@ echo $(_HIDDEN_IF_BOOTSTRAPPING) "$(_WARNING) Nothing to do" \
		"because $(_MAGENTA)go$(_RESET) is not enabled (Investigate $(_CYAN)\`make use/go\`$(_RESET))"
