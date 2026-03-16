using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class PlayerController : MonoBehaviour, IPlayerActions
{
    public Vector2 mousePosition;

    public bool _buildInputReceived;

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

    void Update()
    {
        if (_buildInputReceived)
        {
            _buildInputReceived = false;
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (BuildingManager.Instance.IsBuildingMode)
                {
                    BuildingManager.Instance.Build();    
                }
                else
                {
                    BuildingManager.Instance.SelectBuilding();
                }
            }
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
        if(context.performed)
        {
            _buildInputReceived = true;
        }
    }

    public void OnCancelBuild(InputAction.CallbackContext context)
    {
        if (context.performed && BuildingManager.Instance.IsBuildingMode)
        {
            Debug.Log("Cancelado");
            BuildingManager.Instance.CancelBuild();
        }
    }

    public void OnRotateBuilding(InputAction.CallbackContext contex)
    {
        if (contex.performed && BuildingManager.Instance.IsBuildingMode)
        {
            BuildingManager.Instance.RotateBuilding();
        }
    }
#endregion
}