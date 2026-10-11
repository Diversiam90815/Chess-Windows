from pathlib import Path
from subprocess import check_output
from typing import Optional
import xml.etree.ElementTree as ET


class VersionManager:
    """Keeps the app version in Directory.Build.props and the package manifest."""

    def __init__(self, build_props_file: Path, manifest_file: Path) -> None:
        self.build_props_file = build_props_file
        self.manifest_file = manifest_file


    def get_build_number(self) -> int:
        commit_hashes = check_output(["git", "rev-list", "HEAD"]).rstrip()
        return commit_hashes.count(b"\n") + 1


    def get_current_version(self) -> Optional[str]:
        version_element = ET.parse(self.build_props_file).getroot().find('.//Version')

        if version_element is None or not version_element.text:
            return None

        return version_element.text.strip()


    def update_build_number_in_version(self) -> Optional[str]:
        current_version = self.get_current_version()
        if not current_version:
            return None

        build_number = self.get_build_number()
        parts = current_version.split(".")
        parts[-1] = str(build_number)
        new_version = ".".join(parts)

        self.update_app_version_in_exe(new_version)
        self.update_app_version_in_manifest(new_version)
        return new_version


    def update_app_version_in_exe(self, version):
        tree = ET.parse(self.build_props_file)
        root = tree.getroot()

        version_element = root.find('.//Version')

        if version_element is not None:
            # Update the Version text
            version_element.text = version

        tree.write(self.build_props_file, encoding='utf-8', xml_declaration=True)


    def update_app_version_in_manifest(self, version):
        ET.register_namespace("", "http://schemas.microsoft.com/appx/manifest/foundation/windows10")
        ET.register_namespace("mp", "http://schemas.microsoft.com/appx/2014/phone/manifest")
        ET.register_namespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10")
        ET.register_namespace("rescap", "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities")

        tree = ET.parse(self.manifest_file)
        root = tree.getroot()

        identity_element = root.find('.//{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity')

        if identity_element is not None:
            identity_element.set('Version', version)

            # Write the changes back to the file
            tree.write(self.manifest_file, encoding='utf-8', xml_declaration=True)
