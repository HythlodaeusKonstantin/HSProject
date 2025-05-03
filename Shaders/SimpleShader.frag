#version 330 core

in vec2 v_TexCoord;

uniform vec4 u_Color;
uniform sampler2D u_Texture;
uniform bool u_UseTexture;

out vec4 FragColor;

void main()
{
    if (u_UseTexture) {
        FragColor = texture(u_Texture, v_TexCoord);
    } else {
        FragColor = u_Color;
    }
} 