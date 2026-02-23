# dotnet-monorepo

A .NET tool for managing and working with large repositories and monorepos.

## Installing:

```shell
dotnet tool install dotnet-monorepo
```

## Features

### Selective CLI Command Execution

`dotnet monorepo` includes wrappers for select dotnet cli commands that call `dotnet affects` to determine projects that 
have been affected based on a git diff, and executes the command for only those projects, transparently passing 
through supplied arguments. 

```shell
dotnet monorepo build --from v0.3.0 --to v0.2.0 --verbosity minimum --configuration Release
```

Supported commands include `clean` `restore` `build` `pack` `publish` `test`

## Research


### Selective Version

### Solution Management

A monorepo tool in dotnet should include a mechanism for neatly generating solution files that capture 
the full dependency scope of a project or a set of projects and allow developers to conveniently access them 
without having to load all projects in the repository.

One existing solution for this is Solution filters, which are supported by the build system and major IDEs, 
however these have a few limitations:

1. They can end up a bit verbose, and it’s not clear if globbing is supported
1. They only do transient inclusion of dependencies for specified projects, not dependent projects

What would be ideal is a system for generating solution files for specific projects or groups of projects that 
included the project(s) and all their dependencies and dependents are included.

Generated solution files could use the extension .g.csproj and optionally (probably recommended) excluded 
from source control.

A cli command with “watch” functionality that ran in the background monitoring these files and watching for 
changes and dynamically updating solutions would be useful.

No good library exists for generating or modifying solution files like this. 
The best option is the dotnet cli, may have to borrow some code from Jig. 
