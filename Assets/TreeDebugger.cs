using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TreeDebugger : MonoBehaviour
{
    public static TreeDebugger Instance;

    private Node m_Root;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void DrawDebug(Node root)
    {
        if (EditorApplication.isPaused)
        {
            return;
        }

        m_Root = root;

        DrawNode(root);
    }

    private void DrawNode(Node node)
    {
        if (node == null || node.Entry == null)
        {
            return;
        }

        if (node.Entry is Leaf)
        {
            CreateCube(node, "Leaf");
        }
        else if (node.Entry is Branch branch)
        {
            CreateCube(node, "Branch");
            for (int i = 0; i < branch.Children.Length; i++)
            {
                DrawNode(branch.Children[i]);
            }
        }
    }

    private void CreateCube(Node node, string name)
    {
        Rect rect = node.Entry.Rect;
        System.Numerics.Vector3 center = rect.GetCenter();
        System.Numerics.Vector3 size = rect.UpperRight - rect.LowerLeft;

        if (node.Entry is Leaf)
        {
            Gizmos.color = Color.green;
        }
        if (node == m_Root)
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireCube(ToUnityVector3(center), ToUnityVector3(size));
        Gizmos.color = Color.white;
    }

    private Vector3 ToUnityVector3(System.Numerics.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

}