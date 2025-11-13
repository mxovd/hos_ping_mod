#!/usr/bin/env python3
"""Build and package the Hos Ping Mod for distribution."""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
from pathlib import Path


def run(command: list[str], *, cwd: Path) -> None:
    """Execute an external command and raise on failure."""
    subprocess.run(command, cwd=cwd, check=True)


def compute_package_dir(package_root: Path, mod_version: str) -> Path:
    prefix = f"Ping_Mod-v{mod_version}-"
    highest_index = 0

    if package_root.exists():
        for entry in package_root.iterdir():
            name = entry.name
            if not name.startswith(prefix):
                continue

            suffix = name[len(prefix):]
            if entry.is_file():
                suffix = suffix.split(".", 1)[0]

            if suffix.isdigit():
                highest_index = max(highest_index, int(suffix))

    next_index = highest_index + 1
    return package_root / f"{prefix}{next_index}"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build and package the Hos Ping Mod.")
    parser.add_argument(
        "--install",
        "-i",
        action="store_true",
        help="Copy the built package into the local Hex of Steel mods directory.",
    )
    return parser.parse_args()


def install_package(package_root: Path) -> Path:
    install_root = (
        Path.home()
        / ".var"
        / "app"
        / "com.valvesoftware.Steam"
        / "config"
        / "unity3d"
        / "War Frogs Studio"
        / "Hex of Steel"
        / "MODS"
    )
    target_path = install_root / package_root.name

    install_root.mkdir(parents=True, exist_ok=True)
    if target_path.exists():
        shutil.rmtree(target_path)
    shutil.copytree(package_root, target_path)
    return target_path


def main() -> None:
    args = parse_args()
    root = Path(__file__).resolve().parent.parent
    manifest_path = root / "Manifest.json"
    project_path = root / "HosPingMod.csproj"
    output_dll = root / "output" / "net48" / "HosPingMod.dll"
    package_root = root / "package"
    assets_dir = root / "assets"
    thumbnail_src = assets_dir / "Thumbnail.jpg"
    ping_sound_src = assets_dir / "ping_1.ogg"

    if not manifest_path.exists():
        raise SystemExit(f"manifest.json not found at {manifest_path}")

    if not project_path.exists():
        raise SystemExit(f"Project file not found at {project_path}")

    with manifest_path.open(encoding="utf-8") as handle:
        manifest = json.load(handle)

    mod_version = manifest.get("modVersion", "0.0.0")

    package_root.mkdir(parents=True, exist_ok=True)

    package_dir = compute_package_dir(package_root, mod_version)
    target_root = package_dir / "Multiplayer Tile Pings"
    libraries_dir = target_root / "Libraries"
    sounds_attack_dir = target_root / "Sounds" / "Attack"

    package_dir.mkdir(parents=True, exist_ok=True)
    target_root.mkdir(parents=True, exist_ok=True)
    libraries_dir.mkdir(parents=True, exist_ok=True)
    sounds_attack_dir.mkdir(parents=True, exist_ok=True)

    run(["dotnet", "build", str(project_path), "--configuration", "Release"], cwd=root)

    if not output_dll.exists():
        raise SystemExit(f"Build completed but DLL missing at {output_dll}")

    shutil.copy2(manifest_path, target_root / "Manifest.json")
    shutil.copy2(output_dll, libraries_dir / output_dll.name)

    if thumbnail_src.exists():
        shutil.copy2(thumbnail_src, target_root / "Thumbnail.jpg")

    if ping_sound_src.exists():
        shutil.copy2(ping_sound_src, sounds_attack_dir / ping_sound_src.name)

    print(f"Package created at {package_dir}")

    if args.install:
        installed_path = install_package(target_root)
        print(f"Mod installed to {installed_path}")


if __name__ == "__main__":
    try:
        main()
    except subprocess.CalledProcessError as error:
        raise SystemExit(error.returncode) from error
