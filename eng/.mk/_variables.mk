# Most command output is silenced by default unless VERBOSE is set
VERBOSE ?=

# Prefix controlling where software is installed
PREFIX ?= /usr/local

# Default target when `make` is run
ENG_DEFAULT_TARGET ?= build

# Provides a list of the enabled runtimes, derived from the ENG_USING_X variables
ENG_ENABLED_RUNTIMES +=

# Provides a list of the disabled runtimes, derived from the ENG_USING_X variables
ENG_DISABLED_RUNTIMES = $(filter-out $(ENG_ENABLED_RUNTIMES),$(ENG_AVAILABLE_RUNTIMES))

# Some variables that are globally interesting to examine in `make env`
ENG_GLOBAL_VARIABLES := \
	ENG_AVAILABLE_RUNTIMES \
	ENG_DEFAULT_TARGET \
	ENG_DISABLED_RUNTIMES \
	ENG_ENABLED_RUNTIMES \
	PATH \
	PREFIX \
	VERBOSE \
	BUILD_FIRST \

# Variables that are global but only show when VERBOSE is set
ENG_GLOBAL_VERBOSE_VARIABLES := \
	HOME \
	LANG \
	LC_CTYPE \
	TMPDIR \
	USER \
	DIRENV_DIR \

# ------- .NET settings
#

# Variables used by .NET settings
ENG_DOTNET_VARIABLES := \
	CONFIGURATION \
	ENG_DOTNET_DIR \
	ENG_USING_DOTNET \
	FRAMEWORK \
	NUGET_CONFIG_FILE \
	NUGET_PASSWORD \
	NUGET_SOURCE_URL \
	NUGET_UPLOAD_URL \
	NUGET_SOURCE_NAME \
	NUGET_USER_NAME

# Directory to use as root of a dotnet project.
ENG_DOTNET_DIR := ./dotnet

# The location of the NuGet configuration file
NUGET_CONFIG_FILE ?= ./nuget.config

# The configuration to build (probably "Debug" or "Release")
CONFIGURATION ?= Release

# The framework to publish
FRAMEWORK ?= netcoreapp3.0

# ------- Python settings
#

# Variables used by Python settings
ENG_PYTHON_VARIABLES := \
	ENG_USING_PYTHON \
	PIP \
	PYTHON \
	VIRTUAL_ENV \
	VIRTUAL_ENV_DISABLE_PROMPT \
	VIRTUAL_ENV_NAME \

# Whether we are meant to use Python.  (See python.mk for autodetection)
ENG_USING_PYTHON ?= $(ENG_AUTODETECT_USING_PYTHON)

# Name of the python executable
PYTHON ?= python3

# Name of the pip executable
PIP ?= pip3

# Name of the virtual environment
VIRTUAL_ENV_NAME ?= venv

# ------- Go settings
#

# Variables used by Python settings
ENG_GO_VARIABLES := \
	GOPATH \

# Whether we are meant to use Go.  (See go.mk for autodetection)
ENG_USING_GO ?= $(ENG_AUTODETECT_USING_GO)

# ------- Ruby settings
#

# Variables used by Ruby settings
ENG_RUBY_VARIABLES = \
	ENG_USING_RUBY \
	ENG_LATEST_RUBY_VERSION \
	RBENV_SHELL \

# Whether we are meant to use Ruby.  (See ruby.mk for autodetection)
ENG_USING_RUBY ?= $(ENG_AUTODETECT_USING_RUBY)

# Latest version of Ruby supported
ENG_LATEST_RUBY_VERSION = 2.6.0

# -------
#
# `chronic` is a tool from moreutils which can suppress output except when
# errors occur.   If chronic is available, then OUTPUT_COLLAPSED can be used
# to suppress output conditionally
_CHRONIC = $(shell command -v chronic 2> /dev/null)

ifneq (, $(VERBOSE))
Q =
OUTPUT_HIDDEN =
OUTPUT_COLLAPSED =
_STANDARD_VERBOSE_FLAG =
else
Q = @
OUTPUT_HIDDEN = >/dev/null 2>/dev/null
OUTPUT_COLLAPSED = $(or $(_CHRONIC),$(OUTPUT_HIDDEN))
_STANDARD_VERBOSE_FLAG = -v
endif

_DONE = echo "Done! 🍺" $(OUTPUT_HIDDEN)

# These variables are meant to be used internally

# Common escaped variables
_SPACE :=
_SPACE +=
_COMMA := ,
_PIPE := |

# Terminal output formatting
_RESET = \x1b[39m
_YELLOW = \x1b[33m
_GREEN = \x1b[32m
_RED = \x1b[31m
_MAGENTA = \x1b[35m
_UNDERLINE = \x1b[4m
_CYAN = \x1b[36m

_FATAL_ERROR = $(_RED)fatal: $(_RESET)
_WARNING = $(_YELLOW)warning: $(_RESET)
