using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_SquareFormation", menuName = "Formation/SO_SquareFormation")]
public class SO_FormationSquareOffsetCalc : SO_FormationOffsetCalculation
{
    public override Dictionary<int, Vector3> CalculateOffsets(int _UnitCount, float _WidthScale, float _DepthScale, float _UnitSize, float _UnitSpacing)
    {
        Vector3[] gridOffsets = new Vector3[_UnitCount];

        int[] sortedIndices = new int[_UnitCount];

        for (int i = 0; i < _UnitCount; i++)
        {
            sortedIndices[i] = i;
        }

        int numberOfRows = 0;
        int numberOfColumns = 0;

        numberOfRows = (int)Mathf.Sqrt(_UnitCount / (_WidthScale / _DepthScale));
        numberOfColumns = _UnitCount / numberOfRows;

        float centerX = (numberOfColumns - 1) * 0.5f;
        float centerY = (numberOfRows - 1) * 0.5f;

        for (int i = 0; i < _UnitCount; i++)
        {
            float col = (i % numberOfColumns) - (numberOfColumns - 1) * 0.5f;
            float row = (i / numberOfColumns) - (numberOfRows - 1) * 0.5f;
            gridOffsets[i] = new Vector3(col, 0, row);
        }

        sortedIndices = SortOffsets(sortedIndices, numberOfColumns, centerX, centerY);

        Dictionary<int,Vector3> boidOffsets = new Dictionary<int,Vector3>();

        for (int i = 0; i < gridOffsets.Length; i++)
        {
            int index = sortedIndices[i];

            boidOffsets[index] = gridOffsets[i] * (_UnitSize + _UnitSpacing);
        }

        return boidOffsets;
    }

    protected int[] SortOffsets(int[] _IndicesToSort, int _NumberOfColumns, float _CenterX, float _CenterY)
    {
        int[] sortedIndices = _IndicesToSort;

        Array.Sort(sortedIndices, (a, b) =>
        {
            float distA = Mathf.Sqrt(Mathf.Pow((a % _NumberOfColumns) - _CenterX, 2) + Mathf.Pow((a / _NumberOfColumns) - _CenterY, 2));
            float distB = Mathf.Sqrt(Mathf.Pow((b % _NumberOfColumns) - _CenterX, 2) + Mathf.Pow((b / _NumberOfColumns) - _CenterY, 2));
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
