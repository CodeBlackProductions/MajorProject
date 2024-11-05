using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StaticCostField
{
    Obstacle, Terrain, PointOfInterest
}

[Serializable]
internal class TwoDimensionalArray<T>
{
    private T[] m_Array;
    private int m_Width;
    private int m_Height;

    public TwoDimensionalArray(T[,] _Array)
    {
        m_Width = _Array.GetLength(0);
        m_Height = _Array.GetLength(1);

        m_Array = Convert2DTo1D(_Array);
    }

    public T[,] GetArray()
    {
        return Convert1DTo2D(m_Array);
    }

    private T[] Convert2DTo1D(T[,] _Array2D)
    {
        T[] array1D = new T[m_Width * m_Height];

        for (int x = 0; x < m_Width; x++)
        {
            for (int y = 0; y < m_Height; y++)
            {
                int index = To1DIndex(x, y);
                array1D[index] = _Array2D[x, y];
            }
        }

        return array1D;
    }

    private T[,] Convert1DTo2D(T[] _Array1D)
    {
        T[,] array2D = new T[m_Width, m_Height];

        for (int x = 0; x < m_Width; x++)
        {
            for (int y = 0; y < m_Height; y++)
            {
                int index = To1DIndex(x, y);
                array2D[x, y] = _Array1D[index];
            }
        }

        return array2D;
    }

    private int To1DIndex(int _X, int _Y)
    {
        return _X + _Y * m_Width;
    }
}

[CreateAssetMenu(fileName = "SO_NewFlowfieldDatabase", menuName = "Flowfield/SO_FlowfieldDatabase")]
public class SO_FlowfieldDatabase : ScriptableObject
{
    [SerializeField] private bool m_Calculate = false;

    [SerializeField] private string m_LevelName;

    [SerializeField][HideInInspector] private int width = 0;

    [SerializeField][HideInInspector] private int height = 0;

    [SerializeField] private List<StaticCostField> m_StaticCostKey = new List<StaticCostField>();

    [SerializeField] private List<TwoDimensionalArray<float>> m_StaticCostVal = new List<TwoDimensionalArray<float>>();

    [SerializeField] private List<Vector2> m_StaticIntegrationKey = new List<Vector2>();

    [SerializeField] private List<TwoDimensionalArray<float>> m_StaticIntegrationVal = new List<TwoDimensionalArray<float>>();

    [SerializeField] private List<Vector2> m_StaticFlowKey = new List<Vector2>();

    [SerializeField] private List<TwoDimensionalArray<Vector2>> m_StaticFlowVal = new List<TwoDimensionalArray<Vector2>>();

    public string LevelName { get => m_LevelName; }
    public bool Calculate { get => m_Calculate; }

    public void Precompute()
    {
        m_StaticCostKey.Clear();
        m_StaticCostVal.Clear();
        m_StaticIntegrationKey.Clear();
        m_StaticIntegrationVal.Clear();
        m_StaticFlowKey.Clear();
        m_StaticFlowVal.Clear();

        m_LevelName = SceneManager.GetActiveScene().name;

        GridTile[,] grid = GridDataManager.Instance.BoidGrid;
        width = grid.GetLength(0);
        height = grid.GetLength(1);
        float[,] costFieldTotal;

        m_StaticCostKey.Add(StaticCostField.Obstacle);
        m_StaticCostVal.Add(new TwoDimensionalArray<float>(FlowfieldPathfinding.CalculateCostField(grid)));

        costFieldTotal = m_StaticCostVal[m_StaticCostKey.IndexOf(StaticCostField.Obstacle)].GetArray();

        float[,] terrainCost = null;
        float[,] poiCost = null;

        if (m_StaticCostKey.Contains(StaticCostField.Terrain))
        {
            terrainCost = m_StaticCostVal[m_StaticCostKey.IndexOf(StaticCostField.Terrain)].GetArray();
        }

        if (m_StaticCostKey.Contains(StaticCostField.PointOfInterest))
        {
            poiCost = m_StaticCostVal[m_StaticCostKey.IndexOf(StaticCostField.PointOfInterest)].GetArray();
        }

        if (terrainCost != null || poiCost != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (terrainCost != null)
                    {
                        costFieldTotal[x, y] += terrainCost[x, y];
                    }
                    if (poiCost != null)
                    {
                        costFieldTotal[x, y] += poiCost[x, y];
                    }
                }
            }
        }

        Enumerable.Range(0, width * height).AsParallel().ForAll(i =>
        {
            int x = i / height;
            int y = i % height;

            Vector2Int vec = new Vector2Int(x, y);

            float[,] temp = (float[,])costFieldTotal.Clone();
            temp[x, y] = 0;

            float[,] integrationField = FlowfieldPathfinding.CalculateIntegrationField(temp, vec);
            Vector2[,] flowField = FlowfieldPathfinding.CalculateFlowField(integrationField, vec);

            lock (m_StaticIntegrationKey)
            {
                m_StaticIntegrationKey.Add(vec);
                m_StaticIntegrationVal.Add(new TwoDimensionalArray<float>(integrationField));

                m_StaticFlowKey.Add(vec);
                m_StaticFlowVal.Add(new TwoDimensionalArray<Vector2>(flowField));
            }
        });

        m_Calculate = false;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    public float[,] GetPrecomputedCostfield(StaticCostField _Costfield)
    {
        if (m_StaticCostKey.Contains(_Costfield))
        {
            return m_StaticCostVal[m_StaticCostKey.IndexOf(_Costfield)].GetArray();
        }

        return null;
    }

    public float[,] GetPrecomputedIntegrationField(Vector2Int _TargetPos)
    {
        if (m_StaticIntegrationKey.Contains(_TargetPos))
        {
            return m_StaticIntegrationVal[m_StaticIntegrationKey.IndexOf(_TargetPos)].GetArray();
        }

        return null;
    }

    public Vector2[,] GetPrecomputedFlowField(Vector2Int _TargetPos)
    {
        if (m_StaticFlowKey.Contains(_TargetPos))
        {
            int idx = m_StaticFlowKey.IndexOf(_TargetPos);
            return m_StaticFlowVal[idx].GetArray();
        }

        return null;
    }
}