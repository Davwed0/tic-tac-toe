#ifndef EDGE_DETECTION_HLSL
#define EDGE_DETECTION_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

// Custom function to apply Roberts Cross edge detection
float RobertsCross(float samples[4])
{
    const float difference_1 = samples[1] - samples[2];
    const float difference_2 = samples[0] - samples[3];
    return sqrt(difference_1 * difference_1 + difference_2 * difference_2);
}

// Custom function to detect edges
float DetectEdges_float(float2 uv, float outlineThickness, float depthThreshold, out float Out)
{
    float2 texel_size = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
    
    const float half_width_f = floor(outlineThickness * 0.5);
    const float half_width_c = ceil(outlineThickness * 0.5);

    float2 uvs[4];
    uvs[0] = uv + texel_size * float2(half_width_f, half_width_c) * float2(-1, 1);
    uvs[1] = uv + texel_size * float2(half_width_c, half_width_c) * float2(1, 1);
    uvs[2] = uv + texel_size * float2(half_width_f, half_width_f) * float2(-1, -1);
    uvs[3] = uv + texel_size * float2(half_width_c, half_width_f) * float2(1, -1);
    
    float depth_samples[4];
    for (int i = 0; i < 4; i++) {
        depth_samples[i] = SampleSceneDepth(uvs[i], _CameraDepthTexture);
    }
    
    float edge_depth = RobertsCross(depth_samples);
    Out = edge_depth > depthThreshold ? 1.0 : 0.0;
}

#endif // EDGE_DETECTION_HLSL