# Quick Start

This section will guide you through getting a copy of NLightning up and running on your local machine for development and testing purposes. 

## Prerequisites

Before you begin, ensure you have the following installed on your system:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git (for cloning the repository)

## Installation

1. **Clone the repository**

First, clone the NLightning repository to your local machine using Git:

```sh
git clone https://github.com/ipms-io/nlightning.git
cd nlightning
```

2. **Build the project**

Navigate to the project directory and build the project using the .NET CLI to ensure all dependencies are properly installed:

```sh
dotnet build
```

3. **Contribute to Development**

As NLightning is currently under active development, it may not be in a runnable state just yet. However, this opens up a great opportunity for you to contribute. Whether it's implementing new features, fixing bugs, or improving documentation, your contributions are invaluable to making NLightning fully operational.

To start contributing:

- Explore the [Issues](https://github.com/ipms-io/nlightning/issues) section on GitHub to find out what needs to be worked on.
- Review our [Contributing Guidelines](CONTRIBUTING.md) for details on making contributions, such as how to create pull requests.
- If you have a new idea or feature you'd like to work on, don't hesitate to open a new issue to discuss it with the project maintainers.

We encourage you to dive into the codebase, familiarize yourself with the project structure, and see where your skills and interests can help drive NLightning forward.

## Testing

To verify that everything is set up correctly, you can run the included unit tests:

```sh
dotnet test
```

## MacOS Users

To run the containerized tests we need to connect directly to the docker containers, but if you're using MacOS you won't be able to, thanks to the way Docker for Mac is implemented.

We're using [Docker Mac Net Connect](https://github.com/chipmk/docker-mac-net-connect) due to it's simplicity. Just run:

```sh
# Install via Homebrew
$ brew install chipmk/tap/docker-mac-net-connect

# Run the service and register it to launch at boot
$ sudo brew services start chipmk/tap/docker-mac-net-connect
```