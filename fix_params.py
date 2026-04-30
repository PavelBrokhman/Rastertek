#!/usr/bin/env python3
"""Fix abbreviated parameter/local variable names in Tutorial31-33 OpenGL files."""

import re
from pathlib import Path

BASE = Path("/home/pbrokhman/Projects/Rastertek/RastertekCS/OpenGL")


def patch(path: Path, replacements: list[tuple[str, str]], replace_all=True) -> bool:
    text = path.read_text(encoding="utf-8")
    original = text
    for old, new in replacements:
        if replace_all:
            text = re.sub(r"\b" + re.escape(old) + r"\b", new, text)
        else:
            text = text.replace(old, new, 1)
    if text == original:
        print(f"  NO CHANGE: {path.relative_to(BASE.parent.parent)}")
        return False
    path.write_text(text, encoding="utf-8")
    print(f"  OK: {path.relative_to(BASE.parent.parent)}")
    return True


# ── Shared: fix "var g = gl.Gl" and "g." in all shader/model files ─────────
# We target the exact local-var pattern, then do g. -> glApi. within shader files.
# In model files InitBuf also has "var g = gl.Gl".
SHADER_FILES_WITH_G = [
    BASE / "Tutorial31/Graphics/LightShader.cs",
    BASE / "Tutorial31/Graphics/RefractionShader.cs",
    BASE / "Tutorial31/Graphics/WaterShader.cs",
    BASE / "Tutorial32/Graphics/GlassShader.cs",
    BASE / "Tutorial32/Graphics/TextureShader.cs",
    BASE / "Tutorial32/Graphics/Model.cs",
    BASE / "Tutorial33/Graphics/FireShader.cs",
    BASE / "Tutorial33/Graphics/Model.cs",
]

for f in SHADER_FILES_WITH_G:
    text = f.read_text(encoding="utf-8")
    original = text
    text = text.replace("var g = gl.Gl;", "var glApi = gl.Gl;")
    # Only replace "g." when g is the GL object (word boundary + dot)
    text = re.sub(r"\bg\.", "glApi.", text)
    if text != original:
        f.write_text(text, encoding="utf-8")
        print(f"  OK (g->glApi): {f.relative_to(BASE.parent.parent)}")


# ── GL4.cs Tutorial31: Initialize params ────────────────────────────────────
patch(
    BASE / "Tutorial31/Graphics/GL4.cs",
    [
        ("int sw", "int screenWidth"),
        ("int sh", "int screenHeight"),
        ("float sd", "float screenDepth"),
        ("float sn", "float screenNear"),
        # update usages inside method
        ("_screenWidth = sw;", "_screenWidth = screenWidth;"),
        ("_screenHeight = sh;", "_screenHeight = screenHeight;"),
        ("(uint)sw, (uint)sh", "(uint)screenWidth, (uint)screenHeight"),
        ("(float)sw / sh", "(float)screenWidth / screenHeight"),
        # PerspectiveFovLH call uses sn, sd
        ("PerspectiveFovLH(MathF.PI / 4.0f, (float)screenWidth / screenHeight, sn, sd)",
         "PerspectiveFovLH(MathF.PI / 4.0f, (float)screenWidth / screenHeight, screenNear, screenDepth)"),
    ],
    replace_all=False,
)

# ── RenderTexture.cs Tutorial31: Initialize params ───────────────────────────
patch(
    BASE / "Tutorial31/Graphics/RenderTexture.cs",
    [
        ("int tw", "int textureWidth"),
        ("int th", "int textureHeight"),
        ("float sn", "float screenNear"),
        ("float sd", "float screenDepth"),
        # usages inside Initialize
        ("_textureWidth = tw;", "_textureWidth = textureWidth;"),
        ("_textureHeight = th;", "_textureHeight = textureHeight;"),
        ("(uint)tw", "(uint)textureWidth"),
        ("(uint)th", "(uint)textureHeight"),
        ("(float)tw / th", "(float)textureWidth / textureHeight"),
        ("PerspectiveFovLH(MathF.PI / 4.0f, (float)textureWidth / textureHeight, sn, sd)",
         "PerspectiveFovLH(MathF.PI / 4.0f, (float)textureWidth / textureHeight, screenNear, screenDepth)"),
    ],
    replace_all=False,
)

