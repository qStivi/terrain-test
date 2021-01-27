using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class MeshSettings : UpdatableData
    {
        public const int NumSupportedLODs = 5;
        public const int NumSupportedChunkSizes = 10;
        public const int NumSupportedFlatShadedChunkSizes = 4;
        public static readonly int[] SupportedChunkSizes = {24, 48, 72, 96, 120, 144, 168, 192, 216, 240};


        public float meshScale = 3f;
        public bool usingFlatShading;

        [Range(0, NumSupportedChunkSizes - 1)] public int chunkSizeIndex;

        [Range(0, NumSupportedFlatShadedChunkSizes - 1)]
        public int flatShadedChunkSizeIndex;

        // num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals
        public int NumVertsPerLine => SupportedChunkSizes[usingFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;

        public float MeshWorldSize => (NumVertsPerLine - 3) * meshScale;
    }
}
