using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SolidSky 
{
    public class HoverController : MonoBehaviour
    {
        public HoverInputActions hoverInputActions;

        public float camRotValueX;
        public float camRotValueY;
        private void Awake()
        {
            hoverInputActions = new HoverInputActions();
            hoverInputActions.PlayerHoverSmall.Enable();
            hoverInputActions.PlayerHoverSmall.Movement.performed += Movement;
            hoverInputActions.PlayerHoverSmall.HCamera.performed += HCamera;
            hoverInputActions.PlayerHoverSmall.Boost.performed += Boost;
        }

        public void Movement(InputAction.CallbackContext context)
        {
            Debug.Log("L" + context.ReadValue<Vector2>());
        }

        public void Boost(InputAction.CallbackContext context)
        {
            Debug.Log(context.ReadValue<float>());
        }

        public void HCamera(InputAction.CallbackContext context)
        {
            Debug.Log("R" + context.ReadValue<Vector2>());
            camRotValueX = context.ReadValue<Vector2>().x;
            camRotValueY = context.ReadValue<Vector2>().y;
        }
    }
}

