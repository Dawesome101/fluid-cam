
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Enable this setting to invert the cameras vertical rotation control.")]
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

        private float camOrbitAxisVerticalAngle;
        [Header("Camera Orbit Axis Settings")]
        public float camAxisRotDamping_C = 150f;
        public float camAxisRotDamping_M = 150f;
        
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

        [Header("Sensor Settings")]
        [Tooltip("Initially this mask is only set to the default layer, however the " +
            "cameras environment sensor will collide with any layer added to this mask " +
            "making the camera adjust its position accordingly. Make sure this mask " +
            "does not include the layer of the object it orbits (i.e. the player layer, " +
            "etc.) or it may collide with it and cause unwanted behaviour.")]
        public LayerMask sensorHitLayerMask = 1 << 0;
        public enum SensorType { Sphere, Cube }
        public SensorType sensorType;

        private Ray sensorRay;
        private RaycastHit sensorHit;
        private bool sensorDidHit;
        public float camSensorCastExtents = 0.25f;
        public float camSensorProbeDistance = 5f;
        [Tooltip("This is the cameras vertical position offset from the end point of the " +
            "sensor ray. This keeps the camera from clipping into geometry and may require " +
            "adjustment for the projects specific needs.")]
        public float camHeightOffset = 1f;

        [Header("Debug Settings")]
        public bool enableDebug = false;

        

        public void OnValidate()
        {
            //Check wether the upper and lower vertical orbit axis are above or below their thresholds.
            //Additionally check that neither are closer than their tolerance with one another. Adjust
            //the clamps if they are outside of their bounds.

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
            lookMode = LookMode.Free;
        }

        private void FixedUpdate()
        {
            GetCamPosTarget(sensorType);

            CheckForInverted(enableDebug);

            SetCamAxisPosRot();

            SetCamPosRot();
        }

        /// <summary>
        /// Extends a sensor out from behind the orbit axis and stores the calculated 
        /// camera target position based on what it finds as well as stores the sensor 
        /// hit information which is intend for use with debug.
        /// </summary>
        private void GetCamPosTarget(SensorType senseType)
        {
            // Construct the sensor ray.
            sensorRay = new Ray(camOrbitAxis.position, -camOrbitAxis.forward);

            //Cast the sensor based on its type using the ray to detect the current environment.
            if (senseType == SensorType.Sphere)
            {
                sensorDidHit = Physics.SphereCast(sensorRay, camSensorCastExtents, out sensorHit, camSensorProbeDistance, sensorHitLayerMask);
            }
            else if (senseType == SensorType.Cube)
            { 
            
            }
            
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
        /// Sets inverted camera control flag to on or off. Intended for use with
        /// controller debug enabling quick change of the inverted setting. Currently,
        /// if debug is turned on, depressing the R stick on a controller or pressing
        /// I on the keyboard toggles this setting on and off.
        /// </summary>
        private void CheckForInverted(bool debugIsEnabled)
        {
            if (debugIsEnabled)
            {
                if (!invertCamera && inputManager.invertedOn)
                {
                    invertCamera = true;
                }
                else if (invertCamera && !inputManager.invertedOn)
                {
                    invertCamera = false;
                }
            }
        }

        /// <summary>
        /// Calculates and sets the orbit axis rotation and position.
        /// </summary>
        private void SetCamAxisPosRot()
        {
            //Check wether the orbit axis has moved relative to the target orbit axis and set a new position if it has.
            if (camOrbitAxis.position != camOrbitAxisTarget.position)
            {
                camOrbitAxis.position = camOrbitAxisTarget.position;
            }

            //Check for user input from the InputManager and if any is detected get and set the camera
            //axis rotation based on the change.
            if (lookMode == LookMode.ThirdPerson)
            {
                //Check to see if user input is detected.
                if (inputManager.cameraX != 0f || inputManager.cameraY != 0f)
                {
                    float camAxisRotDamping;

                    //Check which device is sending input values.
                    if (inputManager.currentDevice == InputManager.CurrentDevice.KeyboardMouse)
                    {
                        camAxisRotDamping = camAxisRotDamping_M;
                    }
                    else { camAxisRotDamping = camAxisRotDamping_C; }

                    //Rotate the orbit axis based on user input and zero out the z axis to prevent it from rolling.
                    camOrbitAxis.Rotate(GetCamOrbitAxisTargetRot(inputManager.cameraX, -inputManager.cameraY, camAxisRotDamping, invertCamera, true));
                    camOrbitAxis.eulerAngles = new Vector3(camOrbitAxis.eulerAngles.x, camOrbitAxis.eulerAngles.y, 0f);
                }
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
        /// Sets a new position and rotation for the camera if needed else does nothing.
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
        /// Calculates a dampened Vector3 and inverts the vertical value and/or applies deltaTime if specified (in that order).
        /// </summary>
        /// <param name="horz"></param>
        /// <param name="vert"></param>
        /// <param name="damp"></param>
        /// <param name="inverted"></param>
        /// <param name="useDeltaTime"></param>
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
            //Check if there has been a hit yet
            if (sensorDidHit)
            {
                if (Gizmos.color != Color.red) { Gizmos.color = Color.red; }
                //Draw a Ray forward from GameObject toward the hit
                Gizmos.DrawRay(camOrbitAxis.position, -camOrbitAxis.forward * sensorHit.distance);
                //Draw a cube that extends to where the hit exists
                //Gizmos.DrawWireCube(transform.position + transform.forward * m_Hit.distance, transform.localScale);
                Gizmos.DrawWireSphere(sensorHit.point, camSensorCastExtents);
            }
            //If there hasn't been a hit yet, draw the ray at the maximum distance
            else
            {
                if (Gizmos.color != Color.green) { Gizmos.color = Color.green; }
                //Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.DrawRay(camOrbitAxis.position, -camOrbitAxis.forward * camSensorProbeDistance);
                //Draw a cube at the maximum distance
                //Gizmos.DrawWireCube(transform.position + transform.forward * m_MaxDistance, transform.localScale);
                Gizmos.DrawWireSphere(camOrbitAxis.position + -camOrbitAxis.forward * camSensorProbeDistance, camSensorCastExtents);
            }
        }
    }
}

