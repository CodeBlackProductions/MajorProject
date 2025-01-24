using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour
{
    public static UnitSelectionHandler Instance;

    [SerializeField] private bool m_AllowEnemySelection = true;

    private List<BoidDataManager> m_CurrentSelection = new List<BoidDataManager>();

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

    public void OnUnitSelect(bool _Additive, Guid[] _Guids)
    {
        AddToSelection(_Additive, _Guids);
    }

    public void OnUnitDeselect(Guid[] _Guids)
    {
        RemoveFromSelection(_Guids);
    }

    private void AddToSelection(bool _Additive, Guid[] _Guids)
    {
        BoidDataManager temp;
        List<BoidDataManager> formationBoids = new List<BoidDataManager>();

        if (!_Additive)
        {
            ClearSelection();
        }

        for (int i = 0; i < _Guids.Length; i++)
        {
            temp = BoidPool.Instance.GetActiveBoid(_Guids[i]).GetComponent<BoidDataManager>();
            if (m_AllowEnemySelection)
            {
                if (temp != null && !m_CurrentSelection.Contains(temp))
                {
                    m_CurrentSelection.Add(temp);
                    UpdateBoidStatus(temp, true);
                    CheckForFormation(temp, ref formationBoids);
                }
            }
            else
            {
                if (temp != null && !m_CurrentSelection.Contains(temp) && temp.Team == Team.Ally)
                {
                    m_CurrentSelection.Add(temp);
                    UpdateBoidStatus(temp, true);
                    CheckForFormation(temp, ref formationBoids);
                }
            }
        }

        if (formationBoids.Count > 0)
        {
            for (int i = 0; i < formationBoids.Count; i++)
            {
                if (formationBoids[i] != null && !m_CurrentSelection.Contains(formationBoids[i]))
                {
                    m_CurrentSelection.Add(formationBoids[i]);
                    UpdateBoidStatus(formationBoids[i], true);
                }
            }
        }

        SendSelectionStatus();
    }

    private void RemoveFromSelection(Guid[] _Guids)
    {
        BoidDataManager temp;

        List<BoidDataManager> formationBoids = new List<BoidDataManager>();

        for (int i = 0; i < _Guids.Length; i++)
        {
            temp = BoidPool.Instance.GetActiveBoid(_Guids[i]).GetComponent<BoidDataManager>();
            CheckForFormation(temp, ref formationBoids);
            UpdateBoidStatus(temp, false);
            m_CurrentSelection.Remove(temp);
        }

        if (formationBoids.Count > 0)
        {
            for (int i = 0; i < formationBoids.Count; i++)
            {
                if (formationBoids[i] != null && m_CurrentSelection.Contains(formationBoids[i]))
                {
                    UpdateBoidStatus(formationBoids[i], false);
                    m_CurrentSelection.Remove(formationBoids[i]);
                }
            }
        }

        SendSelectionStatus();
    }

    public void ClearSelection()
    {
        for (int i = 0; i < m_CurrentSelection.Count; i++)
        {
            UpdateBoidStatus(m_CurrentSelection[i], false);
        }
        m_CurrentSelection.Clear();
        SendSelectionStatus();
    }

    private void SendSelectionStatus()
    {
        EventManager.Instance?.PlayerUnitsSelected.Invoke(m_CurrentSelection.Count > 0);
    }

    private void UpdateBoidStatus(BoidDataManager _Boid, bool _Status)
    {
        _Boid.IsSelectedByPlayer = _Status;
    }

    private void CheckForFormation(BoidDataManager _Boid, ref List<BoidDataManager> _FormationBoids)
    {
        if (_Boid.FormationBoidManager)
        {
            for (int i = 0; i < _Boid.FormationBoidManager.Boids.Count; i++)
            {
                BoidDataManager temp = null;
                _Boid.FormationBoidManager.Boids[i].Value.TryGetComponent<BoidDataManager>(out temp);
                if (temp != null && temp != _Boid && !_FormationBoids.Contains(temp))
                {
                    _FormationBoids.Add(temp);
                }
            }
        }
    }

    public void OnFormationChange(string _Formation)
    {
        List<FormationDataManager> formations = new List<FormationDataManager>();
        for (int i = 0; i < m_CurrentSelection.Count; i++)
        {
            FormationDataManager temp = null;
            if (m_CurrentSelection[i].FormationBoidManager != null)
            {
                m_CurrentSelection[i].FormationBoidManager.TryGetComponent<FormationDataManager>(out temp);
                if (temp != null && !formations.Contains(temp))
                {
                    formations.Add(temp);
                }
            }
        }

        for (int i = 0; i < formations.Count; i++)
        {
            if (_Formation == "Square")
            {
                formations[i].SetFormationType(0);
            }
            else if (_Formation == "Circle")
            {
                formations[i].SetFormationType(1);
            }
        }
    }

    public void OnFormationDisband()
    {
        List<FormationBoidManager> formations = new List<FormationBoidManager>();
        for (int i = 0; i < m_CurrentSelection.Count; i++)
        {
            FormationBoidManager temp = m_CurrentSelection[i].FormationBoidManager;
            if (temp != null && !formations.Contains(temp))
            {
                formations.Add(temp);
            }
        }

        for (int i = 0; i < formations.Count; i++)
        {
            formations[i].DisbandFormation();
        }
    }

    public void OnFormationAssembly() 
    {
        EventManager.Instance?.AssembleFormation(m_CurrentSelection);
    }

    public void OnGiveMoveOrder(bool _Additive, Vector3 _TargetPos)
    {
        if (GridDataManager.Instance.IsInBounds((int)(_TargetPos.x / GridDataManager.Instance.CellSize), (int)(_TargetPos.z / GridDataManager.Instance.CellSize)))
        {
            if (_Additive)
            {
                for (int i = 0; i < m_CurrentSelection.Count; i++)
                {
                    m_CurrentSelection[i].AddMovTarget(_TargetPos);
                }
            }
            else
            {
                for (int i = 0; i < m_CurrentSelection.Count; i++)
                {
                    m_CurrentSelection[i].SetMovTarget(_TargetPos);
                }
            }
        }
    }
}