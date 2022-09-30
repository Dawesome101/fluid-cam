
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        // The input manager component is provided with the Fancy Fluid Camera asset and
        // is ready to use right out of the gate. If one is not already present on the
        // camera using the CameraController component, add one now.
        private InputManager inputManager;

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

        [Tooltip("Initially this mask is only set to the default layer, however the " +
            "cameras environment sensor will collide with any layer added to this mask " +
            "making the camera adjust its position accordingly. Make sure this mask " +
            "does not include the layer of the object it orbits (i.e. the player layer, " +
            "etc.) or it may collide with it and cause unwanted behaviour.")]
        public LayerMask sensorHitLayerMask = 1 << 0;

        public enum LookMode { Forward, Free }
        public LookMode lookMode;

        [Tooltip("Enable this setting to invert the cameras vertical control.")]
        public bool invertCamera;

        //ADD Controller Damp and Mouse Damp
        public float camAxisRotDamping_C = 150f;
        public float camAxisVerticalAngle;
        
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
        
        
        private Vector3 camTargetPosition;
        private Quaternion camTargetRotation;
        public float camPosDamping = 8f;
        public float camRotDamping = 8f;

        private Ray sensorRay;
        private RaycastHit sensorHit;
        private bool sensorDidHit;
        public float camSensorCastRadius = 0.25f;
        public float camSensorProbeDistance = 5f;
        [Tooltip("This is the cameras vertical position offset from the end point of the " +
            "sensor ray. This keeps the camera from clipping into geometry and may require " +
            "adjustment for the projects specific needs.")]
        public float camHeightOffset = 1f;

        public bool enableDebug = false;

        

        public void OnValidate()
        {
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
            GetCamPosTarget();

            CheckForInverted();

            SetCamAxisPosRot();

            SetCamPosRot();
        }

        private void GetCamPosTarget()
        {
            sensorRay = new Ray(camOrbitAxis.position, -camOrbitAxis.forward);
            sensorDidHit = Physics.SphereCast(sensorRay, camSensorCastRadius, out sensorHit, camSensorProbeDistance, sensorHitLayerMask);
            
            if (sensorDidHit)
            {
                camTargetPosition = new Vector3(sensorHit.point.x, sensorHit.point.y + camHeightOffset, sensorHit.point.z);
            }
            else
            {
                camTargetPosition = camOrbitAxis.TransformPoint(new Vector3(0f, camHeightOffset, -camSensorProbeDistance));
            }
        }

        private void CheckForInverted()
        {
            if (!invertCamera && inputManager.rStickOn)
            {
                invertCamera = true;
            }
            else if (invertCamera && !inputManager.rStickOn)
            {
                invertCamera = false;
            }
        }

        private void SetCamAxisPosRot()
        {
            if (camOrbitAxis.position != camOrbitAxisTarget.position)
            {
                camOrbitAxis.position = camOrbitAxisTarget.position;
            }

            if (lookMode == LookMode.Forward)
            {
                //if (Input.GetAxisRaw("Mouse_Y") != 0f)
                //{
                //    camAxis.Rotate(GetCameraRotation(0f, Input.GetAxis("Mouse_Y"), camAxisRotDamping_M, invertMouse, true));
                //}
                //else 
                
                //if (hoverController.camRotValueY != 0f)
                //{
                //    camAxis.Rotate(GetCameraRotation(0f, -hoverController.camRotValueY, camAxisRotDamping_C, invertGamepad, true));
                //}

                //if (!lookModeTransitionActive)
                //{
                //    camAxis.eulerAngles = new Vector3(camAxis.eulerAngles.x, hoverController.transform.eulerAngles.y, 0f);
                //}
            }
            else if (lookMode == LookMode.Free)
            {
                //if (Input.GetAxis("Mouse_X") != 0f || Input.GetAxisRaw("Mouse_Y") != 0f)
                //{
                //    camAxis.Rotate(GetCameraRotation(Input.GetAxis("Mouse_X"), Input.GetAxis("Mouse_Y"), camAxisRotDamping_M, invertMouse, true));
                //    camAxis.eulerAngles = new Vector3(camAxis.eulerAngles.x, camAxis.eulerAngles.y, 0f);
                //}
                //else 

                if (inputManager.cameraX != 0f || inputManager.cameraY != 0f)
                {
                    camOrbitAxis.Rotate(GetCamOrbitAxisTargetRot(inputManager.cameraX, -inputManager.cameraY, camAxisRotDamping_C, invertCamera, true));
                    camOrbitAxis.eulerAngles = new Vector3(camOrbitAxis.eulerAngles.x, camOrbitAxis.eulerAngles.y, 0f);
                }
            }

            camAxisVerticalAngle = Vector3.SignedAngle(camOrbitAxis.up, Vector3.up, camOrbitAxis.right);

            if (camAxisVerticalAngle > camAxisLowerAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(-camAxisLowerAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
            }
            else if (camAxisVerticalAngle < -camAxisUpperAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(camAxisUpperAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
            }
        }

        private void SetCamPosRot()
        {
            camTargetRotation = Quaternion.LookRotation(camOrbitAxis.position - transform.position, camOrbitAxis.up);

            if (transform.position != camTargetPosition)
            {
                transform.position = Vector3.Lerp(transform.position, camTargetPosition, camPosDamping * Time.deltaTime);
            }

            if (transform.rotation != camTargetRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, camTargetRotation, camRotDamping * Time.deltaTime);
            }
            
        }

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
                Gizmos.DrawWireSphere(sensorHit.point, camSensorCastRadius);
            }
            //If there hasn't been a hit yet, draw the ray at the maximum distance
            else
            {
                if (Gizmos.color != Color.green) { Gizmos.color = Color.green; }
                //Draw a Ray forward from GameObject toward the maximum distance
                Gizmos.DrawRay(camOrbitAxis.position, -camOrbitAxis.forward * camSensorProbeDistance);
                //Draw a cube at the maximum distance
                //Gizmos.DrawWireCube(transform.position + transform.forward * m_MaxDistance, transform.localScale);
                Gizmos.DrawWireSphere(camOrbitAxis.position + -camOrbitAxis.forward * camSensorProbeDistance, camSensorCastRadius);
            }
        }
    }
}

