import argparse
import os
import sys
import shutil
import xml.etree.ElementTree as ET
import re

from subprocess import Popen, PIPE, check_output


BUILD_DIRECTORY = os.path.join(os.getcwd(), "Build")

class AutoCWD(object):
    """Auto restore directory"""

    def __init__(self, path=""):
        self.orgdir = os.getcwd()
        os.chdir(path)

    def __del__(self): 
        os.chdir(self.orgdir)

    def __enter__(self):
        self.orgdir = os.getcwd()
        os.chdir(self.target)
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        os.chdir(self.orgdir)
        # returning False lets any exception bubble up; True would swallow
        return False


class BuildRunner(object):

    TARGET_CONFIG = 'Release'

    def __init__(self):   
        parser = argparse.ArgumentParser(description='This tool can be used to build and deploy the Chess project.')
        parser.add_argument('-p', '--prepare', action='store_true', help='prepares the project for use with IDE')
        parser.add_argument('-d', '--debug', action='store_true', help='prepare or build debug version')
        parser.add_argument('-b', '--build', action='store_true', help='build the project')
        parser.add_argument('-v', '--version', action='store_true', help='display Python and CMake versions')
        parser.add_argument('-t', '--test', action='store_true', help='run unit & performance tests')
        parser.add_argument('--docs', action='store_true', help='configure and build Doxygen documentation')

        self.args = parser.parse_args()
        self.version = ""
        self.platform = self._find_latest_visual_studio_version()
        self.args.path_project  = os.path.dirname(os.path.realpath(__file__))
        self.msbuild_path = os.path.join(self._get_vs_path(), "MSBuild", "Current", "Bin", "MSBuild.exe")


    @staticmethod
    def __log_description(description):
        print('{}{:80s}'.format('\t', description), end = '')
        sys.stdout.flush()


    @staticmethod
    def __log_done(msg='Done'):
        print(msg)
        

    def _execute_command(self, command, description):
        """ Execute system command and log stdout """
        BuildRunner.__log_description(description)
        
        execution = Popen(command, shell=True, stdout=PIPE, stderr=PIPE)
        output, error = execution.communicate()
        
        if execution.returncode:
            print('\n' + error.decode(encoding='utf-8', errors="replace"))
            print('\n' + output.decode(encoding='utf-8', errors="replace"))
            sys.stdout.flush()
            exit(1)

        BuildRunner.__log_done()

        sys.stdout.flush()
        result = output.decode(encoding='utf-8', errors="replace")

        return result


    def _print_versions(self):
        """ Print Python, CMake & Projects versions """
        self._update_app_version()
        print(f"Project's version: {self.version}")
        print(f"Python version: {sys.version}")
        try:
            cmake_version = self._execute_command("cmake --version", "Get CMake version")
            print(f"CMake version: {cmake_version.splitlines()[0]}")  # print only the first line of output
        except Exception as e:
            print(f"Error retrieving CMake version: {e}")

    
    def __get_number_of_commits(self):
        autoCWD = AutoCWD(self.args.path_project)
        sys.stdout.flush()
        commit_hashes = check_output("git rev-list HEAD", shell=True).rstrip()
        commit_number = commit_hashes.count(b'\n') + 1
        del autoCWD
        return commit_number
    

    def _get_build_number(self):
        BuildNumber = self.__get_number_of_commits()
        return BuildNumber
    

    def _update_app_version_in_cmake(self, version):
        pattern = r'set\(CHESS_VERSION\s*(\d+\.\d+)(\.\d+)?\.(\d+)'
        cmakeFile = os.path.join(self.args.path_project, 'Chess.Engine', 'CMakeLists.txt')
        tempFile = cmakeFile + '.tmp'
        
        with open(cmakeFile, 'r') as fileIn, open(tempFile, 'w') as fileOut:
            for line in fileIn:
                match = re.search(pattern, line)
                if match:
                    fileOut.write(f'set(CHESS_VERSION {version})\n')
                else:
                    fileOut.write(line)
        
        shutil.move(tempFile, cmakeFile)
        fileIn.close()
        fileOut.close()


    def _update_app_version_in_exe(self,version):
        build_props_file = os.path.join(os.getcwd(), 'Chess.UI', 'Chess.UI', "Directory.Build.Props")
        
        tree = ET.parse(build_props_file)
        root = tree.getroot()

        version_element = root.find('.//Version')

        if version_element is not None:
            # Update the Version text
            version_element.text = version
        else:
            print("Version element not found in Directory.Build.Props")
            
        tree.write(build_props_file, encoding='utf-8', xml_declaration=True)


    def _update_app_version(self):
        packageManifest = os.path.join(os.getcwd(), "Chess.UI", "Chess.UI", "Package.appxmanifest")
        
        ET.register_namespace("", "http://schemas.microsoft.com/appx/manifest/foundation/windows10")
        ET.register_namespace("mp", "http://schemas.microsoft.com/appx/2014/phone/manifest")
        ET.register_namespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10")
        ET.register_namespace("rescap", "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities")

        tree = ET.parse(packageManifest)
        root = tree.getroot()

        identity_element = root.find('.//{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity')
        
        if identity_element is not None:
            # Update the Version
            BuildNumber = self._get_build_number()
            
            current_version = identity_element.get('Version')
            
            if current_version:
                version_parts = current_version.split('.')
                version_parts[-1] = str(BuildNumber)
                self.version = '.'.join(version_parts)

                self._update_app_version_in_cmake(self.version)
                self._update_app_version_in_exe(self.version)
                identity_element.set('Version', self.version)
                
                # Write the changes back to the file
                tree.write(packageManifest, encoding='utf-8', xml_declaration=True)
                print(f'Version updated to {self.version}')
            
            else:
                print("No version attribute found")
        else:
            print("Identity element not found")


    def _build_docs(self, target_name: str):
        '''Build the doxygen target generated by the CMake Doxygen wrapper.'''
        engine_dir = os.path.join(self.args.path_project, "Chess.Engine")
        build_dir = os.path.join(engine_dir, "build")
        autoCWD = AutoCWD(engine_dir)

        self._execute_command(f'cmake --build "{build_dir}" --config {self.TARGET_CONFIG} --target {target_name}', f"Generate Doxygen docs ({target_name})…")

        del autoCWD


    def _find_latest_visual_studio_version(self):
        vswhere = os.path.join(os.environ.get("ProgramFiles(x86)"), "Microsoft Visual Studio", "Installer", "vswhere.exe")

        if not os.path.exists(vswhere):
            print("vswhere.exe not found! Using Visual Studio 2022 as default")
            return '\"Visual Studio 17\"'

        result = check_output([vswhere, '-latest', '-property', 'installationVersion'], shell=True).decode().strip()
        versionMatch = re.match(r'^(\d+)', result)

        if versionMatch:
            vsVersion = versionMatch.group(1)
            return f'\"Visual Studio {vsVersion}\"'

        else:
            print("Could not determine latest Visual Studio version. Using Visual Studio 2022 as default")
            return '\"Visual Studio 17\"'


    def _get_vs_path(self):
        vswhere = os.path.join(os.environ.get("ProgramFiles(x86)"), "Microsoft Visual Studio", "Installer", "vswhere.exe")
        if not os.path.exists(vswhere):
            raise FileNotFoundError("vswhere.exe not found. Please install Visual Studio or vswhere tool.")

        installation_path = check_output([vswhere, "-latest", "-property", "installationPath"], shell=True).decode().strip()

        if not installation_path:
            raise RuntimeError("Could not find a Visual Studio installation.")

        return installation_path
    

    def _run_engine_tests(self):
        # Run C++ Engine Tests
        engine_dir = os.path.join(self.args.path_project, "Chess.Engine")
        build_dir = os.path.join(engine_dir, "build")
        
        autoCWD = AutoCWD(engine_dir)
        self._execute_command(f'cmake --build {build_dir} --config {self.TARGET_CONFIG} --target RUN_TESTS', 'Running C++ engine tests…' )
        del autoCWD

    def _build_prepare(self):
        projectfolderVS =  os.path.join(self.args.path_project, 'Chess.Engine')
        autoCWD = AutoCWD(projectfolderVS)

        prepare_cmd = f'cmake -G {self.platform} -B build'
        self._execute_command(prepare_cmd, f"Select build generator: {self.platform}")
        
        del autoCWD

        
    def _build_project(self):
        projectfolderVS =  os.path.join(self.args.path_project, 'Chess.Engine')
        buildFolder = os.path.join(projectfolderVS, "build")

        if os.path.exists(projectfolderVS + "/CMakeCache.txt"):
            self._execute_command("cmake --build " + projectfolderVS + " --target clean", "Run CMake clean")
        
        self._execute_command("cmake --build " + buildFolder + " --config " + BuildRunner.TARGET_CONFIG + " --clean-first ", f"Build the Chess Engine Library v{self.version}")   

        
    def doit(self):
        if self.args.version:
            self._print_versions()
        if self.args.debug:
            BuildRunner.TARGET_CONFIG = 'Debug'
        if self.args.prepare:
            self._build_prepare()
            self._update_app_version()
        if self.args.test:
            self._run_engine_tests()
        if self.args.docs:
            self._build_prepare()
            self._build_docs(target_name="doxygen-Chess_Engine")
        if self.args.build:
            self._build_prepare()
            self._update_app_version()
            self._build_project()

            
if __name__ == '__main__':
    BuildRunner().doit()
    