using UnityEngine;

namespace Procedural_Generation_2._0
{
    public class MapDisplay : MonoBehaviour
    {
        public Renderer textureRenderer;

        public void DrawNoiseMap(float[,] noiseMap)
        {
            // Create texture with resolution of generated noise map.
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);
            var texture = new Texture2D(width, height);

            // Set color of each pixel using the values from our generated noise map.
            var colorMap = new Color[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);

            texture.SetPixels(colorMap);
            texture.Apply();

            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(width, 1, height);
        }
    }
}