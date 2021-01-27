using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class TextureData : UpdatableData
    {
        private const int TextureSize = 512;
        private const TextureFormat TextureFormat = UnityEngine.TextureFormat.RGB565;
        private static readonly int LayerCount = Shader.PropertyToID("layer_count");
        private static readonly int BaseColors = Shader.PropertyToID("base_colors");
        private static readonly int BaseStartHeights = Shader.PropertyToID("base_start_heights");
        private static readonly int BaseBlends = Shader.PropertyToID("base_blends");
        private static readonly int BaseColorStrength = Shader.PropertyToID("base_color_strength");
        private static readonly int BaseTextureScales = Shader.PropertyToID("base_texture_scales");
        private static readonly int BaseTextures = Shader.PropertyToID("baseTextures");
        private static readonly int MINHeight = Shader.PropertyToID("min_height");
        private static readonly int MAXHeight = Shader.PropertyToID("max_height");

        public Layer[] layers;

        private float _savedMaxHeight;

        private float _savedMinHeight;

        public void ApplyToMaterial(Material material)
        {
            ReapplyShader(material);

            material.SetInt(LayerCount, layers.Length);
            material.SetColorArray(BaseColors, layers.Select(x => x.tint).ToArray());
            material.SetFloatArray(BaseStartHeights, layers.Select(x => x.startHeight).ToArray());
            material.SetFloatArray(BaseBlends, layers.Select(x => x.blendStrength).ToArray());
            material.SetFloatArray(BaseColorStrength, layers.Select(x => x.tintStrength).ToArray());
            material.SetFloatArray(BaseTextureScales, layers.Select(x => x.textureScale).ToArray());
            var texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
            material.SetTexture(BaseTextures, texturesArray);

            UpdateMeshHeights(material, _savedMinHeight, _savedMaxHeight);
        }

        private static void ReapplyShader(Material material)
        {
            var shader = material.shader;
            material.shader = null;
            material.shader = shader;
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            _savedMaxHeight = maxHeight;
            _savedMinHeight = minHeight;

            material.SetFloat(MINHeight, minHeight);
            material.SetFloat(MAXHeight, maxHeight);
        }

        private static Texture2DArray GenerateTextureArray(IReadOnlyList<Texture2D> textures)
        {
            var textureArray = new Texture2DArray(TextureSize, TextureSize, textures.Count, TextureFormat, true);
            for (var i = 0; i < textures.Count; i++) textureArray.SetPixels(textures[i].GetPixels(), i);
            textureArray.Apply();
            return textureArray;
        }
    }

    [Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;

        [Range(0, 1)] public float tintStrength;

        [Range(0, 1)] public float startHeight;

        [Range(0, 1)] public float blendStrength;

        public float textureScale;
    }
}
