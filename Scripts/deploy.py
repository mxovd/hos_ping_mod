#!/usr/bin/env python3
"""Build and package the Hos Ping Mod for distribution."""

from __future__ import annotations

import json
import re
import shutil
import subprocess
from datetime import UTC, datetime
from pathlib import Path


def safe_slug(value: str) -> str:
    """Return a filesystem-friendly slug for the provided value."""
    slug = value.strip().replace(" ", "_")
    slug = re.sub(r"[^A-Za-z0-9_]+", "", slug)
    return slug or "mod"


def run(command: list[str], *, cwd: Path) -> None:
    """Execute an external command and raise on failure."""
    subprocess.run(command, cwd=cwd, check=True)


def main() -> None:
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

    mod_name = manifest.get("modName", "mod")
    mod_version = manifest.get("modVersion", "0.0.0")

    safe_name = safe_slug(mod_name)
    timestamp = datetime.now(UTC).strftime("%Y%m%d-%H%M%S")
    package_dir = package_root / f"{safe_name}-v{mod_version}-{timestamp}"
    libraries_dir = package_dir / "Libraries"
    sounds_attack_dir = package_dir / "Sounds" / "Attack"

    package_dir.mkdir(parents=True, exist_ok=True)
    libraries_dir.mkdir(parents=True, exist_ok=True)
    sounds_attack_dir.mkdir(parents=True, exist_ok=True)

    run(["dotnet", "build", str(project_path), "--configuration", "Release"], cwd=root)

    if not output_dll.exists():
        raise SystemExit(f"Build completed but DLL missing at {output_dll}")

    shutil.copy2(manifest_path, package_dir / "Manifest.json")
    shutil.copy2(output_dll, libraries_dir / output_dll.name)

    if thumbnail_src.exists():
        shutil.copy2(thumbnail_src, package_dir / "Thumbnail.jpg")

    if ping_sound_src.exists():
        shutil.copy2(ping_sound_src, sounds_attack_dir / ping_sound_src.name)

    print(f"Package created at {package_dir}")


if __name__ == "__main__":
    try:
        main()
    except subprocess.CalledProcessError as error:
        raise SystemExit(error.returncode) from error
