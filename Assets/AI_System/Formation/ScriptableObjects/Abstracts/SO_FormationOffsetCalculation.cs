using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SO_FormationOffsetCalculation: ScriptableObject
{
    public abstract Dictionary<int, Vector3> CalculateOffsets(int _UnitCount, float _WidthScale, float _DepthScale, float _UnitSize, float _UnitSpacing);

    protected abstract int[] SortOffsets(int[] _OverloadMeWithYourParameters);
}