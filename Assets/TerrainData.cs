using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData
{
    public float uniformScale = 3f;

    
    public bool usingFlatShading;

    
    public bool usingFalloff;

    public float meshHeightMultiplier = 20;
    public AnimationCurve meshHeightCurve;

}
