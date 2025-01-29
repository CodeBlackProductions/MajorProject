using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private int m_spawnAmountTeamA;

    [SerializeField][Range(0, 1)] private List<float> m_RangePercentTeamA;
    [SerializeField] private Material m_spawnMaterialA;
    [SerializeField] private List<Transform> m_SpawnsTeamA;
    [SerializeField] private int m_spawnAmountTeamB;
    [SerializeField][Range(0, 1)] private List<float> m_RangePercentTeamB;
    [SerializeField] private Material m_spawnMaterialB;
    [SerializeField] private List<Transform> m_SpawnsTeamB;

    [Header("Formation")]
    [SerializeField] private bool m_SpawnFormations;

    [SerializeField] private GameObject m_formationPrefab;

    [Header("Optional")]
    [SerializeField] private List<Transform> m_WaypointsTeamA = new List<Transform>();

    [SerializeField] private List<int> m_WaypointsPerSpawnTeamA = new List<int>();

    [SerializeField] private List<Transform> m_WaypointsTeamB = new List<Transform>();
    [SerializeField] private List<int> m_WaypointsPerSpawnTeamB = new List<int>();

    private EventManager m_EventManager = null;
    private List<KeyValuePair<Vector3, Transform>> m_WPTeamA = new List<KeyValuePair<Vector3, Transform>>();
    private List<KeyValuePair<Vector3, Transform>> m_WPTeamB = new List<KeyValuePair<Vector3, Transform>>();

    private void Start()
    {
        m_EventManager = EventManager.Instance;
        m_EventManager.SpawnNewWave += SpawnNewWave;
        m_EventManager.SpawnFormationAtPosition += SpawnNewFormation;
        m_EventManager.AssembleFormation += NewFormationFromBoids;

        int tempIndex = 0;

        for (int o = 0; o < m_SpawnsTeamA.Count; o++)
        {
            if (m_WaypointsPerSpawnTeamA.Count == 0 || m_WaypointsTeamA.Count == 0)
            {
                break;
            }
            for (int i = 0; i < m_WaypointsPerSpawnTeamA[o]; i++)
            {
                m_WPTeamA.Add(new KeyValuePair<Vector3, Transform>(m_SpawnsTeamA[o].position, m_WaypointsTeamA[tempIndex + i]));
            }
            tempIndex += m_WaypointsPerSpawnTeamA[o];
        }

        tempIndex = 0;

        for (int o = 0; o < m_SpawnsTeamB.Count; o++)
        {
            if (m_WaypointsPerSpawnTeamB.Count == 0 || m_WaypointsTeamB.Count == 0)
            {
                break;
            }
            for (int i = 0; i < m_WaypointsPerSpawnTeamB[o]; i++)
            {
                m_WPTeamB.Add(new KeyValuePair<Vector3, Transform>(m_SpawnsTeamB[o].position, m_WaypointsTeamB[tempIndex + i]));
            }
            tempIndex += m_WaypointsPerSpawnTeamB[o];
        }

        SpawnNewWave(Team.Ally);
        SpawnNewWave(Team.Enemy);
    }

    public void SpawnNewWave(Team _Team)
    {
        if (_Team == Team.Ally)
        {
            for (int i = 0; i < m_SpawnsTeamA.Count; i++) 
            {
                SpawnFormation(Team.Ally, m_spawnAmountTeamA, m_spawnMaterialA, m_SpawnFormations, m_SpawnsTeamA[i].position, m_WPTeamA, m_RangePercentTeamA[i]);
            }
        }
        else
        {
            for (int i = 0; i < m_SpawnsTeamB.Count; i++)
            {
                SpawnFormation(Team.Enemy, m_spawnAmountTeamB, m_spawnMaterialB, m_SpawnFormations, m_SpawnsTeamB[i].position, m_WPTeamB, m_RangePercentTeamB[i]);
            }
        }
    }

    public void SpawnNewFormation(Team _Team, Vector3 _Pos, bool _Ranged)
    {
        float rangePercent = 0;

        if (_Ranged)
        {
            rangePercent = 1;
        }

        if (_Team == Team.Ally)
        {
            SpawnFormation(Team.Ally, m_spawnAmountTeamA, m_spawnMaterialA, m_SpawnFormations, _Pos, m_WPTeamA, rangePercent);
        }
        else
        {
            SpawnFormation(Team.Enemy, m_spawnAmountTeamB, m_spawnMaterialB, m_SpawnFormations, _Pos, m_WPTeamB, rangePercent);
        }
    }

    private void SpawnFormation(Team _Team, int _SpawnAmount, Material _BoidMat, bool _SpawnFormations, Vector3 _SpawnPos, List<KeyValuePair<Vector3, Transform>> _OptionalWaypoints, float _RangePercent)
    {
        List<KeyValuePair<Guid, BoidDataManager>> boids = new List<KeyValuePair<Guid, BoidDataManager>>();
        GameObject formation = null;

        if (m_SpawnFormations)
        {
            formation = GameObject.Instantiate(m_formationPrefab);
            formation.transform.position = new Vector3(_SpawnPos.x, 1, _SpawnPos.z);
        }

        int meleeAmount = _SpawnAmount - (int)(_SpawnAmount * _RangePercent);

        for (int i = 0; i < _SpawnAmount; i++)
        {
            KeyValuePair<Guid, GameObject> temp;
            if (meleeAmount > i)
            {
                temp = BoidPool.Instance.GetNewBoid(false);
            }
            else
            {
                temp = BoidPool.Instance.GetNewBoid(true);
            }

            temp.Value.GetComponent<MeshRenderer>().material = _BoidMat;
            temp.Value.transform.position = _SpawnPos + transform.right * 10 * i;
            BoidDataManager tempManager = temp.Value.GetComponent<BoidDataManager>();
            tempManager.Team = _Team;

            if (_OptionalWaypoints.Count > 0)
            {
                for (int o = 0; o < _OptionalWaypoints.Count; o++)
                {
                    if (_OptionalWaypoints[o].Key == _SpawnPos)
                    {
                        tempManager.AddMovTarget(_OptionalWaypoints[o].Value.position);
                    }
                }
            }

            boids.Add(new KeyValuePair<Guid, BoidDataManager>(temp.Key, temp.Value.GetComponent<BoidDataManager>()));
            if (m_SpawnFormations)
            {
                formation.GetComponent<FormationBoidManager>().AddBoid(boids[i]);
                temp.Value.transform.position = formation.GetComponent<FormationDataManager>().QueryBoidPosition(i);
            }
        }

        if (m_SpawnFormations)
        {
            FormationDataManager dataManager = formation.GetComponent<FormationDataManager>();
            FormationBoidManager boidManager = formation.GetComponent<FormationBoidManager>();
            dataManager.UpdateBoidOffsets(boidManager.Boids.Count);
        }
    }

    public void NewFormationFromBoids(List<BoidDataManager> _Boids)
    {
        List<KeyValuePair<Guid, BoidDataManager>> allyBoids = new List<KeyValuePair<Guid, BoidDataManager>>();
        List<KeyValuePair<Guid, BoidDataManager>> EnemyBoids = new List<KeyValuePair<Guid, BoidDataManager>>();

        for (int i = 0; i < _Boids.Count; i++)
        {
            KeyValuePair<Guid, BoidDataManager> temp = new KeyValuePair<Guid, BoidDataManager>(_Boids[i].Guid, _Boids[i]);
            if (_Boids[i].Team == Team.Ally)
            {
                allyBoids.Add(temp);
            }
            else
            {
                EnemyBoids.Add(temp);
            }
            if (_Boids[i].FormationBoidManager != null)
            {
                _Boids[i].FormationBoidManager.RemoveBoid(temp);
            }
        }

        AssembleFormation(allyBoids);
        AssembleFormation(EnemyBoids);
    }

    private void AssembleFormation(List<KeyValuePair<Guid, BoidDataManager>> _Boids)
    {
        GameObject formation = null;

        if (_Boids.Count <= 0)
        {
            return;
        }

        formation = GameObject.Instantiate(m_formationPrefab);
        formation.transform.position = _Boids[0].Value.transform.position;
        FormationBoidManager boidManager = formation.GetComponent<FormationBoidManager>();

        for (int i = 0; i < _Boids.Count; i++)
        {
            boidManager.AddBoid(_Boids[i]);
        }

        FormationDataManager dataManager = formation.GetComponent<FormationDataManager>();
        dataManager.UpdateBoidOffsets(boidManager.Boids.Count);
    }
}