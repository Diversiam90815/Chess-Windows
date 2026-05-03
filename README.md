# Chess (Windows)

## Overview

A Windows chess application built with WinUI 3 and C#, backed by a [C++20 chess engine](https://github.com/Diversiam90815/Chess-Engine). The UI and engine live in separate repositories: the engine is consumed here as a git submodule and exposed to the C# layer via P/Invoke through a plain C API (DLL).

## Features

- **Modern UI**: Built with WinUI 3 (Windows App SDK), targeting Windows 10/11.
- **Game Controls**: Undo the last move or reset the board at any point.
- **Captured Pieces Display**: Visual tally of captured pieces for both players.
- **Personalization**: Choose board and piece styles, set a player name for multiplayer sessions, and adjust SFX, atmosphere, and master volume independently.

## Technology Stack

| Layer | Technology |
|---|---|
| UI | C# / .NET 8, WinUI 3 (Windows App SDK 1.7) |
| UI architecture | MVVM (CommunityToolkit.Mvvm), dependency injection |
| Engine | C++20 - see [Chess-Engine](https://github.com/Diversiam90815/Chess-Engine) |
| Interop | P/Invoke via a plain C API (DLL) |
| Testing (C++) | GoogleTest + CTest |
| Testing (C#) | xUnit, Moq |

## Project Structure

```
Chess-Windows/
├── Chess.Engine/           # Git submodule — C++ chess engine
├── Chess.Game/             # WinUI3 C# application
│   ├── Services/           # P/Invoke bindings, game service, communication layer
│   ├── ViewModels/         # MVVM view models
│   ├── Views/              # XAML pages (game, home, settings, shell)
│   ├── Models/             # Board, captured pieces, move history models
│   ├── Styles/             # Board and piece style loading
│   └── Chess.Game.csproj
├── Chess.Game.Test/        # xUnit tests for the C# layer
├── Chess.sln               # Visual Studio solution
├── scripts/                # Python build automation modules
└── build.py                # Build entry point
```

## Prerequisites

- **Visual Studio 2022** (17.x) or later, with the **.NET desktop development** and **Desktop development with C++** workloads
- **.NET 8 SDK**
- **Windows App SDK 1.7**
- **Windows 11 SDK** (10.0.26100 or later)
- **CMake** 4.0 or higher
- **Python** 3.x
- **Git** — required for submodule initialization and build number derivation

## Getting Started

### 1. Clone

```bash
git clone --recurse-submodules git@github.com:Diversiam90815/Chess-Windows.git
cd Chess-Windows
```

If you already cloned without `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

### 2. Build

`build.py` handles the full build in one step: it builds the C++ engine submodule via CMake, then builds the C# application via `dotnet build`.

**Release build:**

```bash
python build.py -b
```

**Debug build:**

```bash
python build.py -b -c Debug
```

For day-to-day development, opening `Chess.sln` in Visual Studio is fine — just run `python build.py -b` once first so the engine DLL is in place.

## `build.py` Flag Reference

| Flag | Long form | Description |
|---|---|---|
| `-p` | `--prepare` | CMake configure only (no compile) |
| `-b` | `--build` | Build engine and C# application |
| `-c CONFIG` | `--configuration` | `Debug`, `Release` (default), or `RelWithDebInfo` |
| `-a ARCH` | `--architecture` | `x64` (default) or `ARM64` |
| `-pl PLATFORM` | `--platform` | `Ninja` (default), `VS2022`, or `VS2026` |
| `-t` | `--runtest` | Run C++ unit tests via CTest |

## Testing

**C++ engine tests:**

```bash
python build.py -b -t    # build then run
python build.py -t       # run against existing build
```

**C# tests** — from Test Explorer in Visual Studio, or:

```bash
dotnet test Chess.Game.Test/Chess.UI.Test.csproj
```

## Future Plans

- Onboarding flow for new players
- Dynamic audio score

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
