from pathlib import Path

from .enums import Architecture, Configuration, Platform
from .utils import BuildUtils
from .versioning import VersionManager
from .paths import *


class BuildRunner:
    """
    Builds the Chess Windows app. The engine submodule is only built as far as the app needs it:
    its API library. Engine tests, tools and code checks are run from the engine repository.
    """

    def __init__(self, engine_dir: Path, build_dir: Path) -> None:
        self.engine_dir = engine_dir
        self.build_dir = build_dir

        self.version: str | None = None

        self.version_manager = VersionManager(DIRECTORY_BUILD_PROPS_FILE, PACKAGE_MANIFEST_FILE)

    # ---- Versioning ----
    def update_app_version(self) -> None:
        self.version = self.version_manager.update_build_number_in_version()


    # ---- Engine library ----
    def generate_cmake_project(self, platform: Platform, architecture: Architecture) -> None:
        prepare_cmd = [
            "cmake",
            "-G", str(platform),
            "-S", str(self.engine_dir),
            "-B", str(self.build_dir),
            # The engine's own code checks would rewrite and scan the submodule's sources
            "-DENABLE_FORMAT=OFF",
            "-DENABLE_CPPCHECK=OFF",
            "-DENABLE_DOXYGEN=OFF",
        ]
        if platform == Platform.VS2022 or platform == Platform.VS2026:
            prepare_cmd += ["-A", str(architecture)]

        BuildUtils.execute_command(
            prepare_cmd,
            f"CMake: Generate {platform} project for the engine",
        )


    def build_engine_library(self, configuration: Configuration) -> None:
        BuildUtils.execute_command(
            [
                "cmake",
                "--build", str(self.build_dir),
                "--config", str(configuration),
                "--target", ENGINE_API_TARGET,
                "--parallel", "8",
            ],
            f"CMake: Build {ENGINE_API_TARGET} ({configuration})",
        )


    def create_build_generator(self, platform: Platform, architecture: Architecture, configuration: Configuration) -> None:
        self.generate_cmake_project(platform=platform, architecture=architecture)
        self.build_engine_library(configuration)


    def prepare_cmake_project(self, platform: Platform, architecture: Architecture) -> None:
        self.generate_cmake_project(platform=platform, architecture=architecture)

        # Both configurations, so the solution can be built either way in the IDE
        self.build_engine_library(Configuration.Release)
        self.build_engine_library(Configuration.Debug)


    # ---- C# app ----
    @staticmethod
    def _csharp_configuration(configuration: Configuration) -> str:
        # MSBuild only supports Debug and Release; treat RelWithDebInfo as Release
        return "Release" if configuration == Configuration.RelWithDebInfo else str(configuration)


    def _msbuild(self, project_file: Path, cs_config: str, architecture: Architecture, description: str) -> None:
        # The app references the engine's generated .vcxproj, which the dotnet CLI cannot load
        BuildUtils.execute_command(
            [
                BuildUtils.find_msbuild(),
                str(project_file),
                "-restore",
                f"-p:Configuration={cs_config}",
                f"-p:Platform={architecture}",
                "-m",
                "-nologo",
                "-v:minimal",
            ],
            description,
        )


    def build_csharp_project(self, configuration: Configuration, architecture: Architecture) -> None:
        cs_config = self._csharp_configuration(configuration)

        self._msbuild(GAME_PROJECT_FILE, cs_config, architecture, f"MSBuild: Build Chess Game v{self.version or 'unknown'} ({cs_config}|{architecture})")


    def run_csharp_unit_tests(self, configuration: Configuration, architecture: Architecture) -> None:
        cs_config = self._csharp_configuration(configuration)

        self._msbuild(TEST_PROJECT_FILE, cs_config, architecture, f"MSBuild: Build C# unit tests ({cs_config}|{architecture})")

        BuildUtils.execute_command(
            [
                "dotnet", "test",
                str(TEST_PROJECT_FILE),
                "--no-build",
                "-c", cs_config,
                f"-p:Platform={architecture}",
                "--nologo",
            ],
            "dotnet: Running C# unit tests",
        )
