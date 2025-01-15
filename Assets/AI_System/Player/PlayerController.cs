using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] private float m_CameraSpeed = 100;
    [SerializeField] private float m_CameraZoomDistance = 20;
    [SerializeField] private int m_CameraMaxZoomLevel = 5;
    [SerializeField] private float m_ClickSelectionRadius = 1;
    [SerializeField] private float m_DownTimeToDrag = 0.2f;
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

    private void LeftClickDown(bool _ShiftDown, bool _CtrlDown)
    {
        m_LeftIsDown = true;

        RaycastHit hit;
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);

        m_LeftDragStart = hit.point;
    }

    private void LeftClickUp(bool _ShiftDown, bool _CtrlDown)
    {
        m_LeftIsDown = false;

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
        }

        m_LeftDownTime = 0;
    }

    private void RightClickDown(bool _ShiftDown, bool _CtrlDown)
    {
        m_RightIsDown = true;
    }

    private void RightClickUp(bool _ShiftDown, bool _CtrlDown)
    {
        m_RightIsDown = false;

        if (m_RightDownTime < 0.2f)
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);

            UnitSelectionHandler.Instance.OnGiveMoveOrder(_ShiftDown, hit.point);
        }

        m_RightDownTime = 0;
    }

    //private void OnDrawGizmos()
    //{
    //    var oldMatrix = Gizmos.matrix;
        
    //    Gizmos.matrix = Matrix4x4.TRS(debugcenter, Quaternion.identity, debugsize * 2);
    //    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

    //    Gizmos.matrix = oldMatrix;
    //}
}