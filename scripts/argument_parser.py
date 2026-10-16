import argparse
from .enums import Architecture, Configuration, Platform
from .paths import ENGINE_DIR, NETLINK_DIR


def create_argument_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Build script for the Chess Windows Application"
    )

    parser.add_argument(
        "-p", "--prepare",
        action="store_true",
        help="Build and install the native libraries (Debug and Release) for use in the IDE."
    )

    parser.add_argument(
        "-b", "--build",
        action="store_true",
        help="Build and install the native libraries, then build the C# application."
    )

    parser.add_argument(
        "-pl", "--platform",
        default=Platform.VS2026,
        type=Platform,
        choices=list(Platform),
        help="Select your Platform/IDE. Defaults to VS2026 (the C# project needs a Visual Studio generator)."
    )

    parser.add_argument(
        "-a", "--architecture",
        default=Architecture.x64,
        type=Architecture,
        choices=list(Architecture),
        help="Select the architecture. Defaults to x64."
    )

    parser.add_argument(
        "-c", "--configuration",
        default=Configuration.Release,
        type=Configuration,
        choices=list(Configuration),
        help="Select the build configuration. Defaults to Release."
    )
    
    parser.add_argument(
        "-t", "--runtest",
        action="store_true",
        help="Build and run the C# unit tests."
    )

    parser.add_argument(
        "--engine-dir",
        default=str(ENGINE_DIR),
        help="Source of the chess engine. Defaults to the Chess.Engine submodule."
    )

    parser.add_argument(
        "--netlink-dir",
        default=str(NETLINK_DIR),
        help="Source of NetLink. Defaults to the NetLink submodule."
    )
    return parser
