import os
import sys
from pathlib import Path

from scripts.paths import *
from scripts.enums import Configuration
from scripts.build_runner import BuildRunner
from scripts.argument_parser import create_argument_parser


def main():
    parser = create_argument_parser()
    args = parser.parse_args()

    if not len(sys.argv) > 1:
        parser.print_help()
        exit(1)

    ''' Setting up environment '''
    os.chdir(ROOT_DIR)

    dependencies = native_dependencies(engine_dir=Path(args.engine_dir).resolve(), netlink_dir=Path(args.netlink_dir).resolve())
    runner = BuildRunner(dependencies=dependencies, architecture=args.architecture)

    runner.update_app_version()

    print("==== Chess Game Configuration ====")
    print(f"Current Directory:          {ROOT_DIR}")
    print(f"Install Directory:          {INSTALL_DIR}")
    print(f"Architecture:               {args.architecture}")

    for dependency in dependencies:
        print(f"{dependency.name + ':':28s}{dependency.source_dir}")

    if not args.prepare:
        print(f"Configuration:              {args.configuration}")

    print(f"Version:                    {runner.version}")
    print("=====================================")

    if args.prepare:
        # Both configurations, so the solution can be built either way in the IDE
        runner.build_native_libraries(platform=args.platform, configurations=[Configuration.Release, Configuration.Debug])

    if args.build:
        runner.build_native_libraries(platform=args.platform, configurations=[args.configuration])
        runner.build_csharp_project(configuration=args.configuration)

    if args.runtest:
        runner.run_csharp_unit_tests(configuration=args.configuration)



if __name__ == "__main__":
    main()
