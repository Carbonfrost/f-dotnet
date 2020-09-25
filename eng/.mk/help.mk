_DISABLED_RUNTIMES = $(subst $(_SPACE),$(_PIPE),$(strip $(ENG_DISABLED_RUNTIMES)))

ifdef ALL
	_FILTER_DISABLED_TARGETS = | grep --color -E '(zzz|$(_DISABLED_RUNTIMES))/|$$' -
else
	_FILTER_DISABLED_TARGETS = | grep -vE '(zzz|$(_DISABLED_RUNTIMES))/' -
endif

## Show this help screen
help:
	@ awk '/^[^-][/a-zA-Z\-\_0-9]+:/ { \
		helpMessage = match(lastLine, /(.+)/); \
		if (helpMessage) { \
			helpCommand = substr($$1, 0, index($$1, ":")-1); \
			helpMessage = lastLine; \
			gsub(/##/, " ", helpMessage); \
			printf "  $(_CYAN)%-16s$(_RESET) %s\n", helpCommand, helpMessage; \
			lastLine = ""; \
		} \
	} \
	{ if (match($$0, /^##/)) { lastLine = lastLine $$0 } }' $(MAKEFILE_LIST) | \
		sort $(_FILTER_DISABLED_TARGETS)
