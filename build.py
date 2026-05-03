import os
import sys

from scripts.paths import *
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

    build_dir = get_build_dir(str(args.architecture))
    test_dir = build_dir / "tests"
    runner = BuildRunner(root_dir=ENGINE_DIR, build_dir=build_dir, project_name="Chess Game")

    runner.update_environment()
    runner.update_app_version()

    print("==== Chess Game Configuration ====")
    print(f"Current Directory:          {ROOT_DIR}")
    print(f"Build Directory:            {build_dir}")
    print(f"Test Build Directory:       {test_dir}")
    print(f"Architecture:               {args.architecture}")

    if not args.prepare:
        print(f"Configuration:              {args.configuration}")
    
    print(f"Environment:                {runner.env}")
    print(f"Version:                    {runner.version}")
    print("=====================================")

    if args.prepare:
        runner.prepare_cmake_project(platform=args.platform, architecture=args.architecture)

    if args.runtest:
        runner.run_cpp_unit_tests(configuration=args.configuration, test_build_dir=test_dir, target="RUN_TESTS")

    if args.build:
        runner.create_build_generator(platform=args.platform, architecture=args.architecture, configuration=args.configuration)
        runner.build_csharp_project(configuration=args.configuration, architecture=args.architecture)

    if not args.prepare:
        runner.run_cpp_unit_tests(configuration=args.configuration, test_build_dir=test_dir, target="RUN_TESTS")



if __name__ == "__main__":
    main()
