.PHONY: init init-brew-deps

## Initialize dependencies for developing the project
init: init-brew-deps
	@ echo "Done! 🍺"

## Install software for developing on macOS
init-brew-deps:
	@ if [ ! $(shell command -v "brew") ]; then \
		echo "Please install Homebrew first (https://brew.sh)"; \
		exit 1; \
	fi
	@ echo "Installing prerequisites and setting up the environment ..."
	@ brew update && \
		brew tap homebrew/bundle && \
		brew bundle && \
		direnv allow
