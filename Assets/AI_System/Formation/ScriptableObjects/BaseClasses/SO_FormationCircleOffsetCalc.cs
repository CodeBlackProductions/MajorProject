using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "SO_CircleFormation", menuName = "Formation/SO_CircleFormation")]
public class SO_FormationCircleOffsetCalc : SO_FormationOffsetCalculation
{
    public override Dictionary<int, Vector3> CalculateOffsets(int _UnitCount, float _WidthScale, float _DepthScale, float _UnitSize, float _UnitSpacing)
    {
        Vector3[] gridOffsets = new Vector3[_UnitCount];

        int[] sortedIndices = new int[_UnitCount];

        for (int i = 0; i < _UnitCount; i++)
        {
            sortedIndices[i] = i;
        }

        float radiusIncrement = _UnitSize + (_UnitSize * _UnitSpacing);
        float currentRadius = radiusIncrement;
        int currentCircle = 0;

        int unitsPlaced = 0;

        while (unitsPlaced < _UnitCount)
        {
            int unitsInCircle = Mathf.FloorToInt((2 * Mathf.PI * currentRadius) / radiusIncrement);
            unitsInCircle = Mathf.Max(1, unitsInCircle);

            if (unitsPlaced + unitsInCircle > _UnitCount)
            {
                unitsInCircle = _UnitCount - unitsPlaced;
            }

            for (int i = 0; i < unitsInCircle; i++)
            {
                double angle = (2 * Math.PI / unitsInCircle) * i;

                float x = (float)(Math.Cos(angle) * currentRadius);
                float y = (float)(Math.Sin(angle) * currentRadius);

                gridOffsets[unitsPlaced] = new Vector3(x, 0, y);
                unitsPlaced++;
            }

            currentCircle++;
            currentRadius += radiusIncrement;
        }

        sortedIndices = SortOffsets(sortedIndices, gridOffsets, 0, 0);

        Dictionary<int, Vector3> boidOffsets = new Dictionary<int, Vector3>();

        for (int i = 0; i < gridOffsets.Length; i++)
        {
            int index = sortedIndices[i];
            boidOffsets[index] = gridOffsets[i];
        }

        return boidOffsets;
    }

    protected int[] SortOffsets(int[] _IndicesToSort, Vector3[] _Offsets, float _CenterX, float _CenterY)
    {
        int[] sortedIndices = _IndicesToSort;

        Array.Sort(sortedIndices, (a, b) =>
        {
            float distA = Mathf.Sqrt(Mathf.Pow(_Offsets[a].x - _CenterX, 2) + Mathf.Pow(_Offsets[a].z - _CenterY, 2));
            float distB = Mathf.Sqrt(Mathf.Pow(_Offsets[b].x - _CenterX, 2) + Mathf.Pow(_Offsets[b].z - _CenterY, 2));
            int distanceComparison = distA.CompareTo(distB);

            if (distanceComparison != 0)
            {
                return distanceComparison;
            }
            else
            {
                return a.CompareTo(b);
            }
        });

        return sortedIndices;
    }

    protected override int[] SortOffsets(int[] _OverloadMeWithYourParameters)
    {
        throw new System.NotImplementedException();
    }
}