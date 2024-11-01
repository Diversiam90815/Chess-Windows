# Chess Game

## Overview

This is a chess game developed in C++ with the goal of creating a fully-featured chess application. The project is currently in development and aims to provide a seamless chess-playing experience with a modern user interface.

- **Backend**: C++
- **Frontend (Planned)**:
  - **Windows**: WinUI 3 / C#
  - **macOS**: SwiftUI / Swift

## Features (Planned)

- **Multiplayer Mode**: Play against other players locally or online.
- **Modern UI**: Intuitive and responsive user interface using WinUI 3 or SwiftUI.
- **Chess Engine**: Implements standard chess rules and advanced move logic.
- **Save and Load Games**: Ability to save game states and resume later.
- **Undo/Redo Moves**: Navigate through move history.

## Prerequisites

- **C++ Compiler**: Compatible with C++20 or higher.
- **CMake**: Version 3.15 or higher.
- **Git**: For cloning the repository.

### For Windows Development

- **Visual Studio 2022 or higher**: With C++ Desktop Development workload.
- **WinUI 3**: [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/) 

### For macOS Development

- **Xcode 12 or higher**
- **SwiftUI** 

## Getting Started

### Cloning the Repository

Clone the repository using the following command:

```bash
git clone https://github.com/Diversiam90815/Chess-Game.git
```

### Building the Project (Backend only as of now)

The Chess Game project uses a `build.py` script to automate the build process.

#### Build Instructions

##### 1. Prepare the Build Environment

Navigate to the project directory and run:

```bash
python build.py --prepare
```

For a **Debug** build, include the `--debug` or `-d` option:

```bash
python build.py --prepare --debug
```

##### 2. Build the Project

To compile the project, use:

```bash
python build.py --build
```

Or combine preparation and building in one command:

```bash
python build.py --prepare --build
```

For a **Debug** build:

```bash
python build.py --prepare --build --debug
```

##### 3. Run the Application

After building, the executable will be located in the `build/<Configuration>` directory:

- **Windows**: `build/Release/ChessGame.exe`
- **macOS**: `build/Release/ChessGame`
- **Linux**: `build/Release/ChessGame`

Replace `<Configuration>` with `Release` or `Debug` depending on your build configuration.

**Note:** Since the GUI is still under development, running the application will currently not work.

#### Build Script Details (`build.py`)

The `build.py` script simplifies the build process:

- **Options**:
  - `--prepare` or `-p`: Generates build files using CMake.
  - `--build` or `-b`: Compiles the project.
  - `--debug` or `-d`: Sets the build configuration to Debug mode.

**Examples**:

- Prepare and build in **Release** mode:

  ```bash
  python build.py --prepare --build
  ```

- Prepare and build in **Debug** mode:

  ```bash
  python build.py --prepare --build --debug
  ```

This will compile the backend chess engine. Since the frontend is still under development, there is no executable UI application yet.

### Running Tests

*(To be implemented)*

## Future Plans

- **WinUI 3 Integration**: Develop a Windows application with a modern UI.
- **SwiftUI Integration**: Develop a macOS application with native SwiftUI.
- **Cross-Platform GUI**: Consider using a cross-platform GUI framework for simultaneous Windows and macOS development.
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

## License

This project is licensed under the [MIT License](LICENSE).

## Contact

For questions or feedback:

- **GitHub**: [Diversiam90815](https://github.com/Diversiam90815)
- **Website**: [www.diversiam.com](https://www.diversiam.com)

---

*Note: This project is currently under development. Features and plans may change as development progresses.*

---

Feel free to explore the repository and contribute to the project. Stay tuned for updates!
