#!/bin/bash
# Generate C# tutorials 53-59 from C++ Rastertek sources
# This script creates all boilerplate and tutorial-specific files

BASE="/home/pbrokhman/works/Rastertek/RastertekCS/OpenGL.Linux"
SRC="/tmp/rastertek"

# Helper: generate a standard shader class
gen_shader_class() {
    local N=$1 CLASS=$2 VS=$3 PS=$4 ATTRIBS=$5 UNIFORMS=$6
    local DIR="$BASE/Tutorial${N}/Graphics"

    local ATTRIB_BINDS=""
    local IFS=','
    local idx=0
    for a in $ATTRIBS; do
        ATTRIB_BINDS="${ATTRIB_BINDS}
        gl.BindAttribLocation(m_shaderProgram, ${idx}, \"${a}\");"
        idx=$((idx+1))
    done

    cat > "$DIR/${CLASS}.cs" << EOFSHADER
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial${N}.Graphics;

public class ${CLASS}
{
    private uint m_vertexShader, m_fragmentShader, m_shaderProgram;

    public bool Initialize(GL4 OpenGL) => InitializeShader(OpenGL, "Shaders/${VS}", "Shaders/${PS}");

    public void Shutdown(GL4 OpenGL)
    {
        var gl = OpenGL.Gl;
        gl.DetachShader(m_shaderProgram, m_vertexShader); gl.DetachShader(m_shaderProgram, m_fragmentShader);
        gl.DeleteShader(m_vertexShader); gl.DeleteShader(m_fragmentShader); gl.DeleteProgram(m_shaderProgram);
    }

    public uint Program => m_shaderProgram;
    public void Use(GL4 OpenGL) => OpenGL.Gl.UseProgram(m_shaderProgram);

    private bool InitializeShader(GL4 OpenGL, string vsFile, string psFile)
    {
        var gl = OpenGL.Gl;
        m_vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(m_vertexShader, File.ReadAllText(vsFile)); gl.CompileShader(m_vertexShader);
        gl.GetShader(m_vertexShader, ShaderParameterName.CompileStatus, out int s);
        if (s != 1) { Console.WriteLine(\$"Compile {vsFile}: {gl.GetShaderInfoLog(m_vertexShader)}"); return false; }
        m_fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(m_fragmentShader, File.ReadAllText(psFile)); gl.CompileShader(m_fragmentShader);
        gl.GetShader(m_fragmentShader, ShaderParameterName.CompileStatus, out s);
        if (s != 1) { Console.WriteLine(\$"Compile {psFile}: {gl.GetShaderInfoLog(m_fragmentShader)}"); return false; }
        m_shaderProgram = gl.CreateProgram();
        gl.AttachShader(m_shaderProgram, m_vertexShader); gl.AttachShader(m_shaderProgram, m_fragmentShader);${ATTRIB_BINDS}
        gl.LinkProgram(m_shaderProgram);
        gl.GetProgram(m_shaderProgram, ProgramPropertyARB.LinkStatus, out int ls);
        if (ls != 1) { Console.WriteLine(\$"Link: {gl.GetProgramInfoLog(m_shaderProgram)}"); return false; }
        return true;
    }
}
EOFSHADER
}

echo "Script ready - run with bash"
