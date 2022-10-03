using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SolidSky 
{
    public class InputManager : MonoBehaviour
    {
        public InputActions_SolidSky inputActions_SolidSky;

        public enum CurrentDevice { KeyboardMouse, Gamepad };
        public CurrentDevice currentDevice;

        private Keyboard keyboard;
        private Mouse mouse;
        private Gamepad gamepad;

        public float cameraX;
        public float cameraY;
        public float moveX;
        public float moveZ;
        public float boost;
        public bool invertedOn;

        private void Awake()
        {
            inputActions_SolidSky = new InputActions_SolidSky();

            if (InputSystem.GetDevice<Keyboard>() == null || InputSystem.GetDevice<Mouse>() == null)
            {
                Debug.LogError("Please connect a keyboard and mouse.");
            } 
            else 
            { 
                keyboard = InputSystem.GetDevice<Keyboard>();
                mouse = InputSystem.GetDevice<Mouse>();

                currentDevice = CurrentDevice.KeyboardMouse;
            }

            if (InputSystem.GetDevice<Gamepad>() != null) 
            { 
                gamepad = InputSystem.GetDevice<Gamepad>();

                Debug.Log("Gamepad initialized");
            }

            

            inputActions_SolidSky.PlayerHoverSmall.Enable();

            inputActions_SolidSky.PlayerHoverSmall.Movement.performed += MovementPerformed;
            inputActions_SolidSky.PlayerHoverSmall.Movement.canceled += MovementCanceled;

            inputActions_SolidSky.PlayerHoverSmall.Camera.performed += CameraPerformed;
            inputActions_SolidSky.PlayerHoverSmall.Camera.canceled += CameraCanceled;

            inputActions_SolidSky.PlayerHoverSmall.Boost.performed += Boost;

            inputActions_SolidSky.PlayerHoverSmall.InvertCamera.started += InvertCamera;
        }

        private void Update()
        {
            WatchDeviceConnectivity();
            WatchForCurrentDevice();
        }

        private void WatchDeviceConnectivity() 
        {
            InputSystem.onDeviceChange +=
                (device, change) =>
                {
                    switch (change)
                    {
                        case InputDeviceChange.Added:

                            if (device.displayName == "Xbox Controller")
                            {
                                gamepad = InputSystem.GetDevice<Gamepad>();
                            }

                            Debug.Log("New device added: " + device.displayName);

                            break;

                        case InputDeviceChange.Removed:

                            if (device.displayName == "Xbox Controller")
                            {
                                gamepad = null;
                            }

                            Debug.Log("Device removed: " + device);
                            break;
                    }
                };
        }

        private void WatchForCurrentDevice() {
            if (keyboard.lastUpdateTime > gamepad.lastUpdateTime || mouse.lastUpdateTime > gamepad.lastUpdateTime && currentDevice == CurrentDevice.Gamepad)
            {
                currentDevice = CurrentDevice.KeyboardMouse;
            }
            else if (gamepad.lastUpdateTime > keyboard.lastUpdateTime && gamepad.lastUpdateTime > mouse.lastUpdateTime && currentDevice == CurrentDevice.KeyboardMouse)
            {
                currentDevice = CurrentDevice.Gamepad;
            }
        }

        private void MovementPerformed(InputAction.CallbackContext context)
        {
            moveX = context.ReadValue<Vector2>().x;
            moveZ = context.ReadValue<Vector2>().y;
        }
        private void MovementCanceled(InputAction.CallbackContext context)
        {
            moveX = 0;
            moveZ = 0;
        }

        private void CameraPerformed(InputAction.CallbackContext context)
        {
            cameraX = context.ReadValue<Vector2>().x;
            cameraY = context.ReadValue<Vector2>().y;
        }
        private void CameraCanceled(InputAction.CallbackContext context)
        {
            cameraX = 0;
            cameraY = 0;
        }

        private void Boost(InputAction.CallbackContext context)
        {
            boost = context.ReadValue<float>();
        }

        private void InvertCamera(InputAction.CallbackContext context) 
        {
            if (invertedOn)
            {
                invertedOn = false;
            }
            else { invertedOn = true; }
        }
    }
}

