using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance;

    private InputAction m_MoveAction;
    private InputAction m_ScrollAction;
    private InputAction m_LeftClickAction;
    private InputAction m_RightClickAction;
    private InputAction m_ShiftAction;
    private InputAction m_CtrlAction;
    private InputAction m_SpaceAction;

    private EventManager m_EventManager;

    private bool m_ShiftDown = false;
    private bool m_CtrlDown = false;
    private bool m_SpaceDown = false;

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
        InitializeControls();
    }

    private void InitializeControls()
    {
        m_MoveAction = new InputAction("Move", InputActionType.Value);
        m_MoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/W")
            .With("Down", "<Keyboard>/S")
            .With("Left", "<Keyboard>/A")
            .With("Right", "<Keyboard>/D");
        m_MoveAction.performed += OnMoveAction;
        m_MoveAction.canceled += OnMoveAction;
        m_MoveAction.Enable();

        m_ScrollAction = new InputAction("ScrollWheel", InputActionType.Value, "<Mouse>/scroll");
        m_ScrollAction.performed += OnScrollAction;
        m_ScrollAction.Enable();

        m_LeftClickAction = new InputAction("LeftClick", InputActionType.Button, "<Mouse>/leftButton");
        m_LeftClickAction.started += OnLeftClickAction;
        m_LeftClickAction.canceled += OnLeftClickAction;
        m_LeftClickAction.Enable();

        m_RightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
        m_RightClickAction.started += OnRightClickAction;
        m_RightClickAction.canceled += OnRightClickAction;
        m_RightClickAction.Enable();

        m_ShiftAction = new InputAction("Shift", InputActionType.Button);
        m_ShiftAction.AddBinding("<Keyboard>/leftShift");
        m_ShiftAction.AddBinding("<Keyboard>/rightShift");
        m_ShiftAction.started += OnShiftAction;
        m_ShiftAction.canceled += OnShiftAction;
        m_ShiftAction.Enable();

        m_CtrlAction = new InputAction("Ctrl", InputActionType.Button);
        m_CtrlAction.AddBinding("<Keyboard>/leftCtrl");
        m_CtrlAction.AddBinding("<Keyboard>/rightCtrl");
        m_CtrlAction.started += OnCtrlAction;
        m_CtrlAction.canceled += OnCtrlAction;
        m_CtrlAction.Enable();

        m_SpaceAction = new InputAction("Space", InputActionType.Button);
        m_SpaceAction.AddBinding("<Keyboard>/space");
        m_SpaceAction.AddBinding("<Keyboard>/space");
        m_SpaceAction.started += OnSpaceAction;
        m_SpaceAction.canceled += OnSpaceAction;
        m_SpaceAction.Enable();
    }

    private void Start()
    {
        if (EventManager.Instance)
        {
            m_EventManager = EventManager.Instance;
        }
        else
        {
            Debug.LogError("No EventManager in Scene!");
        }
    }

    private void OnMoveAction(InputAction.CallbackContext _context)
    {
        Vector2 value = _context.ReadValue<Vector2>();

        m_EventManager.PlayerMove?.Invoke(value);
    }

    private void OnScrollAction(InputAction.CallbackContext _context)
    {
        Vector2 value = _context.ReadValue<Vector2>();
        float scrollVal = value.y / 120;

        m_EventManager.PlayerScroll?.Invoke(scrollVal);
    }

    private void OnLeftClickAction(InputAction.CallbackContext _context)
    {
        switch (_context.phase)
        {
            case InputActionPhase.Disabled:
                break;

            case InputActionPhase.Waiting:
                break;

            case InputActionPhase.Started:
                m_EventManager.PlayerLeftMouseDown?.Invoke(m_ShiftDown,m_CtrlDown,m_SpaceDown);
                break;

            case InputActionPhase.Performed:
                break;

            case InputActionPhase.Canceled:
                m_EventManager.PlayerLeftMouseUp?.Invoke(m_ShiftDown, m_CtrlDown, m_SpaceDown);
                break;

            default:
                break;
        }
    }

    private void OnRightClickAction(InputAction.CallbackContext _context)
    {
        switch (_context.phase)
        {
            case InputActionPhase.Disabled:
                break;

            case InputActionPhase.Waiting:
                break;

            case InputActionPhase.Started:
                m_EventManager.PlayerRightMouseDown?.Invoke(m_ShiftDown, m_CtrlDown, m_SpaceDown);
                break;

            case InputActionPhase.Performed:
                break;

            case InputActionPhase.Canceled:
                m_EventManager.PlayerRightMouseUp?.Invoke(m_ShiftDown, m_CtrlDown, m_SpaceDown);
                break;

            default:
                break;
        }
    }

    private void OnShiftAction(InputAction.CallbackContext _context)
    {
        switch (_context.phase)
        {
            case InputActionPhase.Disabled:
                break;

            case InputActionPhase.Waiting:
                break;

            case InputActionPhase.Started:
                m_ShiftDown = true;
                break;

            case InputActionPhase.Performed:
                break;

            case InputActionPhase.Canceled:
                m_ShiftDown = false;
                break;

            default:
                break;
        }
    }

    private void OnCtrlAction(InputAction.CallbackContext _context)
    {
        switch (_context.phase)
        {
            case InputActionPhase.Disabled:
                break;

            case InputActionPhase.Waiting:
                break;

            case InputActionPhase.Started:
                m_CtrlDown = true;
                break;

            case InputActionPhase.Performed:
                break;

            case InputActionPhase.Canceled:
                m_CtrlDown = false;
                break;

            default:
                break;
        }
    }

    private void OnSpaceAction(InputAction.CallbackContext _context) 
    {
        switch (_context.phase)
        {
            case InputActionPhase.Disabled:
                break;

            case InputActionPhase.Waiting:
                break;

            case InputActionPhase.Started:
                m_SpaceDown = true;
                break;

            case InputActionPhase.Performed:
                break;

            case InputActionPhase.Canceled:
                m_SpaceDown = false;
                break;

            default:
                break;
        }
    }
}