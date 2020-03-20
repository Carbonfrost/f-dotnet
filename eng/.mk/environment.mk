_DEV_MESSAGE=(Is direnv set up correctly?  Have you tried 'make init'?)
-check-command-%:
	@ if [ ! $(shell command -v "${*}" ) ]; then \
		echo "Command ${*} could not be found $(_DEV_MESSAGE)"; \
		exit 1; \
	fi

-check-env-%:
	@ if [ "${${*}}" = "" ]; then \
		echo "Environment variable ${*} not set $(_DEV_MESSAGE)"; \
		exit 1; \
	fi
