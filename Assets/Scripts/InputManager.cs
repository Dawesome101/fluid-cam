using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SolidSky 
{
    public class InputManager : MonoBehaviour
    {
        //Input Actions Component
        public InputActions_SolidSky inputActions_SolidSky;

        //Currently Active Device
        public enum CurrentDevice { KeyboardMouse, Gamepad };
        public CurrentDevice currentDevice;

        //Devices
        private Keyboard keyboard;
        private Mouse mouse;
        private Gamepad gamepad;

        //Player Controller Variables
        public float cameraX;
        public float cameraY;
        public float moveX;
        public float moveZ;
        public float boost;

        private void Awake()
        {
            inputActions_SolidSky = new InputActions_SolidSky();

            //Check for Keyboard and Mouse and assign them or throw an error if either are not detected.
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

            //Check for a Gamepad and assign it if one is found.
            if (InputSystem.GetDevice<Gamepad>() != null) 
            { 
                gamepad = InputSystem.GetDevice<Gamepad>();

                Debug.Log("Gamepad initialized");
            }

            //Enable Player Controller to send device actions
            inputActions_SolidSky.PlayerController.Enable();

            //Watch for player movement input and apply functions if performed or canceled.
            inputActions_SolidSky.PlayerController.Movement.performed += MovementPerformed;
            inputActions_SolidSky.PlayerController.Movement.canceled += MovementCanceled;

            //Watch for camera movement input and apply functions if performed or canceled.
            inputActions_SolidSky.PlayerController.Camera.performed += CameraPerformed;
            inputActions_SolidSky.PlayerController.Camera.canceled += CameraCanceled;

            //Watch for boost input and apply functions if performed or canceled.
            inputActions_SolidSky.PlayerController.Boost.performed += Boost;
        }

        private void Update()
        {
            WatchDeviceConnectivity();

            if (gamepad != null)
            {
                WatchForCurrentDevice();
            }
        }

        /// <summary>
        ///     Watches for device changes. If one is detected and is a compatable type 
        ///     it will be assigned to the appropriate device variable.
        /// </summary>
        /// <remarks>
        ///     Compatable Devices: Xbox Controller
        /// </remarks>
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

        /// <summary>
        ///     Watches for which device was the last to update and assigns it as the current device in use.
        /// </summary>
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

        /// <summary>
        ///     Collects movement values if any are performed.
        /// </summary>
        /// <param name="context"></param>
        private void MovementPerformed(InputAction.CallbackContext context)
        {
            moveX = context.ReadValue<Vector2>().x;
            moveZ = context.ReadValue<Vector2>().y;
        }

        /// <summary>
        ///     Zeros out movement values if no movement is detected.
        /// </summary>
        /// <remarks>
        ///     Zeroing is necessary because the last value when movement is canceled is 
        ///     never 0 which causes drifting even when no input is detected.
        /// </remarks>
        /// <param name="context"></param>
        private void MovementCanceled(InputAction.CallbackContext context)
        {
            moveX = 0;
            moveZ = 0;
        }

        /// <summary>
        ///     Collects camera movement values if any are performed.
        /// </summary>
        /// <param name="context"></param>
        private void CameraPerformed(InputAction.CallbackContext context)
        {
            cameraX = context.ReadValue<Vector2>().x;
            cameraY = context.ReadValue<Vector2>().y;
        }

        /// <summary>
        ///     Zeros out movement values if no movement is detected.
        /// </summary>
        /// <remarks>
        ///     Zeroing is necessary because the last value when movement is canceled is 
        ///     never 0 which causes drifting even when no input is detected.
        /// </remarks>
        /// <param name="context"></param>
        private void CameraCanceled(InputAction.CallbackContext context)
        {
            cameraX = 0;
            cameraY = 0;
        }

        /// <summary>
        ///     Collects the boost value if it is performed.
        /// </summary>
        /// <param name="context"></param>
        private void Boost(InputAction.CallbackContext context)
        {
            boost = context.ReadValue<float>();
        }
    }
}

