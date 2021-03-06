using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        var texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        // Create texture with resolution of given height map.
        var width = heightMap.Values.GetLength(0);
        var height = heightMap.Values.GetLength(1);

        // Set color of each pixel using the values from our generated noise map.
        var colorMap = new Color[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.MINValue, heightMap.MAXValue, heightMap.Values[x, y]));

        return TextureFromColorMap(colorMap, width, height);
    }
}
