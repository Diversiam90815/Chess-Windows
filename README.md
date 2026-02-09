# Chess

## Overview

This is a chess game developed in C++ and C# with the goal of creating a fully-featured chess application. The project is currently in development and aims to provide a seamless chess-playing experience with a modern user interface.

## Disclaimer:
**Currently the project goes through a restructuring of the engine in order to support bitboards.**

## Features

-   **Modern User Interface**:
    -   A clean and intuitive interface built with the latest WinUI 3 framework for Windows.

-   **Enhanced Gameplay Experience**:
    -   **Captured Pieces Display**: The UI keeps a visual tally of all captured pieces for both players.
    -   **Game Controls**: Easily undo your last move or reset the board to start a new game.

-   **Intelligent CPU Opponent**:
    -   **Play Against the AI**: Challenge yourself in a single-player mode against a computer-controlled opponent.
    -   **Adjustable Difficulty**: Choose from multiple difficulty levels, from random moves to advanced strategies using Minimax with Alpha-Beta pruning.
    -   **Flexible Setup**: Decide whether to play as White or Black when starting a game against the CPU.
    -   **Performance Optimizations**: The engine uses techniques like transposition tables to ensure responsive and intelligent AI gameplay.

-   **Advanced Multiplayer**:
    -   **LAN Gaming**: Host or join games on your local network.
    -   **Automatic Discovery**: Automatically finds other players hosting games on the network.
    -   **Network Selection**: For users with multiple network connections, you can choose the specific network for multiplayer games, ensuring a stable connection.

-   **Personalization**:
    -   **Custom Styles**: Tailor the look of the game by choosing from different styles for the board and pieces.
    -   **Player Naming**: Set your own name for multiplayer sessions.
    -   **Audio Controls**: Independently adjust the volume for sound effects, atmosphere, and the master output.


## Technology Stack

- **Backend (Chess.Engine)**: C++20, utilizing the asio library for networking
- **Frontend (Chess.UI)**: C# with .NET8 and WinUI 3 for the user interface
- **Build System**: CMake, with a Python script to automate the build process
- **Testing**:
   - **C++**: GoogleTest
   - **C#**: xUnit, Moq
- **Communication**: The C# frontend communicates with the C++ backend via P/Invoke
- **Developer Tooling** (optional, CMake-integrated):
   - **Doxygen** with [doxygen-awesome-css](https://jothepro.github.io/doxygen-awesome-css/) for documentation
   - **CppCheck** for static analysis
   - **Clang-Format** for automatic source formatting

## Future Plans
- **Currently the project goes through a restructuring of the engine in order to support bitboards**
- **CPU Evaluation Algorithms** 
   - Refine and improve positional and move evaluation algorithms for different CPU difficulties
- **Onboarding** 
   - Create onboarding process
- **Create score**
   - Enhance the sound engine for playback of dynamic score
   - Compose dynamic score


## Project Structure

- **Chess.UI**:
    - **Chess.UI**: The main WinUI 3 project containing the user interface, view models, and services for the application.
    - **Chess.UI.Test**: Contains unit tests (xUnit, Moq) for the `Chess.UI` project to ensure the reliability of the frontend logic.
- **Chess.Engine**:
    - **cmake**: Containing several cmake modules used in this project.
    - **src**:
        - **Chess.Engine.API**: A C++ DLL project that exposes the core engine functionalities through a C-style API, allowing the C# frontend to communicate with the C++ backend via P/Invoke.
        - **Chess.Engine.Core**: The core of the chess engine, written in C++. It includes all the game logic, such as the chessboard representation, move generation, validation, and the game state machine.
    - **tests**:
        - **Core.Tests**: Contains unit tests (GoogleTest) for the C++ core engine, verifying the correctness and stability of the game logic.


## Prerequisites

- **C++ Compiler**: Compatible with C++20 or higher.
- **CMake**: Version 3.15 or higher.
- **Git**: For cloning the repository.
- **.NET8**: For Windows App SDK / WinUi3
- **Visual Studio 2022**: With C++ Desktop Development workload.
- **Python**: Version 3.x (for running `build.py`).


### Optional CMake Modules

The following developer tools are integrated into the CMake build.  
They are **optional**, controlled by CMake options, and can be enabled/disabled explicitly.  
If enabled but the tool is missing, CMake will warn or fail depending on the module.

- **Doxygen** (`ENABLE_DOXYGEN`):  
  Generates HTML documentation from source code.  
  - Install: [Doxygen](https://www.doxygen.nl/download.html) and optionally [Graphviz](https://graphviz.org/download/) for diagrams.
  
  - Turn off in top-level `CMakeLists.txt`:
    ```cmake
    set(ENABLE_DOXYGEN OFF)
    ```

- **CppCheck** (`ENABLE_CPPCHECK`):  
  Runs [CppCheck](http://cppcheck.sourceforge.net/) for static analysis.  
  - Install (Windows with winget):  
    ```bash
    winget install cppcheck
    ```
  - Disable in CMake:
    ```cmake
    set(ENABLE_CPPCHECK OFF)
    ```

- **Clang-Format** (`ENABLE_FORMAT`):  
  Automatically formats C++ sources using `.clang-format`.  
  - Install (Windows with winget):  
    ```bash
    winget install llvm
    ```
    (which includes `clang-format`)

  - Disable in CMake:
    ```cmake
    set(ENABLE_FORMAT OFF)
    ```

Each module is guarded by its own CMake option, so builds will not fail if the tool is missing and the option is explicitly set to `OFF`.
Under Build


## Getting Started

### Cloning the Repository

Clone the repository using the following command:

```bash
git clone git@github.com:Diversiam90815/Chess.git
```

### Building the Project

The Chess Game project uses a `build.py` script to automate the build process.

#### Build Instructions

##### Prepare the Build Environment

Navigate to the project directory and run:

```bash
python build.py -p
```

For a **Debug** build, include the `--debug` or `-d` option:

```bash
python build.py -pd
```

##### Build the Project

The build preperation is included within the build process of the application. So, to build the project, you can directly call

Debug build:

```bash
python build.py -bd
```

Release build:

```bash
python build.py -b
```


### Build Script Details (`build.py`)

The `build.py` script simplifies the build process:

- **Options**:
  - `--prepare` or `-p`: Generates build files using CMake.
  - `--build` or `-b`: Compiles the project.
  - `--debug` or `-d`: Sets the build configuration to Debug mode.
  - `--test` or `-t`: Runs the C++ unit tests (via CTest).
  - `--docs`: Generates Doxygen documentation (opens in your default browser).


## Testing

The project includes a suite of tests for both the backend and the frontend UI to ensure code quality and reliability.

- **Backend (Chess.Engine.Tests)**: The C++ engine is tested using the **GoogleTest** framework. Tests cover core functionalities such as move generation, validation and execution.
- **Frontend (Chess.UI.Tests)**: The C# UI and its view models are tested using **xUnit**, with **Moq** for creating mock objects.


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
