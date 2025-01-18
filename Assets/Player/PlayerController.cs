using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private float m_CameraSpeed = 100;
    [SerializeField] private float m_CameraZoomDistance = 20;
    [SerializeField] private int m_CameraMaxZoomLevel = 5;
    [SerializeField] private float m_ClickSelectionRadius = 1;
    [SerializeField] private float m_DownTimeToDrag = 0.2f;
    [SerializeField] private Material m_SelectionMat = null;
    [SerializeField] private bool m_AllowFreeSpawn = false;

    private EventManager m_EventManager;
    private Camera m_Camera;
    private Vector3 m_CamPos = Vector3.zero;
    private Vector3 m_CamTargetDir = Vector3.zero;
    private Vector3 m_CamZoomTarget = Vector3.zero;
    private bool m_Moving = false;

    private int m_CurrentZoomStep = 0;
    private int m_PreviousZoomStep = 0;
    private bool m_LeftIsDown = false;
    private float m_LeftDownTime = 0;
    private bool m_RightIsDown = false;
    private float m_RightDownTime = 0;

    private Vector3 m_LeftDragStart = Vector3.zero;
    private Vector3 m_LeftDragEnd = Vector3.zero;
    private GameObject m_DragVisuals = null;

    private bool m_UnitsSeclected = false;
    private bool m_FormationModeActive = false;

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

    private void Start()
    {
        if (EventManager.Instance)
        {
            m_EventManager = EventManager.Instance;
            m_EventManager.PlayerMove += Move;
            m_EventManager.PlayerScroll += Zoom;
            m_EventManager.PlayerLeftMouseDown += LeftClickDown;
            m_EventManager.PlayerLeftMouseUp += LeftClickUp;
            m_EventManager.PlayerRightMouseDown += RightClickDown;
            m_EventManager.PlayerRightMouseUp += RightClickUp;
            m_EventManager.PlayerFormationDown += FormationMode;
            m_EventManager.PlayerFormationUp += FormationModeEnd;
            m_EventManager.PlayerUnitsSelected += OnUnitSelectionChange;
            m_EventManager.Player1Up += FormationChangeSquare;
            m_EventManager.Player2Up += FormationChangeCircle;
        }

        m_Camera = Camera.main;
    }

    private void Update()
    {
        m_CamPos = m_Camera.transform.position;

        if (m_Moving)
        {
            float lerpFactor = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, Time.deltaTime * m_CameraSpeed));
            m_Camera.transform.position = Vector3.Lerp(m_CamPos, m_CamPos + m_CamTargetDir, Time.deltaTime * m_CameraSpeed * lerpFactor);
        }

        if (m_CamZoomTarget != Vector3.zero)
        {
            float lerpFactor = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, Time.deltaTime * m_CameraSpeed));
            m_Camera.transform.position = Vector3.Lerp(m_CamPos, m_CamZoomTarget, Time.deltaTime * 5 * lerpFactor);

            if (Vector3.Distance(m_CamZoomTarget, m_CamPos) <= 0.5f)
            {
                m_CamZoomTarget = Vector3.zero;
            }
        }

        if (m_LeftIsDown)
        {
            m_LeftDownTime += Time.deltaTime;

            if (m_LeftDownTime >= m_DownTimeToDrag)
            {
                if (m_DragVisuals != null)
                {
                    RaycastHit hit;
                    Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                    Physics.Raycast(ray, out hit);

                    Vector3 pos = (m_LeftDragStart + hit.point) * 0.5f;
                    pos += new Vector3(0, 0.25f, 0);
                    float scaleX = Mathf.Abs(m_LeftDragStart.x - hit.point.x) * 0.1f;
                    float scaleZ = Mathf.Abs(m_LeftDragStart.z - hit.point.z) * 0.1f;

                    Vector3 scale = new Vector3(scaleX, 1, scaleZ);

                    m_DragVisuals.transform.position = pos;
                    m_DragVisuals.transform.localScale = scale;
                }
                else
                {
                    m_DragVisuals = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    m_DragVisuals.GetComponent<MeshRenderer>().material = m_SelectionMat;

                    RaycastHit hit;
                    Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                    Physics.Raycast(ray, out hit);

                    Vector3 pos = (m_LeftDragStart + hit.point) * 0.5f;
                    pos += new Vector3(0, 0.25f, 0);
                    float scaleX = Mathf.Abs(m_LeftDragStart.x - hit.point.x) * 0.1f;
                    float scaleZ = Mathf.Abs(m_LeftDragStart.z - hit.point.z) * 0.1f;
                    Vector3 scale = new Vector3(scaleX, 1, scaleZ);

                    m_DragVisuals.transform.position = pos;
                    m_DragVisuals.transform.localScale = scale;
                }
            }
        }

        if (m_RightIsDown)
        {
            m_RightDownTime += Time.deltaTime;
        }
    }

    private void Move(Vector2 _MovDir)
    {
        if (_MovDir == Vector2.zero || !m_Camera)
        {
            m_Moving = false;
            return;
        }

        m_CamTargetDir = new Vector3(_MovDir.x, 0, _MovDir.y);
        m_Moving = true;
    }

    private void Zoom(float _ZoomVal)
    {
        if ((m_CurrentZoomStep < m_CameraMaxZoomLevel && _ZoomVal > 0) || (m_CurrentZoomStep > -m_CameraMaxZoomLevel && _ZoomVal < 0))
        {
            m_PreviousZoomStep = m_CurrentZoomStep;
            m_CurrentZoomStep += (int)_ZoomVal;
            Mathf.Clamp(m_CurrentZoomStep, -m_CameraMaxZoomLevel, m_CameraMaxZoomLevel);

            float zoomchange = (m_CurrentZoomStep - m_PreviousZoomStep) * m_CameraZoomDistance;

            if (m_CamZoomTarget == Vector3.zero)
            {
                m_CamZoomTarget = m_CamPos + m_Camera.transform.forward * zoomchange;
            }
            else
            {
                m_CamZoomTarget = m_CamZoomTarget + m_Camera.transform.forward * zoomchange;
            }
        }
    }

    private void LeftClickDown(bool _ShiftDown, bool _CtrlDown, bool _SpaceDown)
    {
        m_LeftIsDown = true;

        if (!_SpaceDown)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            m_LeftDragStart = hit.point;
        }
    }

    private void LeftClickUp(bool _ShiftDown, bool _CtrlDown, bool _SpaceDown)
    {
        m_LeftIsDown = false;

        if (!m_AllowFreeSpawn || !_SpaceDown)
        {
            if (m_LeftDownTime < m_DownTimeToDrag)
            {
                RaycastHit hit;
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit);

                Collider[] hits = Physics.OverlapSphere(hit.point, m_ClickSelectionRadius, LayerMask.GetMask("Boid"));

                if (hits.Length > 0)
                {
                    hits.OrderBy(h => Vector3.Distance(h.transform.position, hit.point));

                    BoidDataManager boid;
                    if (hits[0].gameObject.TryGetComponent<BoidDataManager>(out boid))
                    {
                        Guid[] guids = new Guid[] { boid.Guid };

                        if (_CtrlDown)
                        {
                            UnitSelectionHandler.Instance.OnUnitDeselect(guids);
                        }
                        else
                        {
                            UnitSelectionHandler.Instance.OnUnitSelect(_ShiftDown, guids);
                        }
                    }
                }
                else if (!_CtrlDown && !_ShiftDown)
                {
                    UnitSelectionHandler.Instance.ClearSelection();
                }
            }
            else
            {
                RaycastHit hit;
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit);
                m_LeftDragEnd = hit.point;

                Vector3 center = (m_LeftDragEnd + m_LeftDragStart) * 0.5f;
                float halfExtentX = Mathf.Abs(m_LeftDragStart.x - m_LeftDragEnd.x) * 0.5f;
                float halfExtentZ = Mathf.Abs(m_LeftDragStart.z - m_LeftDragEnd.z) * 0.5f;

                Vector3 halfExtent = new Vector3(halfExtentX, 1, halfExtentZ);

                Collider[] hits = Physics.OverlapBox(center, halfExtent, Quaternion.identity, LayerMask.GetMask("Boid"));

                if (hits.Length > 0)
                {
                    BoidDataManager boid;
                    List<Guid> guids = new List<Guid>();

                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (hits[i].gameObject.TryGetComponent<BoidDataManager>(out boid))
                        {
                            guids.Add(boid.Guid);
                        }
                    }

                    if (_CtrlDown)
                    {
                        UnitSelectionHandler.Instance.OnUnitDeselect(guids.ToArray());
                    }
                    else
                    {
                        UnitSelectionHandler.Instance.OnUnitSelect(_ShiftDown, guids.ToArray());
                    }
                }
                else if (!_CtrlDown && !_ShiftDown)
                {
                    UnitSelectionHandler.Instance.ClearSelection();
                }

                GameObject.Destroy(m_DragVisuals);
                m_DragVisuals = null;
            }
        }
        else if (m_AllowFreeSpawn)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            EventManager.Instance?.SpawnFormationAtPosition.Invoke(Team.Ally, hit.point);
        }

        m_LeftDownTime = 0;
    }

    private void RightClickDown(bool _ShiftDown, bool _CtrlDown, bool _SpaceDown)
    {
        m_RightIsDown = true;
    }

    private void RightClickUp(bool _ShiftDown, bool _CtrlDown, bool _SpaceDown)
    {
        m_RightIsDown = false;

        if (!m_AllowFreeSpawn || !_SpaceDown)
        {
            if (m_RightDownTime < 0.2f)
            {
                RaycastHit hit;
                Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out hit);

                UnitSelectionHandler.Instance.OnGiveMoveOrder(_ShiftDown, hit.point);
            }
        }
        else if (m_AllowFreeSpawn)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            EventManager.Instance?.SpawnFormationAtPosition.Invoke(Team.Enemy, hit.point);
        }
        m_RightDownTime = 0;
    }

    private void OnUnitSelectionChange(bool _UnitsSelected)
    {
        m_UnitsSeclected = _UnitsSelected;
    }

    private void FormationMode()
    {
        if (m_UnitsSeclected)
        {
            m_FormationModeActive = true;
        }
    }

    private void FormationModeEnd()
    {
        m_FormationModeActive = false;
    }

    private void FormationChangeSquare()
    {
        if (m_FormationModeActive)
        {
            UnitSelectionHandler.Instance?.OnFormationChange("Square");
        }
    }

    private void FormationChangeCircle()
    {
        if (m_FormationModeActive)
        {
            UnitSelectionHandler.Instance?.OnFormationChange("Circle");
        }
    }
}