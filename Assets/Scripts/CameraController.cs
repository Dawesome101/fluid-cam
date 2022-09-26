
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        private InputManager inputManager;

        // camOrbitAxis is a game object with only a transform component which has a tag of CamOrbitAxis.
        //it is used to 
        private Transform camOrbitAxis; 
        private Transform camOrbitAxisTargetPos;

        [Tooltip("This is the object the camera will attempt to focus on. " +
            "Typically this would be something like the players head or the " +
            "cockpit of a vehicle, however it can be any transform so long " +
            "as it is the object intended for the camera to focus on.")]
        public Transform camFocusSubject;
        public enum LookMode { Forward, Free }
        public LookMode lookMode;

        public bool invertCamera;

        public float camAxisRotDamping_C = 150f;
        public float camAxisAngleClamp = 85f;
        private float camVerticalAxisAngle;

        private Vector3 camTargetPosition;
        private Quaternion camTargetRotation;
        public float camPosDamping = 8f;
        public float camRotDamping = 8f;

        private Ray bumpRay;
        public float camBumpCastRadius = 0.25f;
        public float camBumpLength = 5f;
        public float camHeight = 2f;
        public float camBumpHeight = 1f;

        [Tooltip("Make sure to remove all layers from this mask that you do not want the " +
            "camera bumper ray to collide with. Typically this mask should only include t" +
            "he default layer but you may include any layers you wish so long as you want the" +
            "camera to treat them as bumper objects.")]
        public LayerMask hoverPlayerLayerMask;

        private void Awake()
        {
            if (!FindObjectOfType<InputManager>())
            {
                Debug.LogError("CameraController needs a GameObject with an InputManager component attached to it in the scene.");
            } 
            else inputManager = FindObjectOfType<InputManager>();

            if (!GameObject.FindGameObjectWithTag("CameraOrbitAxis"))
            {
                Debug.LogError("The camera orbit axis object is missing from the scene. Create an " +
                    "empty GameObject with the tag 'CameraOrbitAxis' then add it as a child of " +
                    "the camera using the CameraController component.");
            } 
            else camOrbitAxis = GameObject.FindGameObjectWithTag("CameraOrbitAxis").transform;

            if (!GameObject.FindGameObjectWithTag("CameraOrbitAxisTarget"))
            {
                Debug.LogError("The camera orbit axis target object is missing from the scene. " +
                    "Create an empty GameObject with the tag 'CameraOrbitAxisTarget' then add it " +
                    "as a child of the camera using the CameraController component.");
            } 
            else camOrbitAxisTargetPos = GameObject.FindGameObjectWithTag("CameraOrbitAxisTarget").transform;

            if (!camFocusSubject)
            {
                Debug.LogError("The camera will attempt to focus on the transform assigned to " +
                    "camFocusSubject which has not yet been set in the inspector. The transform " +
                    "intended as the cameras subject must manually be assigned to Cam Focus Subject " +
                    "in the inspector. Typically this would be set to a players head or a vehicles cockpit.");
            }
            else
            {
                camOrbitAxisTargetPos.parent = camFocusSubject.transform;
                camOrbitAxisTargetPos.localPosition = Vector3.zero;
                camOrbitAxisTargetPos.localRotation = Quaternion.identity;
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
            RaycastHit bumpHit;
            bumpRay = new Ray(camOrbitAxis.position, -camOrbitAxis.forward);

            if (Physics.SphereCast(bumpRay, camBumpCastRadius, out bumpHit, camBumpLength, hoverPlayerLayerMask))
            {
                camTargetPosition = new Vector3(bumpHit.point.x, bumpHit.point.y + camBumpHeight, bumpHit.point.z);
            }
            else
            {
                camTargetPosition = camOrbitAxis.TransformPoint(new Vector3(0f, camBumpHeight, -camBumpLength));
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
            if (camOrbitAxis.position != camOrbitAxisTargetPos.position)
            {
                camOrbitAxis.position = camOrbitAxisTargetPos.position;
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

            camVerticalAxisAngle = Vector3.SignedAngle(camOrbitAxis.up, Vector3.up, camOrbitAxis.right);

            if (camVerticalAxisAngle > camAxisAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(-camAxisAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
            }
            else if (camVerticalAxisAngle < -camAxisAngleClamp)
            {
                camOrbitAxis.eulerAngles = new Vector3(camAxisAngleClamp, camOrbitAxis.eulerAngles.y, 0f);
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
    }
}

