using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class TextureData : UpdatableData
    {
        private const int textureSize = 512;
        private const TextureFormat textureFormat = TextureFormat.RGB565;

        public Layer[] layers;

        float savedMaxHeight;

        float savedMinHeight;

        public void ApplyToMaterial(Material material)
        {
            ReapplyShader(material);

            material.SetInt("layerCount", layers.Length);
            material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
            material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
            material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
            material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrength).ToArray());
            material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
            var testuresArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
            material.SetTexture("baseTextures", testuresArray);

            UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
        }

        private static void ReapplyShader(Material material)
        {
            var shader = material.shader;
            material.shader = null;
            material.shader = shader;
        }

        public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
        {
            savedMaxHeight = maxHeight;
            savedMinHeight = minHeight;

            material.SetFloat("minHeight", minHeight);
            material.SetFloat("maxHeight", maxHeight);
        }

        Texture2DArray GenerateTextureArray(Texture2D[] textures)
        {
            var texttureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
            for (var i = 0; i < textures.Length; i++)
            {
                texttureArray.SetPixels(textures[i].GetPixels(), i);
            }
            texttureArray.Apply();
            return texttureArray;
        }
    }

    [Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0,1)]
        public float tintStrength;
        [Range(0,1)]
        public float startHeight;
        [Range(0,1)]
        public float blendStrength;
        public float textureScale;
    }
}
