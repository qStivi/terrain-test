using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class HeightMapSettings : UpdatableData
    {
        public NoiseSettings noiseSettings;
        
        public bool usingFalloff;

        public float heightMultiplier = 20;
        public AnimationCurve heightCurve;

        public float minHeight
        {
            get
            {
                var height = heightMultiplier * heightCurve.Evaluate(0);
                return height;
            }
        }

        public float maxHeight
        {
            get
            {
                var height = heightMultiplier * heightCurve.Evaluate(1);
                return height;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            noiseSettings.ValidateValues();
            base.OnValidate();
        }
#endif

    }
}
