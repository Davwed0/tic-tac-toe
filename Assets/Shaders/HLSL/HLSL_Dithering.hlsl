//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void ColorQuant_float(float2 UV, float Count, float3 In, out float3 Out)
{
    int x = UV.x * _ScreenParams.x;
    int y = UV.y * _ScreenParams.y;

    float3 ditherColor = In;

    ditherColor.r = floor((Count - 1.0f) * In.r + 0.5) / (Count - 1.0f);
    ditherColor.g = floor((Count - 1.0f) * In.g + 0.5) / (Count - 1.0f);
    ditherColor.b = floor((Count - 1.0f) * In.b + 0.5) / (Count - 1.0f);

    Out = ditherColor;
}

#endif //MYHLSLINCLUDE_INCLUDED