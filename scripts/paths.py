from dataclasses import dataclass, field
from pathlib import Path

ROOT_DIR                    = Path(__file__).resolve().parent.parent
# The native libraries the app loads, as git submodules
SUBMODULES_DIR              = ROOT_DIR / "submodules"
ENGINE_DIR                  = SUBMODULES_DIR / "Chess.Engine"
NETLINK_DIR                 = SUBMODULES_DIR / "NetLink"

# The native libraries are installed here, one folder per configuration. The C# project takes them from <config>/bin.
INSTALL_DIR                 = ROOT_DIR / "install"

# The native libraries are built here, apart from the build folders of their own repositories
BUILD_DIR                   = ROOT_DIR / "build"

UI_DIR                      = ROOT_DIR / "Chess.Game"
DIRECTORY_BUILD_PROPS_FILE  = UI_DIR / "Directory.Build.props"
PACKAGE_MANIFEST_FILE       = UI_DIR / "Package.appxmanifest"
GAME_PROJECT_FILE           = UI_DIR / "Chess.Game.csproj"
TEST_PROJECT_FILE           = ROOT_DIR / "Chess.Game.Test" / "Chess.Game.Test.csproj"


@dataclass
class NativeDependency:
    """A native library the app loads, built from its own repository and installed with CMake."""

    name: str
    # Folder name below build/
    key: str
    source_dir: Path
    # The only target the app needs: the shared library it loads
    target: str
    cmake_options: list[str] = field(default_factory=list)
    # Install only this component; None installs everything the project installs
    install_component: str | None = None

    def build_dir(self, architecture: str) -> Path:
        return BUILD_DIR / self.key / architecture


def native_dependencies(engine_dir: Path = ENGINE_DIR, netlink_dir: Path = NETLINK_DIR) -> list[NativeDependency]:
    return [
        NativeDependency(
            name="Chess Engine",
            key="ChessEngine",
            source_dir=engine_dir,
            target="Chess.Engine.API",
            cmake_options=[
                # The engine's own code checks would rewrite and scan the submodule's sources
                "-DENABLE_FORMAT=OFF",
                "-DENABLE_CPPCHECK=OFF",
                "-DENABLE_DOXYGEN=OFF",
            ],
            install_component="ChessEngine",
        ),
        NativeDependency(
            name="NetLink",
            key="NetLink",
            source_dir=netlink_dir,
            target="NetLinkC",
            cmake_options=[
                "-DNETLINK_BUILD_C_API=ON",
                "-DNETLINK_BUILD_TESTS=OFF",
                "-DNETLINK_BUILD_BENCHMARKS=OFF",
            ],
        ),
    ]


def get_install_dir(configuration: str) -> Path:
    return INSTALL_DIR / configuration
