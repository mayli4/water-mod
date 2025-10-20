#include "../pixelation.h"
#include "../common.h"
#include "../dithering.h"
#include "../rand.h"

sampler uImage0 : register(s0);

texture uTexture;
sampler tex0 = sampler_state
{
    texture = <uTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

texture uWaterNoiseTexture;
sampler texWaterNoise = sampler_state
{
    texture = <uWaterNoiseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};


#define NUM_STARS 60

float time;
float4 source;
float pixel;

float4 color;
float hover_intensity;
bool small_panel;

float daylight_intensity;

float water_scroll_speed;
float water_effect_power;
float wave_amplitude;
float wave_frequency;
float wave_speed;
float water_base_level;
float water_gradient_intensity;

float4 water_bottom_color_day;
float4 water_top_color_day;
float4 water_bottom_color_night;
float4 water_top_color_night;

float4 sky_gradient_bottom_color_day;
float4 sky_gradient_top_color_day;
float4 sky_gradient_bottom_color_night;
float4 sky_gradient_top_color_night;

float3 surface_line_color_day;
float3 surface_line_color_night;

float4 color_quantization_resolution;
float star_intensity;
float4 atmosphere_edge_color;
float atmosphere_curve_strength;

float4 cloud_color;
float cloud_density;
float cloud_scale;
float cloud_speed;

float gen_stars(float2 panelUV, float current_time, float panelAspectRatio, float maxBrightness) {
    float totalBrightness = 0.0f;
    float2 adjustedUV = float2(panelUV.x * panelAspectRatio, panelUV.y);

    for (int i = 0; i < NUM_STARS; i++) {
        float seed = float(i);
        float2 starPosRaw = float2(hash11(seed), hash11(seed + 123.456f));
        float2 adjustedStarPos = float2(starPosRaw.x * panelAspectRatio, starPosRaw.y);

        float flickerOffset = frac(sin(seed * 0.1f) * 1000.0f);
        float flicker = smoothstep(0.0f, 1.0f, sin(current_time * 2.0f + flickerOffset * 6.28f) * 0.5f + 0.5f);

        float dist = distance(adjustedUV, adjustedStarPos);
        totalBrightness += smoothstep(0.015f, 0.005f, dist) * flicker * maxBrightness;
    }
    return totalBrightness;
}

float gen_clouds(float2 panelUV, float current_time, float cloudScale, float cloudSpeed) {
    float2 cloudUV = panelUV;
    cloudUV.x += current_time * cloudSpeed;
    cloudUV.y += current_time * cloudSpeed * 0.1f;
    cloudUV *= cloudScale;

    float noise = tex2D(tex0, cloudUV).r + tex2D(tex0, cloudUV * 2.0f).r * 0.5f;
    return pow(noise, 3.0f);
}

float4 main(float2 fragCoord : SV_POSITION, float2 tex_coords : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0 {
    float2 panelCoords = fragCoord - source.zw;
    float2 uv = normalize_with_pixelation(panelCoords, pixel, source.xy);
    uv.y = 1.0f - uv.y;
    float panelAspectRatio = source.x / source.y;

    float4 flatPanelBaseColor = small_panel ? color : baseColor;
    flatPanelBaseColor.rgb = (flatPanelBaseColor.rgb - 0.05f) + (hover_intensity * 0.1f);

    float4 currentSkyTop    = lerp(sky_gradient_top_color_night, sky_gradient_top_color_day, daylight_intensity);
    float4 currentSkyBottom = lerp(sky_gradient_bottom_color_night, sky_gradient_bottom_color_day, daylight_intensity);
    float4 currentWaterTop  = lerp(water_top_color_night, water_top_color_day, daylight_intensity);
    float4 currentWaterBottom = lerp(water_bottom_color_night, water_bottom_color_day, daylight_intensity);
    float3 currentSurfaceLine = lerp(surface_line_color_night, surface_line_color_day, daylight_intensity);

    float scrolledUVX = uv.x - time * water_scroll_speed;
    float totalWaveOffset = wave_amplitude * (
        sin(wave_frequency * scrolledUVX + time * wave_speed) +
        0.5f * sin(wave_frequency * 2.0f * scrolledUVX - time * wave_speed * 1.5f) +
        0.25f * sin(wave_frequency * 3.0f * scrolledUVX + time * wave_speed * 0.7f)
    );
    float waterSurfaceY = water_base_level + totalWaveOffset;

    float4 finalPixelColorMultiplier;

    float2 finalTexCoords = tex_coords; 

    float2 waterNoiseUV = (uv.xy * 5.0f) + (time * 0.1f);
    float waterNoise = tex2D(texWaterNoise, waterNoiseUV).r;
    
    float waterNoiseOffset = (waterNoise - 0.5f);


    if (uv.y < waterSurfaceY) {
        float wavyuv = uv.y + waterNoiseOffset * 0.10f;
        
        float waterGradientFactor = pow(saturate(wavyuv / max(0.001f, waterSurfaceY)), water_gradient_intensity);
        float4 gradientWaterColor = lerp(currentWaterBottom, currentWaterTop, waterGradientFactor);

        float distToSurface = waterSurfaceY - uv.y;
        float highlightStrength = 1.0f - smoothstep(0.0f, 0.05f * max(0.1f, wave_amplitude * 10.0f), distToSurface);
        float3 tintedWaterColor = lerp(gradientWaterColor.rgb, 1.0f.xxx, highlightStrength * 0.3f);

        finalPixelColorMultiplier.rgb = tintedWaterColor * water_effect_power;
        finalPixelColorMultiplier.a = gradientWaterColor.a;
        
    } else {
        finalPixelColorMultiplier = lerp(currentSkyBottom, currentSkyTop, uv.y);

        float horizontalEdgeFactor = pow(abs(uv.x - 0.5f) * 2.0f, 2.0f);
        finalPixelColorMultiplier = lerp(finalPixelColorMultiplier, atmosphere_edge_color, horizontalEdgeFactor * atmosphere_curve_strength);

        float starsFactor = gen_stars(uv, time, panelAspectRatio, star_intensity);
        starsFactor *= (1.0f - smoothstep(0.2f, 0.8f, daylight_intensity));
        finalPixelColorMultiplier.rgb += starsFactor;
        
        float cloudCoverage = gen_clouds(uv, time, cloud_scale, cloud_speed);
        float cloudFadeFactor = smoothstep(0.2f, 0.8f, daylight_intensity);
        float verticalCloudMask = smoothstep(0.5f, 0.7f, uv.y);
        float totalCloudBlend = cloudCoverage * cloud_density * cloudFadeFactor * verticalCloudMask;
        finalPixelColorMultiplier = lerp(finalPixelColorMultiplier, cloud_color, totalCloudBlend);

        float2 pixelatedCoords = floor(panelCoords / pixel);
        float dither = bayer_2x2(pixelatedCoords) - 0.5f;
        float quantizationStepSize = 1.0f / max(1.0f, color_quantization_resolution.x - 1.0f);
        finalPixelColorMultiplier.rgb += dither * quantizationStepSize;
    }

    float perturbedSurfaceY = waterSurfaceY + waterNoiseOffset * 0.05f;
    
    float distanceFromSurface = abs(uv.y - perturbedSurfaceY);
    float surfaceLineFactor = 1.0f - step(0.01f, distanceFromSurface);
    finalPixelColorMultiplier.rgb = lerp(finalPixelColorMultiplier.rgb, currentSurfaceLine, surfaceLineFactor * 0.9f);

    float4 outputColor = tex2D(uImage0, finalTexCoords) * finalPixelColorMultiplier;
    
    float4 safeResolutionDivisor = max(1.0f.xxxx, color_quantization_resolution - 1.0f);
    outputColor = floor(outputColor * color_quantization_resolution) / safeResolutionDivisor;
    
    return outputColor;
}

technique Technique1 {
    pass PanelShaderPass {
        PixelShader = compile ps_3_0 main();
    }
}