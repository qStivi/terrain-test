using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 3f;


        public bool usingFlatShading;


        public bool usingFalloff;

        public float meshHeightMultiplier = 20;
        public AnimationCurve meshHeightCurve;

        public float minHeight
        {
            get
            {
                var height = uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
                return height;
            }
        }

        public float maxHeight
        {
            get
            {
                var height = uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
                return height;
            }
        }
    }
}
