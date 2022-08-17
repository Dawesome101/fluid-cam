using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{
    public class HoverDrive : MonoBehaviour
    {
        private InputManager inputManager;

        private Rigidbody rb;

        public float mass;
        public float drag;
        public float force;

        protected virtual void Awake()
        {
            if (!FindObjectOfType<InputManager>())
            {
                Debug.LogError("Please add a player object that has an InputManager component to " + this);
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
        }


        private void FixedUpdate()
        {
            if (inputManager.moveX != 0f || inputManager.moveZ != 0f) 
            {
                rb.AddRelativeForce(new Vector3(inputManager.moveX, 0f, inputManager.moveZ) * force, ForceMode.Force);
            }
        }

        protected virtual void SetupRigidbody(float rbMass, float rbDrag, float driveForce)
        {
            rb.mass = rbMass;
            rb.drag = rbDrag;
            force = driveForce;
        }
    }
}

