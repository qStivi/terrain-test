using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class TextureData : UpdatableData
    {
        public Color[] baseColors;
        [Range(0, 1)] public float[] baseStartHeights;

        float savedMaxHeight;

        float savedMinHeight;

        public void ApplyToMaterial(Material material)
        {
            ReapplyShader(material);

            material.SetInt("baseColorCount", baseColors.Length);
            material.SetColorArray("baseColors", baseColors);
            material.SetFloatArray("baseStartHeights", baseStartHeights);

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
    }
}
