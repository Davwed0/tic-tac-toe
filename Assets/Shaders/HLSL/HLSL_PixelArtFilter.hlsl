//UNITY_SHADER_NO_UPGRADE
#ifndef PIXELATEINCLUDE_INCLUDED
#define PIXELATEINCLUDE_INCLUDED

void PixelArtFilter_float(float3 UV, int Downsampling, bool useBilinear, out float2 Out)
{
    
    float screenWidth = _ScreenParams.x;
    float screenHeight = _ScreenParams.y;
    
    float2 downsampledUV = (Downsampling, screenHeight / screenWidth * Downsampling);
    float2 pixelUV = floor(UV * downsampledUV) / downsampledUV;

    if (useBilinear) {
        // Calculate pixel position and fractional offset
        float2 pixelPos = UV * downsampledUV;
        int2 pixelPosInt = floor(pixelPos);
        float2 fracCoord = pixelPos - pixelPosInt;
        
        // Apply a sharper transition curve to the fractional coordinates
        // This reduces the amount of blending between pixels
        float2 sharpFrac = smoothstep(0.25, 0.75, fracCoord);
        
        // Calculate UVs for the four surrounding pixels
        float2 uvTopLeft = (pixelPosInt) / downsampledUV;
        float2 uvTopRight = (pixelPosInt + float2(1, 0)) / downsampledUV;
        float2 uvBottomLeft = (pixelPosInt + float2(0, 1)) / downsampledUV;
        float2 uvBottomRight = (pixelPosInt + float2(1, 1)) / downsampledUV;
        
        // Blend between the pixels based on the sharper fractional position
        float2 topRow = lerp(uvTopLeft, uvTopRight, sharpFrac.x);
        float2 bottomRow = lerp(uvBottomLeft, uvBottomRight, sharpFrac.x);
        pixelUV = lerp(topRow, bottomRow, sharpFrac.y);
    }

    Out = pixelUV;
}

#endif //PIXELATEINCLUDE_INCLUDED