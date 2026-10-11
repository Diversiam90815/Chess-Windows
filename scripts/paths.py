from pathlib import Path

ROOT_DIR                    = Path(__file__).resolve().parent.parent
ENGINE_DIR                  = ROOT_DIR / "Chess.Engine"
BUILD_DIR                   = ENGINE_DIR / "build"

UI_DIR                      = ROOT_DIR / "Chess.Game"
DIRECTORY_BUILD_PROPS_FILE  = UI_DIR / "Directory.Build.props"
PACKAGE_MANIFEST_FILE       = UI_DIR / "Package.appxmanifest"
GAME_PROJECT_FILE           = UI_DIR / "Chess.Game.csproj"
TEST_PROJECT_FILE           = ROOT_DIR / "Chess.Game.Test" / "Chess.Game.Test.csproj"

# The only engine target the app needs: the DLL it loads
ENGINE_API_TARGET           = "Chess.Engine.API"


def get_build_dir(architecture: str) -> Path:
    return BUILD_DIR / architecture
