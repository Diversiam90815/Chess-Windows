from pathlib import Path

ROOT_DIR                    = Path(__file__).resolve().parent.parent
ENGINE_DIR                  = ROOT_DIR / "Chess.Engine"
CMAKE_FILE                  = ENGINE_DIR / "CMakeLists.txt"
BUILD_DIR                   = ENGINE_DIR / "build"
CMAKE_INSTALL_DIR           = ENGINE_DIR / "install"
TEST_BUILD_DIR              = BUILD_DIR / "tests"

UI_DIR                      = ROOT_DIR / "Chess.UI" / "Chess.UI"
DIRECTORY_BUILD_PROPS_FILE  = UI_DIR / "Directory.Build.Props"
PACKAGE_MANIFEST_FILE       = UI_DIR / "Package.appxmanifest"


def get_build_dir(architecture: str) -> Path:
    return BUILD_DIR / architecture
