using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class PlayerController : MonoBehaviour, IPlayerActions
{
    public Vector2 mousePosition;

    private static PlayerController _instance;
    public static PlayerController Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }else
        {
            Destroy(gameObject);
        }
    }


#region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePosition = context.ReadValue<Vector2>();
        }
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if(context.performed && BuildingManager.Instance.isBuildingMode)
        {
            BuildingManager.Instance.Build();
        }
    }

    public void OnCancelBuild(InputAction.CallbackContext context)
    {
        if (context.performed && BuildingManager.Instance.isBuildingMode)
        {
            Debug.Log("Cancelado");
            BuildingManager.Instance.CancelBuild();
        }
    }

    public void OnRotateBuilding(InputAction.CallbackContext contex)
    {
        if (contex.performed && BuildingManager.Instance.isBuildingMode)
        {
            BuildingManager.Instance.RotateBuilding();
        }
    }
#endregion
}