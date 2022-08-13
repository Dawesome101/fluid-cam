using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



namespace SolidSky 
{
    public class InputManager : MonoBehaviour
    {
        public InputActions_SolidSky inputActions_SolidSky;

        public Keyboard keyboard;
        public Mouse mouse;
        public Gamepad gamepad;

        public InputDevice[] theDeviceArray;

        public float camRotValueX;
        public float camRotValueY;
        private void Awake()
        {
            inputActions_SolidSky = new InputActions_SolidSky();

            inputActions_SolidSky.PlayerHoverSmall.Enable();
            inputActions_SolidSky.PlayerHoverSmall.Movement.performed += Movement;

            inputActions_SolidSky.PlayerHoverSmall.HCamera.performed += HCameraPerformed;
            inputActions_SolidSky.PlayerHoverSmall.HCamera.canceled += HCameraCanceled;

            inputActions_SolidSky.PlayerHoverSmall.Boost.performed += Boost;


        }

        private void Update()
        {
            
            InputSystem.onDeviceChange +=
                (device, change) =>
                {
                    switch (change)
                    {
                        case InputDeviceChange.Added:

                            if (device.displayName == "Xbox Controller")
                            {
                                Debug.Log(device.name);
                                Debug.Log(device.layout);
                            }
                            Debug.Log("New device added: " + device.displayName);


                            
                            break;

                        case InputDeviceChange.Removed:
                            Debug.Log("Device removed: " + device);
                            break;
                    }
                };
            gamepad = InputSystem.GetDevice<Gamepad>();
            Debug.Log(gamepad.lastUpdateTime);
            //keyboard = InputSystem.GetDevice<Keyboard>();
            //Debug.Log(keyboard.lastUpdateTime);
            //Debug.Log(keyboard.f1Key.isPressed);
        }
        public void Movement(InputAction.CallbackContext context)
        {
            //Debug.Log("L" + context.ReadValue<Vector2>());
        }

        public void Boost(InputAction.CallbackContext context)
        {
            Debug.Log(context.ReadValue<float>());
        }

        public void HCameraPerformed(InputAction.CallbackContext context)
        {
            //Debug.Log("R" + context.ReadValue<Vector2>());
            camRotValueX = context.ReadValue<Vector2>().x;
            camRotValueY = context.ReadValue<Vector2>().y;
        }

        public void HCameraCanceled(InputAction.CallbackContext context) {
            camRotValueX = 0;
            camRotValueY = 0;
        }
    }
}

