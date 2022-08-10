using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky
{
    public class CameraController : MonoBehaviour
    {
        public enum LookMode { Forward, Free }
        public LookMode lookMode;

        private HoverController hoverController;
        private Collider hoverPlayerCollider;
        public Transform camAxis;
        private Transform camCenter;

        public bool invertMouse;
        public bool invertGamepad;

        public float camAxisRotDamping_C = 150f;
        public float camAxisRotDamping_M = 100f;
        public float camAxisAngleClamp = 85f;
        private float camVerticalAxisAngle;

        private Vector3 camPosition;
        private Quaternion camRotation;
        public float camPosDamping = 8f;
        public float camRotDamping = 8f;

        private Ray bumpRay;
        public float camBumpLength = 5f;
        public float camHeight = 2f;
        public float camBumpHeight = 1f;
        public LayerMask hoverPlayerLayerMask;

        private void Awake()
        {
            if (!FindObjectOfType<HoverController>())
            {
                Debug.LogError("Please add a player object with a HoverController component attached to the scene.");
            } 
            else hoverController = FindObjectOfType<HoverController>();

            if (!FindObjectOfType<HoverController>().GetComponent<Collider>())
            {
                Debug.LogError("Please ensure the hover player gameobject has a collider");
            }
            else hoverPlayerCollider = FindObjectOfType<HoverController>().GetComponent<Collider>();

            if (!GameObject.FindGameObjectWithTag("CameraAxis"))
            {
                Debug.LogError("Please add an empty GameObject with the tag 'CameraAxis' to the scene.");
            } 
            else camAxis = GameObject.FindGameObjectWithTag("CameraAxis").transform;

            if (!GameObject.FindGameObjectWithTag("CameraAxisCenter"))
            {
                Debug.LogError("Please tag an object desired as the centerpoint for the camera hinge with the tag 'CameraAxisCenter'.");

            } 
            else camCenter = GameObject.FindGameObjectWithTag("CameraAxisCenter").transform;
            
            transform.parent = null;
            camAxis.transform.parent = null;
            lookMode = LookMode.Free;
        }

        private void Update()
        {
            //if (Input.GetButtonDown("R_Stick") && !player.jumped)
            //{
            //    if (lookMode == LookMode.Forward && !lookModeTransitionActive)
            //    {
            //        lookMode = LookMode.Free;
            //    }
            //    else if (lookMode == LookMode.Free && !lookModeTransitionActive)
            //    {
            //        player.playerLookModeTransitionRot.y = player.cc.transform.eulerAngles.y;
            //        player.camAxisLookModeTransitionRot.y = camAxis.eulerAngles.y;
            //        player.lookModeTransitionStartTime = Time.time;

            //        lookMode = LookMode.Forward;
            //        lookModeTransitionActive = true;
            //    }
            //}
        }

        private void LateUpdate()
        {
            SetCamAxisPosRot();

            SetCamPosRot();
        }

        private void FixedUpdate()
        {
            GetBumpPos();
        }

        public bool lookModeTransitionActive;
        private void SetCamAxisPosRot()
        {
            if (camAxis.position != camCenter.position)
            {
                camAxis.position = camCenter.position;
            }

            if (lookMode == LookMode.Forward)
            {
                //if (Input.GetAxisRaw("Mouse_Y") != 0f)
                //{
                //    camAxis.Rotate(GetCameraRotation(0f, Input.GetAxis("Mouse_Y"), camAxisRotDamping_M, invertMouse, true));
                //}
                //else 
                
                if (hoverController.camRotValueY != 0f)
                {
                    camAxis.Rotate(GetCameraRotation(0f, -hoverController.camRotValueY, camAxisRotDamping_C, invertGamepad, true));
                }

                if (!lookModeTransitionActive)
                {
                    camAxis.eulerAngles = new Vector3(camAxis.eulerAngles.x, hoverController.transform.eulerAngles.y, 0f);
                }
            }
            else if (lookMode == LookMode.Free)
            {
                //if (Input.GetAxis("Mouse_X") != 0f || Input.GetAxisRaw("Mouse_Y") != 0f)
                //{
                //    camAxis.Rotate(GetCameraRotation(Input.GetAxis("Mouse_X"), Input.GetAxis("Mouse_Y"), camAxisRotDamping_M, invertMouse, true));
                //    camAxis.eulerAngles = new Vector3(camAxis.eulerAngles.x, camAxis.eulerAngles.y, 0f);
                //}
                //else 
                
                if (hoverController.camRotValueX != 0f || hoverController.camRotValueY != 0f)
                {
                    camAxis.Rotate(GetCameraRotation(hoverController.camRotValueX, -hoverController.camRotValueY, camAxisRotDamping_C, invertGamepad, true));
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

            if (transform.rotation != camRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, camRotation, camRotDamping * Time.deltaTime);
            }

            if (transform.position != camPosition)
            {
                transform.position = Vector3.Lerp(transform.position, camPosition, camPosDamping * Time.deltaTime);
            }
        }

        private void GetBumpPos()
        {
            RaycastHit bumpHit;
            bumpRay = new Ray(camAxis.position, -camAxis.forward);

            if (Physics.SphereCast(bumpRay, (hoverController.gameObject.transform.localScale.x/2), out bumpHit, camBumpLength, hoverPlayerLayerMask))
            {
                camPosition = new Vector3(bumpHit.point.x, bumpHit.point.y + camBumpHeight, bumpHit.point.z);
            }
            else
            {
                camPosition = camAxis.TransformPoint(new Vector3(0f, camBumpHeight, -camBumpLength));
            }

            //if (Physics.Raycast(bumpRay, out bumpHit, camBumpLength, playerLayerMask))
            //{
            //    camPosition = new Vector3(bumpHit.point.x, bumpHit.point.y + camBumpHeight, bumpHit.point.z);
            //}
            //else
            //{
            //    camPosition = camAxis.TransformPoint(new Vector3(0f, camBumpHeight, -camBumpLength));
            //}
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

