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

-   **Advanced Multiplayer**:
    -   **LAN Gaming**: Host or join games on your local network.
    -   **Automatic Discovery**: Automatically finds other players hosting games on the network.
    -   **Network Selection**: For users with multiple network connections, you can choose the specific network for multiplayer games, ensuring a stable connection.

-   **Personalization**:
    -   **Custom Themes**: Tailor the look of the game by choosing from different styles for the board and pieces.
    -   **Player Naming**: Set your own name for multiplayer sessions.


## Technology Stack

- **Backend (Chess.Engine)**: C++20, utilizing the asio library for networking
- **Frontend (Chess.UI)**: C# with .NET8 and WinUI 3 for the user interface
- **Build System**: CMake, with a Python script to automate the build process
- **Testing**:
   - **C++**: GoogleTest
   - **C#**: xUnit, Moq
- **Communication**: The C# frontend communicates with the C++ backend via P/Invoke


## Future Plans

- **CPU opponnent** 
   - Implement a chess algorithm to play against.
   - Implement move evaluation module
- **Sound Engine**
   - Implement a sound engine for playback of sound effects
   - Integrate dynamic a dynamic music score


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