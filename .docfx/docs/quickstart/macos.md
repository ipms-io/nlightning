# MacOS Users

To run the containerized tests we need to connect directly to the docker containers, but if you're using macOS you won't
be able to, thanks to the way Docker for Mac is implemented.

We're using [Docker Mac Net Connect](https://github.com/chipmk/docker-mac-net-connect) due to its simplicity. Just run:

```sh
# Install via Homebrew
$ brew install chipmk/tap/docker-mac-net-connect

# Run the service and register it to launch at boot
$ sudo brew services start chipmk/tap/docker-mac-net-connect
```