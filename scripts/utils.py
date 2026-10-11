import os
import sys
import subprocess
from contextlib import contextmanager
from pathlib import Path
from typing import Iterable


class BuildUtils:
    @staticmethod
    def _log_description(description: str) -> None:
        print(f"\t{description:80s}", end="")
        sys.stdout.flush()

    @staticmethod
    def _log_done(msg: str = "Done") -> None:
        print(msg)

    @staticmethod
    def execute_command(command: Iterable[str], description: str) -> str:
        """Execute system command and log stdout/stderr. Command must be an iterable of args."""
        BuildUtils._log_description(description)

        process = subprocess.run(
            list(command),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding="utf-8",
            errors="replace",
        )

        if process.returncode != 0:
            print("\n" + process.stderr)
            print("\n" + process.stdout)
            sys.stdout.flush()
            sys.exit(1)

        BuildUtils._log_done()
        sys.stdout.flush()
        return process.stdout

    @staticmethod
    def find_msbuild() -> str:
        """Locate the MSBuild.exe of the newest Visual Studio installation."""
        program_files = os.environ.get("ProgramFiles(x86)", r"C:\Program Files (x86)")
        vswhere = Path(program_files) / "Microsoft Visual Studio" / "Installer" / "vswhere.exe"

        if vswhere.exists():
            result = subprocess.run(
                [
                    str(vswhere),
                    "-latest",
                    "-prerelease",
                    "-requires", "Microsoft.Component.MSBuild",
                    "-find", r"MSBuild\**\Bin\MSBuild.exe",
                ],
                stdout=subprocess.PIPE,
                text=True,
            )
            candidates = result.stdout.splitlines()
            if candidates:
                return candidates[0]

        print("\nCould not find MSBuild.exe. Is Visual Studio with the MSBuild component installed?")
        sys.exit(1)


@contextmanager
def working_directory(path: Path):
    """Context manager to temporarily change CWD."""
    original = Path.cwd()
    os.chdir(path)
    try:
        yield
    finally:
        os.chdir(original)
