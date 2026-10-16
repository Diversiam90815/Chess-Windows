from pathlib import Path

from .enums import Architecture, Configuration, Platform
from .utils import BuildUtils
from .versioning import VersionManager
from .paths import *


class BuildRunner:
    """
    Builds the Chess Windows app. The native libraries it loads (the chess engine and NetLink) are built from
    their repositories only as far as the app needs them, then installed into install/<configuration>, where the
    C# project picks them up. Their tests, tools and code checks are run from their own repositories.
    """

    def __init__(self, dependencies: list[NativeDependency], architecture: Architecture) -> None:
        self.dependencies = dependencies
        self.architecture = architecture

        self.version: str | None = None

        self.version_manager = VersionManager(DIRECTORY_BUILD_PROPS_FILE, PACKAGE_MANIFEST_FILE)

    # ---- Versioning ----
    def update_app_version(self) -> None:
        self.version = self.version_manager.update_build_number_in_version()


    # ---- Native libraries ----
    def generate_cmake_project(self, dependency: NativeDependency, platform: Platform) -> None:
        prepare_cmd = [
            "cmake",
            "-G", str(platform),
            "-S", str(dependency.source_dir),
            "-B", str(dependency.build_dir(str(self.architecture))),
            # For the INSTALL project in Visual Studio: it installs the configuration it is built in where the app
            # takes it from (cmake_install.cmake gets BUILD_TYPE). Only the projects it depends on in the solution are built.
            f"-DCMAKE_INSTALL_PREFIX={INSTALL_DIR.as_posix()}/${{BUILD_TYPE}}",
            "-DCMAKE_SKIP_INSTALL_ALL_DEPENDENCY=ON",
            *dependency.cmake_options,
        ]
        if platform == Platform.VS2022 or platform == Platform.VS2026:
            prepare_cmd += ["-A", str(self.architecture)]

        BuildUtils.execute_command(
            prepare_cmd,
            f"CMake: Generate {platform} project for {dependency.name}",
        )


    def build_and_install(self, dependency: NativeDependency, configuration: Configuration) -> None:
        build_dir = dependency.build_dir(str(self.architecture))
        # Named after the C# configuration that loads it
        install_dir = get_install_dir(self._csharp_configuration(configuration))

        BuildUtils.execute_command(
            [
                "cmake",
                "--build", str(build_dir),
                "--config", str(configuration),
                "--target", dependency.target,
                "--parallel", "8",
            ],
            f"CMake: Build {dependency.target} ({configuration})",
        )

        install_cmd = [
            "cmake",
            "--install", str(build_dir),
            "--config", str(configuration),
            "--prefix", str(install_dir),
        ]
        if dependency.install_component:
            install_cmd += ["--component", dependency.install_component]

        BuildUtils.execute_command(install_cmd, f"CMake: Install {dependency.name} ({configuration})")


    def build_native_libraries(self, platform: Platform, configurations: list[Configuration]) -> None:
        for dependency in self.dependencies:
            self.generate_cmake_project(dependency, platform)

            for configuration in configurations:
                self.build_and_install(dependency, configuration)


    # ---- C# app ----
    @staticmethod
    def _csharp_configuration(configuration: Configuration) -> str:
        # MSBuild only supports Debug and Release; treat RelWithDebInfo as Release
        return "Release" if configuration == Configuration.RelWithDebInfo else str(configuration)


    def _msbuild(self, project_file: Path, cs_config: str, description: str) -> None:
        # MSBuild rather than the dotnet CLI: the single-project MSIX packaging of the app needs it
        BuildUtils.execute_command(
            [
                BuildUtils.find_msbuild(),
                str(project_file),
                "-restore",
                f"-p:Configuration={cs_config}",
                f"-p:Platform={self.architecture}",
                "-m",
                "-nologo",
                "-v:minimal",
            ],
            description,
        )


    def build_csharp_project(self, configuration: Configuration) -> None:
        cs_config = self._csharp_configuration(configuration)

        self._msbuild(GAME_PROJECT_FILE, cs_config, f"MSBuild: Build Chess Game v{self.version or 'unknown'} ({cs_config}|{self.architecture})")


    def run_csharp_unit_tests(self, configuration: Configuration) -> None:
        cs_config = self._csharp_configuration(configuration)

        self._msbuild(TEST_PROJECT_FILE, cs_config, f"MSBuild: Build C# unit tests ({cs_config}|{self.architecture})")

        BuildUtils.execute_command(
            [
                "dotnet", "test",
                str(TEST_PROJECT_FILE),
                "--no-build",
                "-c", cs_config,
                f"-p:Platform={self.architecture}",
                "--nologo",
            ],
            "dotnet: Running C# unit tests",
        )
