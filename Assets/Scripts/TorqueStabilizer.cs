using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{    public class TorqueStabilizer : MonoBehaviour
    {
        public Rigidbody rb;
        public float stability = 0.3f;
        public float speed = 2.0f;
        public Vector3 wantedUp = new Vector3(0, 1, 0);
        public bool singleAxis;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.Z))
            {
                Vector3 predictedUp = Quaternion.AngleAxis(
                rb.angularVelocity.magnitude * Mathf.Rad2Deg * stability / speed,
                rb.angularVelocity
                ) * transform.up;

                Vector3 torqueVector = Vector3.Cross(predictedUp, wantedUp);

                if (singleAxis)
                {
                    torqueVector = Vector3.Project(torqueVector, transform.forward);
                }

                rb.AddTorque(torqueVector * speed);
            }

            if (Input.GetKey(KeyCode.L))
            {
                rb.AddTorque(new Vector3(30f, 30f, 30f));
            }
        }
    }
}

