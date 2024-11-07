using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

    [SerializeField][HideInInspector] private string m_LevelName;

    [SerializeField][HideInInspector] private int m_width = 0;

    [SerializeField][HideInInspector] private int m_height = 0;

    [SerializeField][HideInInspector] private List<StaticCostField> m_StaticCostKey = new List<StaticCostField>();

    [SerializeField][HideInInspector] private List<TwoDimensionalArray<float>> m_StaticCostVal = new List<TwoDimensionalArray<float>>();

    private Dictionary<string, Vector2[,]> m_LoadedGrids = new Dictionary<string, Vector2[,]>();

    public string LevelName { get => m_LevelName; }
    public bool Calculate { get => m_Calculate; }

    public void Precompute()
    {
        m_StaticCostKey.Clear();
        m_StaticCostVal.Clear();

        m_LevelName = SceneManager.GetActiveScene().name;

        GridTile[,] grid = GridDataManager.Instance.BoidGrid;
        m_width = grid.GetLength(0);
        m_height = grid.GetLength(1);
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
            for (int x = 0; x < m_width; x++)
            {
                for (int y = 0; y < m_height; y++)
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

        ConcurrentDictionary<Vector2Int, Vector2[,]> rawGrids = new ConcurrentDictionary<Vector2Int, Vector2[,]>();
        Dictionary<Vector2Int, Texture2D> grids = new Dictionary<Vector2Int, Texture2D>();

        Enumerable.Range(0, m_width * m_height).AsParallel().ForAll(i =>
        {
            int x = i / m_height;
            int y = i % m_height;

            Vector2Int vec = new Vector2Int(x, y);

            float[,] temp = (float[,])costFieldTotal.Clone();
            temp[x, y] = 0;

            float[,] integrationField = FlowfieldPathfinding.CalculateIntegrationField(temp, vec);
            Vector2[,] flowField = FlowfieldPathfinding.CalculateFlowField(integrationField, vec);

            rawGrids.TryAdd(vec, flowField);
        });

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/" + m_LevelName))
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/" + m_LevelName);
        }

        foreach (KeyValuePair<Vector2Int, Vector2[,]> data in rawGrids)
        {
            string path = Application.persistentDataPath + "/" + m_LevelName + "/" + m_LevelName + "_" + data.Key + ".bin";
            SaveToBinary(data.Value, path);
        }

        m_Calculate = false;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    public Vector2[,] GetPrecomputedFlowfield(Vector2Int _TargetPos)
    {
        string path = FindGrid(_TargetPos);
        Vector2[,] grid = LoadFromBinary(path);
        return grid;
    }

    private string FindGrid(Vector2Int _TargetPos)
    {
        string path = Application.persistentDataPath + "/" + m_LevelName + "/" + m_LevelName + "_" + _TargetPos + ".bin";

        return path;
    }

    public void SaveToBinary(Vector2[,] _Grid, string _Path)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(_Path, FileMode.Create)))
        {
            writer.Write(_Grid.GetLength(0));
            writer.Write(_Grid.GetLength(1));
            for (int i = 0; i < _Grid.GetLength(0); i++)
            {
                for (int j = 0; j < _Grid.GetLength(1); j++)
                {
                    writer.Write(_Grid[i, j].x);
                    writer.Write(_Grid[i, j].y);
                }
            }
        }
    }

    public Vector2[,] LoadFromBinary(string _Path)
    {
        if (m_LoadedGrids.ContainsKey(_Path))
        {
            return m_LoadedGrids[_Path];
        }

        if (!File.Exists(_Path))
        {
            Debug.LogError($"File not found at {_Path}");
            return null;
        }

        try
        {
            using (BinaryReader reader = new BinaryReader(File.Open(_Path, FileMode.Open)))
            {
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                Vector2[,] grid = new Vector2[width, height];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        grid[i, j] = new Vector2(x, y);
                    }
                }

                m_LoadedGrids.Add(_Path, grid);

                return grid;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load file at {_Path}: {e.Message}");
            return null;
        }
    }
}