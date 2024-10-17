using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_NewCostfield", menuName = "Flowfields/SO_CostfieldBase")]
public class SO_CostfieldBase : ScriptableObject
{
    private bool m_Calculated = false;

    private float[,] m_Field;

    public float[,] Field { get => m_Field; set => m_Field = value; }
    public bool Calculated { get => m_Calculated; set => m_Calculated = value; }
}
