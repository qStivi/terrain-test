using UnityEditor;
using UnityEngine;

namespace Procedural_Generation_2._0
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var mapGenerator = (MapGenerator) target;

            if (DrawDefaultInspector())
                if (mapGenerator.autoUpdate)
                    mapGenerator.GenerateMap();

            if (GUILayout.Button("Generate")) mapGenerator.GenerateMap();
        }
    }
}