# ── Tutorial31 LightShader: param names ─────────────────────────────────────
patch(
    BASE / "Tutorial31/Graphics/LightShader.cs",
    [
        ("Matrix4X4<float> proj", "Matrix4X4<float> projectionMatrix"),
        ("float[] lightDir",      "float[] lightDirection"),
        ("float[] diffuse",       "float[] diffuseColor"),
        ("float[] ambient",       "float[] ambientColor"),
        ("string vsf",            "string vertexShaderFile"),
        ("string psf",            "string pixelShaderFile"),
        # update usages
        ("(float*)&proj",         "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)", "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)", "File.ReadAllText(pixelShaderFile)"),
        # fixed-block pin vars (lightDir -> lightDirection etc.)
        ("float* p = lightDir",   "float* p = lightDirection"),
        ("float* p = diffuse",    "float* p = diffuseColor"),
        ("float* p = ambient",    "float* p = ambientColor"),
    ],
)

# ── Tutorial31 RefractionShader: same fixes ──────────────────────────────────
patch(
    BASE / "Tutorial31/Graphics/RefractionShader.cs",
    [
        ("Matrix4X4<float> proj", "Matrix4X4<float> projectionMatrix"),
        ("float[] lightDir",      "float[] lightDirection"),
        ("float[] diffuse",       "float[] diffuseColor"),
        ("float[] ambient",       "float[] ambientColor"),
        ("float[] clipPlane",     "float[] clipPlane"),   # already fine, no-op
        ("string vsf",            "string vertexShaderFile"),
        ("string psf",            "string pixelShaderFile"),
        ("(float*)&proj",         "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)", "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)", "File.ReadAllText(pixelShaderFile)"),
        ("float* p = lightDir",   "float* p = lightDirection"),
        ("float* p = diffuse",    "float* p = diffuseColor"),
        ("float* p = ambient",    "float* p = ambientColor"),
        ("float* p = clipPlane",  "float* p = clipPlane"),
    ],
)

# ── Tutorial31 WaterShader ───────────────────────────────────────────────────
patch(
    BASE / "Tutorial31/Graphics/WaterShader.cs",
    [
        ("Matrix4X4<float> proj",    "Matrix4X4<float> projectionMatrix"),
        ("float waterTrans",         "float waterTranslation"),
        ("float reflRefScale",       "float reflectRefractScale"),
        ("string vsf",               "string vertexShaderFile"),
        ("string psf",               "string pixelShaderFile"),
        ("(float*)&proj",            "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)",    "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)",    "File.ReadAllText(pixelShaderFile)"),
        # uniform calls
        ('"waterTranslation"),\n            glApi.Uniform1(loc, waterTrans)',
         '"waterTranslation"),\n            glApi.Uniform1(loc, waterTranslation)'),
        ('"reflectRefractScale"),\n            glApi.Uniform1(loc, reflRefScale)',
         '"reflectRefractScale"),\n            glApi.Uniform1(loc, reflectRefractScale)'),
    ],
)

# simpler approach for WaterShader uniform calls
f = BASE / "Tutorial31/Graphics/WaterShader.cs"
text = f.read_text()
text = re.sub(r'\bwaterTrans\b', 'waterTranslation', text)
text = re.sub(r'\breflRefScale\b', 'reflectRefractScale', text)
f.write_text(text)

