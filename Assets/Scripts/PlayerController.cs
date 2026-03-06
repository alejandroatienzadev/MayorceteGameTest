using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class PlayerController : MonoBehaviour, IPlayerActions
{
#region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
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
        
    }
#endregion
}