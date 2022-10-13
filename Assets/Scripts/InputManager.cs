using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SolidSky 
{
    public class InputManager : MonoBehaviour
    {
        //Input Actions Component
        public InputActions_FluidCam inputActionsFluidCam;

        //Currently Active Device
        public enum CurrentDevice { KeyboardMouse, Gamepad };
        public CurrentDevice currentDevice;

        //Devices
        private Keyboard keyboard;
        private Mouse mouse;
        private Gamepad gamepad;

        //Player Controller Variables
        public Vector2 playerMove;
        public Vector2 cameraMove;
        public Vector2 cameraProximity;
        public float boost;

        private void Awake()
        {
            inputActionsFluidCam = new InputActions_FluidCam();

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
            inputActionsFluidCam.PlayerController.Enable();

            //Watch for player movement input and apply functions if performed or canceled.
            inputActionsFluidCam.PlayerController.Movement.performed += MovementPerformed;
            inputActionsFluidCam.PlayerController.Movement.canceled += MovementCanceled;

            //Watch for camera movement input and apply functions if performed or canceled.
            inputActionsFluidCam.PlayerController.Camera.performed += CameraPerformed;
            inputActionsFluidCam.PlayerController.Camera.canceled += CameraCanceled;

            inputActionsFluidCam.PlayerController.CameraProximityChange.started += CameraProximityChangeStarted;

            //Watch for boost input and apply functions if performed.
            inputActionsFluidCam.PlayerController.Boost.performed += Boost;
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
            playerMove = context.ReadValue<Vector2>();
        }

        /// <summary>
        ///     Zeros out values if no input is detected.
        /// </summary>
        /// <remarks>
        ///     Zeroing is necessary because the last value stored before being canceled is 
        ///     never 0 which leaves a small remainder causing drifting even when no input 
        ///     is detected.
        /// </remarks>
        /// <param name="context"></param>
        private void MovementCanceled(InputAction.CallbackContext context)
        {
            playerMove = Vector2.zero;
        }

        /// <summary>
        ///     Collects camera movement values if any are performed.
        /// </summary>
        /// <param name="context"></param>
        private void CameraPerformed(InputAction.CallbackContext context)
        {
            cameraMove = context.ReadValue<Vector2>();
        }

        /// <summary>
        ///     Zeros out values if no input is detected.
        /// </summary>
        /// <remarks>
        ///     Zeroing is necessary because the last value stored before being canceled is 
        ///     never 0 which leaves a small remainder causing drifting even when no input 
        ///     is detected.
        /// </remarks>
        /// <param name="context"></param>
        private void CameraCanceled(InputAction.CallbackContext context)
        {
            cameraMove = Vector2.zero;
        }

        /// <summary>
        ///     Sends event message to CameraController to set the camera proximity to a new value.
        /// </summary>
        /// <param name="context"></param>
        private void CameraProximityChangeStarted(InputAction.CallbackContext context)
        {
            SendMessage("SetCamProximity", context.ReadValue<Vector2>());
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

