#!/usr/bin/env python3
"""Rename m_xxx private fields to _camelCase across all RastertekCS tutorials."""

import re
import sys
from pathlib import Path

# Specific abbreviation expansions (cryptic -> readable)
ABBREV_MAP = {
    "m_vs":         "_vertexShader",
    "m_fs":         "_fragmentShader",
    "m_prog":       "_shaderProgram",
    "m_vao":        "_vertexArrayId",
    "m_vbo":        "_vertexBufferId",
    "m_ibo":        "_indexBufferId",
    "m_vc":         "_vertexCount",
    "m_md":         "_modelData",
    "m_tex":        "_textures",
    "m_cam":        "_camera",
    "m_gfx":        "_graphics",
    "m_ic":         "_inputContext",
    "m_in":         "_input",
    "m_w":          "_window",
    "m_gl":         "_gl",
    "m_px":         "_positionX",
    "m_py":         "_positionY",
    "m_pz":         "_positionZ",
    "m_rx":         "_rotationX",
    "m_ry":         "_rotationY",
    "m_rz":         "_rotationZ",
    "m_proj":       "_projectionMatrix",
    "m_projMatrix": "_projectionMatrix",
    "m_world":      "_worldMatrix",
    "m_view":       "_viewMatrix",
    "m_rt":         "_renderTexture",
    "m_fbo":        "_frameBufferId",
    "m_tw":         "_textureWidth",
    "m_th":         "_textureHeight",
    "m_sw":         "_screenWidth",
    "m_sh":         "_screenHeight",
    "m_texId":      "_textureId",
    "m_texShader":  "_textureShader",
    "m_init":       "_initialized",
    "m_winModel":   "_windowModel",
    "m_prevPosX":   "_previousPositionX",
    "m_prevPosY":   "_previousPositionY",
    "m_depth":      "_depth",
    "m_depthId":    "_depthId",
    "m_id":         "_id",
    "m_vbo":        "_vertexBufferId",
    "m_ibo":        "_indexBufferId",
    "m_fps":        "_fps",
    "m_sw":         "_screenWidth",
    "m_sh":         "_screenHeight",
}


def to_camel(old_name: str) -> str:
    """Convert m_Xyz or m_xyz to _xyz."""
    if old_name in ABBREV_MAP:
        return ABBREV_MAP[old_name]
    m = re.match(r"m_([A-Za-z])(.*)", old_name)
    if m:
        return f"_{m.group(1).lower()}{m.group(2)}"
    return old_name


def process_file(path: Path) -> bool:
    try:
        text = path.read_text(encoding="utf-8-sig")
    except Exception as e:
        print(f"  SKIP {path}: {e}")
        return False

    # Collect all m_ identifiers used in this file
    found = set(re.findall(r"\bm_[A-Za-z][A-Za-z0-9_]*", text))
    if not found:
        return False

    # Sort longest first so m_projMatrix is replaced before m_proj
    ordered = sorted(found, key=len, reverse=True)

    # Build rename map, checking for conflicts
    rename_map: dict[str, str] = {}
    new_names: set[str] = set()
    for old in ordered:
        new = to_camel(old)
        if new in new_names:
            conflict = next(k for k, v in rename_map.items() if v == new)
            print(f"  CONFLICT {path.name}: {old} -> {new} (clashes with {conflict})")
            continue
        rename_map[old] = new
        new_names.add(new)

    # Apply replacements
    new_text = text
    for old, new in sorted(rename_map.items(), key=lambda kv: len(kv[0]), reverse=True):
        new_text = re.sub(r"\b" + re.escape(old) + r"\b", new, new_text)

    if new_text == text:
        return False

    path.write_text(new_text, encoding="utf-8")
    return True


def main():
    base = Path("/home/pbrokhman/Projects/Rastertek/RastertekCS")
    changed = 0
    errors = 0

    for platform in ("OpenGL", "Windows"):
        for n in range(2, 34):
            tut_dir = base / platform / f"Tutorial{n:02d}"
            if not tut_dir.exists():
                continue
            for cs in sorted(tut_dir.rglob("*.cs")):
                if "obj" in cs.parts or "bin" in cs.parts:
                    continue
                try:
                    if process_file(cs):
                        print(f"  OK  {cs.relative_to(base)}")
                        changed += 1
                except Exception as e:
                    print(f"  ERR {cs}: {e}")
                    errors += 1

    print(f"\nDone: {changed} files changed, {errors} errors.")


if __name__ == "__main__":
    main()
