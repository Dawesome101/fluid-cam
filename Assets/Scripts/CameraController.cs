
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Enable this setting to invert the cameras vertical control.")]
        public bool invertCamera;

        // The input manager component is provided with the asset and is ready to use
        // right out of the gate. If one is not already present on the camera using
        // the CameraController component, add one now.
        private InputManager inputManager;

        [Header("Camera Objects")]
        // camOrbitAxis requires an empty GameObject which has the tag CamOrbitAxis to be
        // present in the scene. It is recommended that this GameObject is a child of the
        // camera which is using the CameraController component in order to keep things
        // organzied. However the only requirement is that one be present in the scene.
        // camOrbitAxis is used to project the sphereCast environment sensor and provide
        // camera position information based on what the sensor hits.
        public Transform camOrbitAxis;

        // camOrbitAxisTargetPos requires an empty GameObject which has the tag
        // CamOrbitAxisTarget to be present in the scene. It is recommended that
        // this GameObject is a child of the camera which is using the CameraController
        // component however the only requirement is that one be present in the scene.
        // camOrbitAxisTarget is used to provide angle information and position
        // information to the camOrbitAxis. 
        public Transform camOrbitAxisTarget;

        [Tooltip("This is the object the camera will attempt to focus on. It needs " +
            "to be manually set by dragging the object intended as the cameras " +
            "focal point from the hierarchy into this field. This would typically be " +
            "set to something like the players head or cockpit of a car. However it " +
            "can be assigned to any transform so long as it is the object intended for " +
            "the camera to focus on.")]
        public Transform camFocusSubject;

        [Header("Camera Orbit Axis Dampening")]
        [Tooltip("Orbit axis rotation dampening for a controller. The default value is 150.")]
        public float camAxisRotDamping_C = 200f;
        [Tooltip("Orbit axis rotation dampening for a mouse. The default value is 150.")]
        public float camAxisRotDamping_M = 1200f;
        [Tooltip("In Unity, mouse input is only one of three values, -1, 0 or 1. Additionally " +
            "Unity only detects movement if an entire pixel of movement is detected. This means " +
            "that sub-pixel movement is not hard coded in. Raw Mouse Input Damping is intended " +
            "to smooth out the jitter from this effect. In short, this value helps smooth out " +
            "mouse movements. It is highly recommended to leave this set to 0.05 (the default " +
            "value) however it can be adjusted for the projects specific needs. This value is " +
            "clamped between 0 and 1.")]
        [Range(0.0f, 1.0f)]
        public float rawMouseInputDamping = 0.05f;

        [Header("Camera Orbit Axis Angle Clamps")]
        [Tooltip("The tolerance in degrees with witch Cam Axis Lower and Upper Angle Clamps " +
            "can be to one another. For example if this number is set to 5 then the camera " +
            "clamps will never be closer than 5 degrees apart. This number can not be lower " +
            "than 0 or higher than 10.")]
        public float camAxisAngleClampTolerance = 10f;
        [Tooltip("The degree angle the camera can move vertically downward before stoping. " +
            "This number can not be below -70 or above 80. This number will observe " +
            "Cam Axis Angle Clamp Tolerance as the difference between itself and " +
            "Cam Axis Upper Angle Clamp. Meaning this number will never be closer to " +
            "Cam Axis Upper Angle Clamp than the tolerance.")]
        public float camAxisLowerAngleClamp = 80f;
        [Tooltip("The degree angle the camera can move vertically upward before stoping. " +
            "This number can not be below -70 or above 80. This number will observe " +
            "Cam Axis Angle Clamp Tolerance as the difference between itself and " +
            "Cam Axis Lower Angle Clamp. Meaning this number will never be closer to " +
            "Cam Axis Lower Angle Clamp than the tolerance.")]
        public float camAxisUpperAngleClamp = 80f;
        private float camOrbitAxisVerticalAngle;

        // This is the desired position for the camera and is used as the target
        // destination for position lerping.
        private Vector3 camTargetPosition;
        // This is the desired rotation for the camera and is used as the target
        // destination for rotation lerping.
        private Quaternion camTargetRotation;

        [Header("Camera Motion Settings")]
        [Tooltip("This setting is for the camera itself, not the orbit axis. This is the " +
            "position dampening used to speed up or slow down the cameras ability to " +
            "relocate to its desired position. The default value for this is 8. This can " +
            "be tweaked until getting the desired result. A lower number will cause it to " +
            "move slower while a higher number will make it move faster. If you find the " +
            "camera is trailing to far behind the target, try raising this number to help " +
            "it catch up.")]
        public float camPosDamping = 8f;
        [Tooltip("This setting is for the camera itself, not the orbit axis. This is the " +
            "rotation dampening used to speed up or slow down the cameras ability to rotate " +
            "in order to focus on the target. The default value for this number is 8 however " +
            "raising it will speed up the cameras rotation and lowering it will slow it down. " +
            "It is safe to tweak this value until getting the desired result.")]
        public float camRotDamping = 8f;

        [Header("Camera Proximity Settings")]
        [Tooltip("How far the camera will move when the player changes the proximity to " +
            "its subject. The default value is 1 and can not go below zero.")]
        public float camProximityStepCount = 1f;

        [Header("Sensor Settings")]
        [Tooltip("Initially this mask is only set to the default layer, however the " +
            "cameras environment sensor will collide with any layer added to this mask " +
            "making the camera adjust its position accordingly. Make sure this mask " +
            "does not include the layer of the object it orbits (i.e. the player layer, " +
            "etc.) or it may collide with it and cause unwanted behaviour.")]
        public LayerMask sensorHitLayerMask = 1 << 0;
        //This must be globally scoped because it is used in multiple functions.
        private RaycastHit sensorHit;
        private bool sensorDidHit;
        [Tooltip("The size of the probe used to check for the environment. It is important " +
            "to note this value is the radius. If for example the value here is 0.5, the " +
            "diameter of the probe will be 1. Because of this, always use a number exactly " +
            "half the intended size.")]
        public float camSensorCastRadius = 0.5f;
        [Tooltip("The distance the probe will check for collisions with the environment.")]
        public float camSensorProbeDistance = 10f;
        [Tooltip("The minimum distance the probe can be to the cameras subject. This is " +
            "mostly useful when setting the cameras proximity at runtime to keep it from " +
            "going closer than intended.")]
        public float camSensorProbeDistanceMin = 1f;
        [Tooltip("The maximum distance the probe can be to the cameras subject. This is " +
            "mostly useful when setting the cameras proximity at runtime to keep it from " +
            "going farther than intended.")]
        public float camSensorProbeDistanceMax = 10f;
        [Tooltip("This is the cameras vertical position offset from the end point of the " +
            "sensor ray. This keeps the camera from clipping into geometry and may require " +
            "adjustment for the projects specific needs.")]
        public float camHeightOffset = 1f;

        [Header("Debug Settings")]
        public bool enableDebug = false;

        

        public void OnValidate()
        {

            //MAKE FUNCTION CALL CHECKANGLECLAMPS


            //Check if the upper and lower vertical orbit axis are above or below their thresholds.
            //If they are closer than their threshold adjust the values back into tolerance.

            //Note: It is okay to have the clampsMax and Min hard coded to 80f and -70f as they will
            //always be the same number regardless of the camera systems use.
            camAxisLowerAngleClamp = Mathf.Clamp(camAxisLowerAngleClamp, -70f, 80f);

            camAxisAngleClampTolerance = Mathf.Clamp(camAxisAngleClampTolerance, 0f, 10f);

            if ((camAxisUpperAngleClamp + camAxisLowerAngleClamp) < camAxisAngleClampTolerance && camAxisLowerAngleClamp > -70f)
            {
                camAxisUpperAngleClamp += camAxisAngleClampTolerance - (camAxisUpperAngleClamp + camAxisLowerAngleClamp);
            }
            else if (camAxisLowerAngleClamp == -70f && camAxisUpperAngleClamp < 70f + camAxisAngleClampTolerance)
            { 
                camAxisUpperAngleClamp = 70f + camAxisAngleClampTolerance;
            }

            camAxisUpperAngleClamp = Mathf.Clamp(camAxisUpperAngleClamp, -70f, 80f);



            //Camera sensor probe 
            camProximityStepCount = Mathf.Clamp(camProximityStepCount, 0f, Mathf.Infinity);

            camSensorProbeDistance = Mathf.Clamp(camSensorProbeDistance, 0f, Mathf.Infinity);
            camSensorProbeDistanceMin = Mathf.Clamp(camSensorProbeDistanceMin, 0f, Mathf.Infinity);
            camSensorProbeDistanceMax = Mathf.Clamp(camSensorProbeDistanceMax, 0f, Mathf.Infinity);

            if (camSensorProbeDistanceMin > camSensorProbeDistanceMax)
            { 
                camSensorProbeDistanceMin = camSensorProbeDistanceMax;
            }
        }
        private void Awake()
        {
            if (!FindObjectOfType<InputManager>())
            {
                Debug.LogError("CameraController needs the InputManager component in order " +
                    "to function. Add one to the same camera using the CameraController component.");
            } 
            else inputManager = FindObjectOfType<InputManager>();

            if (!GameObject.FindGameObjectWithTag("CamOrbitAxis"))
            {
                Debug.LogError("The camera orbit axis object is missing from the scene. Create an " +
                    "empty GameObject with the tag 'CamOrbitAxis' then add it as a child of " +
                    "the camera using the CameraController component.");
            } 
            else camOrbitAxis = GameObject.FindGameObjectWithTag("CamOrbitAxis").transform;

            if (!GameObject.FindGameObjectWithTag("CamOrbitAxisTarget"))
            {
                Debug.LogError("The camera orbit axis target object is missing from the scene. " +
                    "Create an empty GameObject with the tag 'CamOrbitAxisTarget' then add it " +
                    "as a child of the camera using the CameraController component.");
            } 
            else camOrbitAxisTarget = GameObject.FindGameObjectWithTag("CamOrbitAxisTarget").transform;

            if (!camFocusSubject)
            {
                Debug.LogError("The camera will attempt to focus on the transform assigned to " +
                    "camFocusSubject which has not yet been set in the inspector. The transform " +
                    "intended as the cameras subject must manually be assigned to Cam Focus Subject " +
                    "in the inspector. Typically this would be set to a players head or a vehicles cockpit.");
            }
            else
            {
                camOrbitAxisTarget.parent = camFocusSubject.transform;
                camOrbitAxisTarget.localPosition = Vector3.zero;
                camOrbitAxisTarget.localRotation = Quaternion.identity;
            }

            transform.parent = null;
            camOrbitAxis.transform.parent = null;
        }

        private void Update()
        {
            GetCamProximity(inputManager.cameraProximity.y);
        }
        private void FixedUpdate()
        {
            GetCamPosTarget();

            SetCamAxisPosRot(inputManager.cameraMove, inputManager.currentDevice);

            SetCamPosRot();
        }

        private void GetCamProximity(float camProximityInput)
        {
            if (camProximityInput < 0)
            {
                camSensorProbeDistance -= camProximityStepCount;
            }
            else if (camProximityInput > 0)
            {
                camSensorProbeDistance += camProximityStepCount;
            }

            if (camSensorProbeDistance < camSensorProbeDistanceMin)
            {
                camSensorProbeDistance = camSensorProbeDistanceMin;
            }
            else if (camSensorProbeDistance > camSensorProbeDistanceMax)
            {
                camSensorProbeDistance = camSensorProbeDistanceMax;
            }
        }

        /// <summary>
        ///     Extends a sensor out from behind the orbit axis and stores the calculated 
        ///     camera target position based on what it finds as well as stores the sensor 
        ///     hit information which is intend for use with debug.
        /// </summary>
        private void GetCamPosTarget()
        {
            //Cast the sensor based on its type using the ray to detect the current environment.
            Ray sensorRay = new Ray(camOrbitAxis.position, -camOrbitAxis.forward);
            sensorDidHit = Physics.SphereCast(sensorRay, camSensorCastRadius, out sensorHit, camSensorProbeDistance, sensorHitLayerMask);

            //Store position information based on if the sensor has detected a collision or not.
            if (sensorDidHit)
            {
                camTargetPosition = new Vector3(sensorHit.point.x, sensorHit.point.y + camHeightOffset, sensorHit.point.z);
            }
            else
            {
                camTargetPosition = camOrbitAxis.TransformPoint(new Vector3(0f, camHeightOffset, -camSensorProbeDistance));
            }
        }

        /// <summary>
        ///     Calculates and sets the orbit axis rotation and position.
        /// </summary>
        /// <param name="cameraMove"></param>
        /// <param name="currentDevice"></param>
        private void SetCamAxisPosRot(Vector2 cameraMove, InputManager.CurrentDevice currentDevice)
        {
            //Check wether the orbit axis has moved relative to the target orbit axis and set a new position if it has.
            if (camOrbitAxis.position != camOrbitAxisTarget.position)
            {
                camOrbitAxis.position = camOrbitAxisTarget.position;
            }

            //Check for user input from the InputManager and if any is detected get and set the camera
            //axis rotation based on the change.
            if (cameraMove.x != 0f || cameraMove.y != 0f)
            {
                //Set up our default value.
                float camAxisRotDamping;
                
                //Check which device is sending input values and assigning the value to the global axis rotation dampening.
                if (currentDevice == InputManager.CurrentDevice.KeyboardMouse)
                {
                    //Dampen the raw mouse input to smooth out jitter from micro mouse movements.
                    cameraMove *= rawMouseInputDamping;

                    //Assigning the mouse damping to the orbit damping. This value is not related to rawMouseInputDamping.
                    camAxisRotDamping = camAxisRotDamping_M;
                }
                else { camAxisRotDamping = camAxisRotDamping_C; }

                //Rotate the orbit axis based on user input and zero out the z axis to prevent it from rolling.
                camOrbitAxis.Rotate(GetCamOrbitAxisTargetRot(cameraMove.x, -cameraMove.y, camAxisRotDamping, invertCamera, true));
                camOrbitAxis.eulerAngles = new Vector3(camOrbitAxis.eulerAngles.x, camOrbitAxis.eulerAngles.y, 0f);
            }
            
            //Get the signed angle of the orbit axis to make clamping between -180 and 180 simple.
            camOrbitAxisVerticalAngle = Vector3.SignedAngle(camOrbitAxis.up, Vector3.up, camOrbitAxis.right);

            //Clamp the orbit axis so it never goes beyond the specified angles. This also helps avoid gimble lock.
            if (camOrbitAxisVerticalAngle > camAxisLowerAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(-camAxisLowerAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
            }
            else if (camOrbitAxisVerticalAngle < -camAxisUpperAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(camAxisUpperAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
            }
        }

        /// <summary>
        ///     Sets a new position and rotation for the camera if needed else does nothing.
        /// </summary>
        private void SetCamPosRot()
        {
            //Get the direction from the camera to the orbit axis
            camTargetRotation = Quaternion.LookRotation(camOrbitAxis.position - transform.position, camOrbitAxis.up);
            
            //Check to see if the camera should change position and/or rotation based on calculated
            //values then set them if they have.
            if (transform.position != camTargetPosition)
            {
                transform.position = Vector3.Lerp(transform.position, camTargetPosition, camPosDamping * Time.deltaTime);
            }

            if (transform.rotation != camTargetRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, camTargetRotation, camRotDamping * Time.deltaTime);
            }
        }

        /// <summary>
        ///     Calculates a dampened Vector3 and inverts the vertical value and/or applies deltaTime if specified (in that order).
        /// </summary>
        /// <param name="horz"></param>
        /// <param name="vert"></param>
        /// <param name="damp"></param>
        /// <param name="inverted"></param>
        /// <param name="useDeltaTime"></param>
        /// <returns>One of 3 outputs, a new Vector 3 (vert, horz, 0f) * damp, the same but inverted or the same but *= deltaT</returns>
        private Vector3 GetCamOrbitAxisTargetRot(float horz, float vert, float damp, bool inverted, bool useDeltaTime)
        {
            Vector3 newVec = Vector3.zero;

            if (!inverted)
            {
                newVec = new Vector3(vert, horz, 0f) * damp;
            }
            else if (inverted)
            {
                newVec = new Vector3(-vert, horz, 0f) * damp;
            }

            if (useDeltaTime)
            {
                newVec *= Time.deltaTime;
            }

            return newVec;
        }

        void OnDrawGizmos()
        {
            if (camOrbitAxis != null && enableDebug)
            {
                if (sensorDidHit)
                {
                    if (Gizmos.color != Color.red) { Gizmos.color = Color.red; }

                    Gizmos.DrawRay(camOrbitAxis.position, -camOrbitAxis.forward * sensorHit.distance);
                    Gizmos.DrawWireSphere(sensorHit.point, camSensorCastRadius);
                }
                else
                {
                    if (Gizmos.color != Color.green) { Gizmos.color = Color.green; }

                    Gizmos.DrawRay(camOrbitAxis.position, -camOrbitAxis.forward * camSensorProbeDistance);
                    Gizmos.DrawWireSphere(camOrbitAxis.position + -camOrbitAxis.forward * camSensorProbeDistance, camSensorCastRadius);
                }
            }
        }
    }
}