# ── Tutorial32 GlassShader ───────────────────────────────────────────────────
patch(
    BASE / "Tutorial32/Graphics/GlassShader.cs",
    [
        ("Matrix4X4<float> w",  "Matrix4X4<float> worldMatrix"),
        ("Matrix4X4<float> v",  "Matrix4X4<float> viewMatrix"),
        ("Matrix4X4<float> p,", "Matrix4X4<float> projectionMatrix,"),
        ("float refrScale",     "float refractionScale"),
        ("string vsf",          "string vertexShaderFile"),
        ("string psf",          "string pixelShaderFile"),
        ("(float*)&w",          "(float*)&worldMatrix"),
        ("(float*)&v",          "(float*)&viewMatrix"),
        ("(float*)&p",          "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)", "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)", "File.ReadAllText(pixelShaderFile)"),
    ],
)
# fix refractionScale uniform call
f = BASE / "Tutorial32/Graphics/GlassShader.cs"
text = f.read_text()
text = re.sub(r'\brefrScale\b', 'refractionScale', text)
f.write_text(text)

# ── Tutorial32 TextureShader ─────────────────────────────────────────────────
patch(
    BASE / "Tutorial32/Graphics/TextureShader.cs",
    [
        ("Matrix4X4<float> w",  "Matrix4X4<float> worldMatrix"),
        ("Matrix4X4<float> v",  "Matrix4X4<float> viewMatrix"),
        ("Matrix4X4<float> p",  "Matrix4X4<float> projectionMatrix"),
        ("string vsf",          "string vertexShaderFile"),
        ("string psf",          "string pixelShaderFile"),
        ("(float*)&w",          "(float*)&worldMatrix"),
        ("(float*)&v",          "(float*)&viewMatrix"),
        ("(float*)&p",          "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)", "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)", "File.ReadAllText(pixelShaderFile)"),
    ],
)

# ── Tutorial33 FireShader ────────────────────────────────────────────────────
patch(
    BASE / "Tutorial33/Graphics/FireShader.cs",
    [
        ("Matrix4X4<float> w",   "Matrix4X4<float> worldMatrix"),
        ("Matrix4X4<float> v",   "Matrix4X4<float> viewMatrix"),
        ("Matrix4X4<float> p,",  "Matrix4X4<float> projectionMatrix,"),
        ("float[] d1",           "float[] distortion1"),
        ("float[] d2",           "float[] distortion2"),
        ("float[] d3",           "float[] distortion3"),
        ("float dScale",         "float distortionScale"),
        ("float dBias",          "float distortionBias"),
        ("string vsf",           "string vertexShaderFile"),
        ("string psf",           "string pixelShaderFile"),
        ("(float*)&w",           "(float*)&worldMatrix"),
        ("(float*)&v",           "(float*)&viewMatrix"),
        ("(float*)&p",           "(float*)&projectionMatrix"),
        ("File.ReadAllText(vsf)", "File.ReadAllText(vertexShaderFile)"),
        ("File.ReadAllText(psf)", "File.ReadAllText(pixelShaderFile)"),
    ],
)
# fix remaining usages in FireShader
f = BASE / "Tutorial33/Graphics/FireShader.cs"
text = f.read_text()
text = re.sub(r'\bd1\b', 'distortion1', text)
text = re.sub(r'\bd2\b', 'distortion2', text)
text = re.sub(r'\bd3\b', 'distortion3', text)
text = re.sub(r'\bdScale\b', 'distortionScale', text)
text = re.sub(r'\bdBias\b', 'distortionBias', text)
f.write_text(text)

