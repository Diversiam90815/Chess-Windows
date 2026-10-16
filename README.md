# Chess (Windows)

## Overview

A Windows chess application built with WinUI 3 and C#, backed by a [C++20 chess engine](https://github.com/Diversiam90815/Chess-Engine). The UI, the engine and the networking live in separate repositories: the engine and [NetLink](https://github.com/Diversiam90815/NetLink) (LAN discovery and messaging) are consumed here as git submodules and called from C# via P/Invoke through their plain C APIs (DLLs). The engine only plays chess; finding an opponent on the network and passing moves back and forth is done by the app.

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
| Networking | C++23 - see [NetLink](https://github.com/Diversiam90815/NetLink) |
| Interop | P/Invoke via a plain C API (DLL) |
| Testing (C++) | GoogleTest + CTest |
| Testing (C#) | xUnit, Moq |

## Project Structure

```
Chess-Windows/
├── submodules/
│   ├── Chess.Engine/       # Git submodule — C++ chess engine
│   └── NetLink/            # Git submodule — LAN discovery and messaging
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
├── build.py                # Build entry point
├── build/                  # Native library builds (not committed)
└── install/                # Installed native libraries, per configuration (not committed)
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

`build.py` handles the full build in one step: it builds the two native libraries the app loads (`Chess.Engine.API` and `NetLinkC`) from the submodules via CMake into `build/`, installs them into `install/<configuration>/bin`, and then builds the C# application, which copies the DLLs from there.

This repository only consumes the libraries. Their tests, tools and code checks are run from their own repositories, and the build never writes into the submodules' sources. To build against another checkout of a library, pass `--engine-dir` or `--netlink-dir`.

**Release build:**

```bash
python build.py -b
```

**Debug build:**

```bash
python build.py -b -c Debug
```

For day-to-day development, open `Chess.sln` in Visual Studio after running `python build.py -p` once: it generates the native projects, which the solution contains (`Chess.Engine` and `NetLink` folders). Building the app builds the two libraries first and runs their `INSTALL` projects, so changes to the engine or NetLink reach the app without running `build.py` again. Run `build.py -p` again after updating a submodule, so the projects are regenerated.

## `build.py` Flag Reference

| Flag | Long form | Description |
|---|---|---|
| `-p` | `--prepare` | Build and install the native libraries in Debug and Release, for working in Visual Studio |
| `-b` | `--build` | Build and install the native libraries, then build the C# application |
| `-c CONFIG` | `--configuration` | `Debug`, `Release` (default), or `RelWithDebInfo` |
| `-a ARCH` | `--architecture` | `x64` (default) or `ARM64` |
| `-pl PLATFORM` | `--platform` | `VS2026` (default) or `VS2022`; the C# project needs a Visual Studio generator |
| `-t` | `--runtest` | Build and run the C# unit tests |
| | `--engine-dir DIR` | Source of the chess engine (default: `submodules/Chess.Engine`) |
| | `--netlink-dir DIR` | Source of NetLink (default: `submodules/NetLink`) |

## Testing

The C# tests run from Test Explorer in Visual Studio, or with:

```bash
python build.py -b -t -c Debug    # build the app, then build and run the tests
python build.py -t -c Debug       # build and run the tests only
```

Most tests are plain unit tests. `EngineIntegrationTests` load the engine DLL and check that the app and the engine agree on their interface.

## Future Plans

- Onboarding flow for new players
- Dynamic audio score

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
