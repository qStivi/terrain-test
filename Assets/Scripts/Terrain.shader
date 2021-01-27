Shader "Custom/Terrain"
{
    Properties
    {
        testTexture("Texture", 2D) = "white"{}
        testScale("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static  int max_layer_count = 8;
        const static float epsilon = 1E-4;

        int layer_count;
        float3 base_colors[max_layer_count];
        float base_start_heights[max_layer_count];
        float base_blends[max_layer_count];
        float base_color_strength[max_layer_count];
        float base_texture_scales[max_layer_count];
        
        float min_height;
        float max_height;

        sampler2D test_texture;
        float test_scale;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverse_lerp(float a, float b, float value)
        {
            return saturate((value-a)/(b-a));
        }

        float3 tri_planar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            float3 scaled_world_pos = worldPos / scale;
            const float3 x_projection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaled_world_pos.y, scaled_world_pos.z, textureIndex)) * blendAxes.x;
            const float3 y_projection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaled_world_pos.x, scaled_world_pos.z, textureIndex)) * blendAxes.y;
            const float3 z_projection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaled_world_pos.x, scaled_world_pos.y, textureIndex)) * blendAxes.z;
            return x_projection + y_projection + z_projection;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            const float height_percent = inverse_lerp(min_height, max_height, IN.worldPos.y);
            float3 blend_axes = abs(IN.worldNormal);
            blend_axes /= blend_axes.x + blend_axes.y + blend_axes.z;
            for (int i = 0; i < layer_count; i++)
            {
                const float draw_strength = inverse_lerp(-base_blends[i]/2 - epsilon, base_blends[i]/2, height_percent -base_start_heights[i]);

                const float3 base_color = base_colors[i] * base_color_strength[i];
                const float3 texture_color = tri_planar(IN.worldPos, base_texture_scales[i], blend_axes, i) * (1-base_color_strength[i]);
                
                o.Albedo = o.Albedo * (1-draw_strength) + (base_color+texture_color) * draw_strength;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