# ── Tutorial31 Model.cs: local vars ──────────────────────────────────────────
f = BASE / "Tutorial31/Graphics/Model.cs"
text = f.read_text()
# param fn -> filename
text = re.sub(r'\bstring fn\b', 'string filename', text)
text = re.sub(r'if \(!File\.Exists\(fn\)', 'if (!File.Exists(filename)', text)
text = re.sub(r'File\.ReadAllLines\(fn\)', 'File.ReadAllLines(filename)', text)
text = re.sub(r'Console\.WriteLine\(\$"Model not found: \{fn\}"\)', 'Console.WriteLine($"Model not found: {filename}")', text)
# local vars in LoadModel
text = re.sub(r'\bint vc = 0,\s*\n\s*ds = -1;', 'int vertexCount = 0,\n            dataStartIndex = -1;', text)
text = re.sub(r'\bvc\b', 'vertexCount', text)
text = re.sub(r'\bds\b', 'dataStartIndex', text)
text = re.sub(r'\bvar l = lines\[i\]\.Trim\(\);', 'var line = lines[i].Trim();', text)
text = re.sub(r'\bl\.StartsWith\b', 'line.StartsWith', text)
text = re.sub(r'\bl == "Data:"\b', 'line == "Data:"', text)
text = re.sub(r'\bif \(string\.IsNullOrEmpty\(l\)\)', 'if (string.IsNullOrEmpty(line))', text)
# var p = l.Split -> var parts = line.Split
text = re.sub(r'\bvar p = l\.Split\b', 'var parts = line.Split', text)
text = re.sub(r'\bif \(p\.Length < 8\)', 'if (parts.Length < 8)', text)
text = re.sub(r'\bfloat\.Parse\(p\[(\d+)\]\)', lambda m: f'float.Parse(parts[{m.group(1)}])', text)
# vi -> vertexIndex, o -> offset (in LoadModel context)
text = re.sub(r'\bint vi = 0;', 'int vertexIndex = 0;', text)
text = re.sub(r'\bvi\b', 'vertexIndex', text)
# local o -> offset in loops
text = re.sub(r'\bint o = (\w+) \* 8;', r'int offset = \1 * 8;', text)
text = re.sub(r'\b_modelData\[o\b', '_modelData[offset', text)
text = re.sub(r'\bo \+ (\d+)\]', r'offset + \1]', text)
# InitBuffers local vars
text = re.sub(r'\bvar v = new VT\b', 'var vertices = new VT', text)
text = re.sub(r'\bvar idx = new uint\b', 'var indices = new uint', text)
text = re.sub(r'\bv\[i\]', 'vertices[i]', text)
text = re.sub(r'\bidx\[i\]\b', 'indices[i]', text)
text = re.sub(r'\bfixed \(VT\* p = v\)', 'fixed (VT* p = vertices)', text)
text = re.sub(r'\bsizeof\(VT\) \* v\.Length\)', 'sizeof(VT) * vertices.Length)', text)
text = re.sub(r'\bfixed \(uint\* p = idx\)', 'fixed (uint* p = indices)', text)
text = re.sub(r'\bsizeof\(uint\) \* idx\.Length\)', 'sizeof(uint) * indices.Length)', text)
# uint tu -> uint textureUnit
text = re.sub(r'\buint tu\b', 'uint textureUnit', text)
text = re.sub(r'SetTexture\(OpenGL, tu\)', 'SetTexture(OpenGL, textureUnit)', text)
f.write_text(text)
print(f"  OK (local vars): {f.relative_to(BASE.parent.parent)}")

