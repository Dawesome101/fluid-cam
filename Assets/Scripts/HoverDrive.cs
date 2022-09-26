using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{
    public class HoverDrive : MonoBehaviour
    {
        private InputManager inputManager;

        private Rigidbody rb;

        private Camera cam;

        public float mass;
        public float drag;
        public float force;

        protected virtual void Awake()
        {
            if (!FindObjectOfType<InputManager>())
            {
                Debug.LogError("Please add a player object that has an InputManager component to the scene.");
            }
            else inputManager = FindObjectOfType<InputManager>();

            if (TryGetComponent(out Rigidbody rigidB))
            {
                rb = rigidB;
            }
            else
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            if (cam == null)
            {
                //Camera[] tempArr = FindObjectsOfType<Camera>();

                cam = Camera.main;
                //if (tempArr.Length == 0)
                //{
                //    Debug.LogError("Please add a main camera to the scene.");
                //}
                //foreach (Camera c in tempArr)
                //{
                //    if (c == Camera.main) 
                //    {
                //        cam = c;
                //        break;
                //    }
                //}
            }
        }

        public float inputAngleRaw;
        public float multiplier = 100f;

       

        private void FixedUpdate()
        {
            //if (inputManager.moveX != 0f || inputManager.moveZ != 0f) 
            //{
            //    rb.AddRelativeForce(new Vector3(, 0f, inputManager.moveZ) * force, ForceMode.Force);
            //}

            //inputAngleRaw = MathTools.GetInputAngle(new Vector2(Input.GetAxis("L_Horizontal"), Input.GetAxis("L_Vertical")), 1);
            //Debug.Log("Forward: " + inputAngleRaw);

            inputAngleRaw = MathTools.GetInputAngleOfRootRelativeToCamera(
                new Vector2(inputManager.moveX, inputManager.moveZ),
                transform,
                cam.transform,
                1,
                false);
            //Debug.Log("Free: " + inputAngleRaw);

            Vector3 controlDirection = new Vector3(inputManager.moveX, 0, inputManager.moveZ);
            Vector3 actualDirection = Camera.main.transform.TransformDirection(controlDirection);

            rb.AddForce(actualDirection * force);
            //AddForceAtAngle(force, inputAngleRaw);



            //Vector3 camForward = Camera.main.transform.forward;
            //Vector3 rbForward = rb.transform.forward;

            //Vector3 torque = Vector3.Cross(camForward, -rbForward);
            //rb.AddTorque(0f, torque.y * multiplier, 0f);





            //if (cam.lookMode == Camera_Controller.LookMode.Forward)
            //{
            //    inputAngleRaw = MathTools.GetInputAngle(new Vector2(Input.GetAxis("L_Horizontal"), Input.GetAxis("L_Vertical")), 1);
            //}
            //else if (cam.lookMode == Camera_Controller.LookMode.Free)
            //{
            //    inputAngleRaw = MathTools.GetInputAngleOfRootRelativeToCamera(new Vector2(Input.GetAxis("L_Horizontal"), Input.GetAxis("L_Vertical")), cc.transform, cam.transform, 1, false);
            //}






            //inputAngleRaw = MathTools.GetInputAngleOfRootRelativeToCamera(
            //    new Vector2(inputManager.moveX, inputManager.moveZ),
            //    transform,
            //    cam.transform,
            //    1,
            //    false);

            //Debug.Log(inputAngleRaw);
            //if (Mathf.Abs(inputAngleLast - inputAngleRaw) >= inputAngleMaxChangeDistance)
            //{
            //    inputAngleSmooth = inputAngleRaw;
            //}
            //else
            //{
            //    inputAngleSmooth = Mathf.Lerp(inputAngleSmooth, inputAngleRaw, inputAngleDamping * Time.deltaTime);
            //}

            //inputAngleLast = inputAngleRaw;

            //if (anim.GetFloat(moveAngle) != inputAngleSmooth)
            //{
            //    anim.SetFloat(moveAngle, inputAngleSmooth);
            //}
        }

        public void AddForceAtAngle(float force, float angle)
        {
            angle *= Mathf.Deg2Rad;
            float xComponent = Mathf.Cos(angle) * force;
            float zComponent = Mathf.Sin(angle) * force;
            Vector3 forceApplied = new Vector3(xComponent, 0, zComponent);

            rb.AddForce(forceApplied);
        }
        protected virtual void SetupRigidbody(float rbMass, float rbDrag, float driveForce)
        {
            rb.mass = rbMass;
            rb.drag = rbDrag;
            force = driveForce;
        }
    }
}

