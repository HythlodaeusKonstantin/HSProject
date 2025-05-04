#version 410 core
layout(location = 0) in vec2 fTexCoord;
layout(location = 1) in vec4 fColor;

layout(location = 0) out vec4 outColor;

uniform sampler2D Texture;
uniform float smoothing;
uniform float thickness;

void main() {
    // Получаем значение из SDF текстуры (расстояние до контура)
    float distance = texture(Texture, fTexCoord).r;
    
    // Вычисляем альфа-канал с учетом сглаживания и толщины
    float alpha = smoothstep(thickness - smoothing, thickness + smoothing, distance);
    
    // Выходной цвет: цвет текста с пересчитанной альфой
    outColor = vec4(fColor.rgb, fColor.a * alpha);
} 