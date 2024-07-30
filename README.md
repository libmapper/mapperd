# mapperd
`mapperd` is a tool providing access to the [libmapper](https://github.com/libmapper/libmapper) API from sandboxed
environments, namely the browser.

## Prerequisites
- libmapper 2.4.8+
- .NET 8

## Running
Checkout the repo, then start the server:
```bash
$ dotnet run
```
By default the server will start listening on `http://localhost:5001`.

## Developers
See [developers.md](docs/developers.md)