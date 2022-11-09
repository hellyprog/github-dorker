# github-dorker
Tool for automatic github dorking


## Installation
Note: you need `.net 6` installed to run this tool.
1. Clone a repository.
2. Run cmd in folder with `.csproj` file.
3. Execute `dotnet pack` command.
4. Run `dotnet tool install --global --add-source ./nupkg github-dorker`.

## Usage
`-h` - help, list of parameters\
`-df` - dork file path\
`-t` - github token\
`-org` - organization to dork\

`github-dorker -df C:\\medium_dorks.txt -t ghp_pxfJLpr38nDPyn92azfgeXCbqqaCV70Art5L -org Microsoft`
