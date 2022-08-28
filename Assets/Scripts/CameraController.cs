using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        private InputManager inputManager;

        public enum LookMode { Forward, Free }
        public LookMode lookMode;

        //[Tooltip("This will automatically be set to the CameraAxis gameobject which is assigned at runtime. " + 
        //    "However if you would like to manually retarget the camera axis to a different game object, " +
        //    "set this to that object. Keep in mind the function of this is to serve as a 'desired' " + 
        //    "rotation and position for the camera itself. The camera will always try to match this " + 
        //    "rotation and position based on the dampers and other variables settings.")]
        public Transform camAxis;

        [Tooltip("Set this to the object you would like the camera focus to orbit around. " +
            "This would typically be something like the players head or the cockpit of a v" +
            "ehicle, however it can be any transform so long as it is the object intended " +
            "for the camera to orbit.")]
        public Transform cameraOrbitTarget;
        //[Tooltip("This will automatically be set to at runtime. This is the hinge around which the camera " + 
        //    "will orbit. It is typically set to an empty game object at the center of where the camera " + 
        //    "should orbit, such as a players head, or the center of a vehicle.")]
        public Transform cameraOrbitDeltaTarget;

        

        public bool invertCamera;

        public float camAxisRotDamping_C = 150f;
        public float camAxisRotDamping_M = 100f;
        public float camAxisAngleClamp = 85f;
        private float camVerticalAxisAngle;

        private Vector3 camPosition;
        private Quaternion camRotation;
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
                Debug.LogError("Please add a player object with a InputManager component attached to the scene.");
            } 
            else inputManager = FindObjectOfType<InputManager>();

            //if (!FindObjectOfType<InputManager>().GetComponent<Collider>())
            //{
            //    Debug.LogError("Please ensure the hover player gameobject has a collider");
            //}
            //else hoverPlayerCollider = FindObjectOfType<InputManager>().GetComponent<Collider>();

            if (!GameObject.FindGameObjectWithTag("CameraAxis"))
            {
                Debug.LogError("The camera axis object is missing from the scene. Create an " +
                    "empty GameObject with the tag 'CameraAxis' then add it as a child of th" +
                    "e camera that contains this script.");
            } 
            else camAxis = GameObject.FindGameObjectWithTag("CameraAxis").transform;

            if (!GameObject.FindGameObjectWithTag("CameraOrbitDeltaTarget"))
            {
                Debug.LogError("The camera orbit delta target object is missing from the scene. Create an " +
                    "empty GameObject with the tag 'CameraOrbitDeltaTarget' then add it as a child of th" +
                    "e camera that contains this script.");
            } 
            else cameraOrbitDeltaTarget = GameObject.FindGameObjectWithTag("CameraOrbitDeltaTarget").transform;

            if (!cameraOrbitTarget)
            {
                Debug.LogError("The camera orbit target has not been set. An object must manually be assigned to this" +
                    "this variable in the inspector. This is the static point for which the entire camera system is hinged." +
                    "Typically, this varible would be set to a players head or a vehicles cockpit.");
            }
            else { 
                cameraOrbitDeltaTarget.parent = cameraOrbitTarget.transform;
                cameraOrbitDeltaTarget.localPosition = Vector3.zero;
                cameraOrbitDeltaTarget.localRotation = Quaternion.identity;
            }
            
            transform.parent = null;
            camAxis.transform.parent = null;
            lookMode = LookMode.Free;
        }

        private void Update()
        {

        }

        private void LateUpdate()
        {
            CheckForInverted();

            SetCamAxisPosRot();

            SetCamPosRot();
        }

        private void FixedUpdate()
        {
            GetBumpPos();
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
            if (camAxis.position != cameraOrbitDeltaTarget.position)
            {
                camAxis.position = cameraOrbitDeltaTarget.position;
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
                    camAxis.Rotate(GetCameraRotation(inputManager.cameraX, -inputManager.cameraY, camAxisRotDamping_C, invertCamera, true));
                    camAxis.eulerAngles = new Vector3(camAxis.eulerAngles.x, camAxis.eulerAngles.y, 0f);
                }
            }

            camVerticalAxisAngle = Vector3.SignedAngle(camAxis.up, Vector3.up, camAxis.right);

            if (camVerticalAxisAngle > camAxisAngleClamp)
            {
                camAxis.eulerAngles = new Vector3(-camAxisAngleClamp, camAxis.eulerAngles.y, 0f);
            }
            else if (camVerticalAxisAngle < -camAxisAngleClamp)
            {
                camAxis.eulerAngles = new Vector3(camAxisAngleClamp, camAxis.eulerAngles.y, 0f);
            }
        }

        private void SetCamPosRot()
        {
            camRotation = Quaternion.LookRotation(camAxis.position - transform.position, camAxis.up);

            if (transform.position != camPosition)
            {
                transform.position = Vector3.Lerp(transform.position, camPosition, camPosDamping * Time.deltaTime);
            }

            if (transform.rotation != camRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, camRotation, camRotDamping * Time.deltaTime);
            }
        }

        private void GetBumpPos()
        {
            RaycastHit bumpHit;
            bumpRay = new Ray(camAxis.position, -camAxis.forward);

            if (Physics.SphereCast(bumpRay, camBumpCastRadius, out bumpHit, camBumpLength, hoverPlayerLayerMask))
            {
                camPosition = new Vector3(bumpHit.point.x, bumpHit.point.y + camBumpHeight, bumpHit.point.z);
            }
            else
            {
                camPosition = camAxis.TransformPoint(new Vector3(0f, camBumpHeight, -camBumpLength));
            }
        }

        private Vector3 GetCameraRotation(float horz, float vert, float damp, bool inverted, bool useDeltaTime)
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

