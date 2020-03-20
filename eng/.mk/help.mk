## Show this help screen
help:
	@ awk '/^[^-][/a-zA-Z\-\_0-9]+:/ { \
		helpMessage = match(lastLine, /(.+)/); \
		if (helpMessage) { \
			helpCommand = substr($$1, 0, index($$1, ":")-1); \
			helpMessage = lastLine; \
			gsub(/##/, "\n            ", helpMessage); \
			printf "  \033[36m%s\033[0m %s\n", helpCommand, helpMessage; \
			lastLine = ""; \
		} \
	} \
	{ if (match($$0, /^##/)) { lastLine = lastLine $$0 } }' $(MAKEFILE_LIST)
