# Chess Game

## Overview

This is a chess game developed in C++ and C# with the goal of creating a fully-featured chess application. The project is currently in development and aims to provide a seamless chess-playing experience with a modern user interface.

- **Backend**: C++
- **Frontend**:
  - **Windows**: WinUI 3 / C#


## Prerequisites

- **C++ Compiler**: Compatible with C++20 or higher.
- **CMake**: Version 3.15 or higher.
- **Git**: For cloning the repository.
- **.NET8**: For Windows App SDK / WinUi3
- **Visual Studio 2022 or higher**: With C++ Desktop Development workload.
- **Python**: Version 3.x (for running `build.py`).


## Getting Started

### Cloning the Repository

Clone the repository using the following command:

```bash
git clone git@github.com:Diversiam90815/Chess-Game.git
```

Keep in mind that you also need to check out the submodules with this project. To do so, you can include `--recurse-submodules` within the clone command (with git version 2.13 or higher):

```bash
git clone --recurse-submdules git@github.com:Diversiam90815/Chess-Game.git
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


##### Run the Application

After building, the executable will be located in the `build/<Configuration>` directory:

- **Windows**: `build/Release/ChessGame.exe`

Replace `<Configuration>` with `Release` or `Debug` depending on your build configuration.


#### Build Script Details (`build.py`)

The `build.py` script simplifies the build process:

- **Options**:
  - `--prepare` or `-p`: Generates build files using CMake.
  - `--build` or `-b`: Compiles the project.
  - `--debug` or `-d`: Sets the build configuration to Debug mode.


## Future Plans

- **Online Multiplayer**: Implement network capabilities for online play.


## Contributing

Contributions are welcome! Please follow these steps:

1. **Fork** the repository.

2. **Create** a new branch for your feature or bugfix:

   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Commit** your changes with clear messages:

   ```bash
   git commit -m "Add your feature"
   ```

4. **Push** to your branch:

   ```bash
   git push origin feature/your-feature-name
   ```

5. **Submit** a pull request to the `main` branch of this repository.

Please ensure your code follows the project's coding standards and includes appropriate documentation.


---

*Note: This project is currently under development. Features and plans may change as development progresses.*

---

Feel free to explore the repository and contribute to the project. Stay tuned for updates!
