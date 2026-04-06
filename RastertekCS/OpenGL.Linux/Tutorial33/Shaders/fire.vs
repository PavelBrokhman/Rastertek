#version 400
in vec3 inputPosition;
in vec2 inputTexCoord;
in vec3 inputNormal;
out vec2 texCoord;
out vec2 texCoords1;
out vec2 texCoords2;
out vec2 texCoords3;
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform float frameTime;
uniform vec3 scrollSpeeds;
uniform vec3 scales;
void main(void) {
    gl_Position = projectionMatrix * viewMatrix * worldMatrix * vec4(inputPosition, 1.0f);
    texCoord = inputTexCoord;
    texCoords1 = (inputTexCoord * scales.x); texCoords1.y -= (frameTime * scrollSpeeds.x);
    texCoords2 = (inputTexCoord * scales.y); texCoords2.y -= (frameTime * scrollSpeeds.y);
    texCoords3 = (inputTexCoord * scales.z); texCoords3.y -= (frameTime * scrollSpeeds.z);
}
