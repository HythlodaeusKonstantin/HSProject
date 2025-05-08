#version 410 core
layout(location = 0) in vec2 fTexCoord;
layout(location = 1) in vec4 fColor;

layout(location = 0) out vec4 outColor;

uniform sampler2D Texture;

void main() {
    outColor = texture(Texture, fTexCoord) * fColor;
}