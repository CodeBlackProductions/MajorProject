using UnityEngine;

public class PlayerController : MonoBehaviour
{
   public static PlayerController Instance;

    private EventManager m_EventManager;

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
    }

    private void Move(Vector2 _MovDir) 
    {
        Debug.Log(_MovDir);
    }

    private void Zoom(float _ZoomVal) 
    {
        Debug.Log(_ZoomVal);
    }

    private void LeftClickDown(bool _ShiftDown, bool _CtrlDown)
    {
        string result = "leftDown";
        if (_ShiftDown)
        {
            result += " + Shift";
        }
        if (_CtrlDown)
        {
            result += " + Ctrl";
        }

        Debug.Log(result);
    }

    private void LeftClickUp(bool _ShiftDown, bool _CtrlDown) 
    {
        string result = "leftUp";
        if (_ShiftDown)
        {
            result += " + Shift";
        }
        if (_CtrlDown)
        {
            result += " + Ctrl";
        }

        Debug.Log(result);
    }

    private void RightClickDown(bool _ShiftDown, bool _CtrlDown)
    {
        string result = "rightDown";
        if (_ShiftDown)
        {
            result += " + Shift";
        }
        if (_CtrlDown)
        {
            result += " + Ctrl";
        }

        Debug.Log(result);
    }

    private void RightClickUp(bool _ShiftDown, bool _CtrlDown)
    {
        string result = "rightUp";
        if (_ShiftDown)
        {
            result += " + Shift";
        }
        if (_CtrlDown)
        {
            result += " + Ctrl";
        }

        Debug.Log(result);
    }
}