# ── Tutorial32 & Tutorial33 Model.cs: _inputContext bug + local vars + struct ─
for tut in ["Tutorial32", "Tutorial33"]:
    f = BASE / f"{tut}/Graphics/Model.cs"
    text = f.read_text()
    # BUG FIX: _inputContext is really _indexCount in Model
    text = re.sub(r'\b_inputContext\b', '_indexCount', text)
    # struct VT -> VertexType
    text = re.sub(r'\bstruct VT\b', 'struct VertexType', text)
    text = re.sub(r'\bVT\b', 'VertexType', text)
    # InitBuf -> InitializeBuffers, ShutBuf -> ShutdownBuffers
    text = re.sub(r'\bInitBuf\b', 'InitializeBuffers', text)
    text = re.sub(r'\bShutBuf\b', 'ShutdownBuffers', text)
    # Initialize params: mf, t1, w1, t2, w2, t3, w3
    text = re.sub(r'\bstring mf\b', 'string modelFile', text)
    text = re.sub(r'\bstring t1\b', 'string texture1File', text)
    text = re.sub(r'\bstring t2\b', 'string texture2File', text)
    text = re.sub(r'\bstring t3\b', 'string texture3File', text)
    text = re.sub(r'\bbool w1\b', 'bool wrap1', text)
    text = re.sub(r'\bbool w2\b', 'bool wrap2', text)
    text = re.sub(r'\bbool w3\b', 'bool wrap3', text)
    # update usages of these params
    text = re.sub(r'\bLoadModel\(mf\)', 'LoadModel(modelFile)', text)
    text = re.sub(r'Initialize\(gl, t1, 0, w1\)', 'Initialize(gl, texture1File, 0, wrap1)', text)
    text = re.sub(r'Initialize\(gl, t2, 0, w2\)', 'Initialize(gl, texture2File, 0, wrap2)', text)
    text = re.sub(r'Initialize\(gl, t3, 0, w3\)', 'Initialize(gl, texture3File, 0, wrap3)', text)
    # LoadModel param fn -> filename
    text = re.sub(r'\bstring fn\b', 'string filename', text)
    text = re.sub(r'if \(!File\.Exists\(fn\)\)', 'if (!File.Exists(filename))', text)
    text = re.sub(r'File\.ReadAllLines\(fn\)', 'File.ReadAllLines(filename)', text)
    # local vars in LoadModel
    text = re.sub(r'\bint vc = 0,\s*\n\s*ds = -1;', 'int vertexCount = 0,\n            dataStartIndex = -1;', text)
    text = re.sub(r'\bvc\b', 'vertexCount', text)
    text = re.sub(r'\bds\b', 'dataStartIndex', text)
    text = re.sub(r'\bvar l = lines\[i\]\.Trim\(\);', 'var line = lines[i].Trim();', text)
    text = re.sub(r'\bl\.StartsWith\b', 'line.StartsWith', text)
    text = re.sub(r'\bl == "Data:"\b', 'line == "Data:"', text)
    text = re.sub(r'\bif \(string\.IsNullOrEmpty\(l\)\)', 'if (string.IsNullOrEmpty(line))', text)
    text = re.sub(r'\bvar p = l\.Split\b', 'var parts = line.Split', text)
    text = re.sub(r'\bif \(p\.Length < 8\)', 'if (parts.Length < 8)', text)
    text = re.sub(r'\bfloat\.Parse\(p\[(\d+)\]\)', lambda m: f'float.Parse(parts[{m.group(1)}])', text)
    text = re.sub(r'\bint vi = 0;', 'int vertexIndex = 0;', text)
    text = re.sub(r'\bvi\b', 'vertexIndex', text)
    text = re.sub(r'\bint o = (\w+) \* 8;', r'int offset = \1 * 8;', text)
    text = re.sub(r'\b_modelData\[o\b', '_modelData[offset', text)
    text = re.sub(r'\bo \+ (\d+)\]', r'offset + \1]', text)
    # InitializeBuffers local vars
    text = re.sub(r'\bvar v = new VertexType\b', 'var vertices = new VertexType', text)
    text = re.sub(r'\bvar idx = new uint\b', 'var indices = new uint', text)
    text = re.sub(r'\bv\[i\]', 'vertices[i]', text)
    text = re.sub(r'\bidx\[i\]\b', 'indices[i]', text)
    text = re.sub(r'\bfixed \(VertexType\* p = v\)', 'fixed (VertexType* p = vertices)', text)
    text = re.sub(r'\bsizeof\(VertexType\) \* v\.Length\)', 'sizeof(VertexType) * vertices.Length)', text)
    text = re.sub(r'\bfixed \(uint\* p = idx\)', 'fixed (uint* p = indices)', text)
    text = re.sub(r'\bsizeof\(uint\) \* idx\.Length\)', 'sizeof(uint) * indices.Length)', text)
    # SetTexture1/2/3 uint tu -> uint textureUnit
    text = re.sub(r'\buint tu\b', 'uint textureUnit', text)
    text = re.sub(r'SetTexture\(gl, tu\)', 'SetTexture(gl, textureUnit)', text)
    f.write_text(text)
    print(f"  OK (Model.cs): {f.relative_to(BASE.parent.parent)}")

print("\nDone.")
