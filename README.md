# Chess

## Overview

This is a chess game developed in C++ and C# with the goal of creating a fully-featured chess application. The project is currently in development and aims to provide a seamless chess-playing experience with a modern user interface.


## Features

-   **Modern User Interface**:
    -   A clean and intuitive interface built with the latest WinUI 3 framework for Windows.
    -   **High-DPI Support**: The UI automatically scales for a crisp and clear viewing experience on any display.

-   **Enhanced Gameplay Experience**:
    -   **Captured Pieces Display**: The UI keeps a visual tally of all captured pieces for both players.
    -   **Game Controls**: Easily undo your last move or reset the board to start a new game.
    -   **Immersive Audio**: A dynamic audio engine provides sound effects for moves and captures, along with ambient background tracks to enhance the atmosphere.

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


## Future Plans

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
    - **Chess.Engine.API**: A C++ DLL project that exposes the core engine functionalities through a C-style API, allowing the C# frontend to communicate with the C++ backend via P/Invoke.
    - **Chess.Engine.Core**: The core of the chess engine, written in C++. It includes all the game logic, such as the chessboard representation, move generation, validation, and the game state machine.
    - **Chess.Engine.Performance**: A project dedicated to performance testing of the C++ engine, helping to benchmark and optimize critical components like the CPU player's move evaluation.
    - **Chess.Engine.Tests**: Contains unit tests (GoogleTest) for the C++ engine, verifying the correctness and stability of the core game logic.


## Prerequisites

- **C++ Compiler**: Compatible with C++20 or higher.
- **CMake**: Version 3.15 or higher.
- **Git**: For cloning the repository.
- **.NET8**: For Windows App SDK / WinUi3
- **Visual Studio 2022**: With C++ Desktop Development workload.
- **Python**: Version 3.x (for running `build.py`).


## Getting Started

### Cloning the Repository

Clone the repository using the following command:

```bash
git clone git@github.com:Diversiam90815/Chess.git
```

Keep in mind that you also need to check out the submodules with this project. To do so, you can include `--recurse-submodules` within the clone command (with git version 2.13 or higher):

```bash
git clone --recurse-submdules git@github.com:Diversiam90815/Chess.git
```

or if you already cloned the repository call

```bash
git submodule update --init --recursive
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


## Testing

The project includes a suite of tests for both the backend and the frontend UI to ensure code quality and reliability.

- **Backend (Chess.Engine.Tests)**: The C++ engine is tested using the **GoogleTest** framework. Tests cover core functionalities such as move generation, validation and execution.
- **Frontend (Chess.UI.Tests)**: The C# UI and its view models are tested using **xUnit**, with **Moq** for creating mock objects.



## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
