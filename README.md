# DirectXHook
A tool for logging models so that the information can later be used in ESPs or simple wallhacks

## Building
Requirements:
- Windows, x64
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- The Visual Studio "Desktop development with C++" workload, which Native AOT needs to compile the hook

Build the solution with Visual Studio or `dotnet build DirectXHook.sln`. Building the UI also publishes the hook
as a native `Hook.dll` next to `UI.exe` (the Hook project uses Native AOT, since it runs inside the target process).

Only 64-bit Direct3D 11 processes are supported.

Saved models are stored as JSON in `<process name>.json` in the working directory. Files saved by older
versions are not read.
