using UnityEngine;
using static InputSystem_Actions;

public class InputController : MonoBehaviour
{
    [SerializeField]
    private GameObject playerActionsGO;

    [SerializeField]
    private GameObject uiActionsGO;

    private InputSystem_Actions inputs;
    private IPlayerActions playerActions;
    private IUIActions uiActions;

    private static InputController _instance;
    public static InputController Instance => _instance;

    private void OnValidate()
    {
        if (playerActionsGO != null && !playerActionsGO.TryGetComponent(out playerActions))
        {
            playerActionsGO = null;
        }

        if (uiActionsGO != null && !uiActionsGO.TryGetComponent(out uiActions))
        {
            uiActionsGO = null;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }else
        {
            Destroy(this);
        }

        inputs = new InputSystem_Actions();

        if (playerActions == null && playerActionsGO != null)
        {
            playerActions = playerActionsGO.GetComponent<IPlayerActions>();
        }

        if (uiActions == null && uiActionsGO != null)
        {
            uiActions = uiActionsGO.GetComponent<IUIActions>();
        }
    }

    void Start()
    {
        if (playerActions != null)
        {
            inputs.Player.AddCallbacks(playerActions);
        }

        EnablePlayerInputs(true);

        if (uiActions != null)
        {
            inputs.UI.AddCallbacks(uiActions);
        }
    }

    public void EnablePlayerInputs(bool value)
    {
        if (value)
        {
            inputs.Player.Enable();
        }
        else
        {
            inputs.Player.Disable();
        }
    }

    public void EnableUIInputs(bool value)
    {
        if (value)
        {
            inputs.UI.Enable();
        }
        else
        {
            inputs.UI.Disable();
        }
    }
}
