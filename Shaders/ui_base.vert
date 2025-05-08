#version 410 core
layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 2) in vec4 Color;

layout(location = 0) out vec2 fTexCoord;
layout(location = 1) out vec4 fColor;

uniform mat4 ProjectionView;

void main() {
    gl_Position = ProjectionView * vec4(Position, 0.0, 1.0);
    fTexCoord = TexCoord;
    fColor = Color;
}