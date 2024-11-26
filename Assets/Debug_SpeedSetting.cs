using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_SpeedSetting : MonoBehaviour
{
    [SerializeField] private float m_Speed = 1;

    private void Start()
    {
         Time.timeScale = m_Speed;
    }
}
