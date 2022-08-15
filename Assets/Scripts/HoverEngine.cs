using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{   
    public class HoverEngine : MonoBehaviour
    {
        protected Rigidbody rb;

        public float mass;
        public float drag;
        public float force;

        [Tooltip("This is the radius of the sphere used to probe for what the engine force has hit. " +
            "The sphere will travel downward from the hover engine as far as the hover height, if it " +
            "hits anything it will apply force in the opposite direction. If it does not hit anything, " +
            "no force is applied. This should be equal to the extents or radius of the engines collider.  " +
            "If one is not set, the script will calculate the size to be the same as the colliders bounds. " +
            "The bumper ray is a sphereCast so this number should equal the radius you want. For example: " +
            "if the collider scale is 1x 1y 1z, this should be set to 0.5")]
        public float rayBumperRadius;

        [Tooltip("The target height is the distance from the ground the vehicle will attempt to hover. If the" +
            " vehicle is too heavy to reach the target height it will need more engines.")]
        public float targetHeight;

        protected Ray heightRay;
        protected RaycastHit heightRayHit;

        public LayerMask hoverForceInteractionMask;

        public bool debugVisualization;
        protected void Awake()
        {
            if (TryGetComponent(out Collider col))
            {
                if (col.attachedRigidbody != null)
                {
                    rb = col.attachedRigidbody;
                }
                else
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                    Debug.Log("A Rigidbody was added to " + transform.name + " because a Rigidbody is required for " +
                        this + " to function. It is recommended that one be added to the object using the editor prior " +
                        "to entering play mode.");
                }
            }
            else 
            {
                Debug.Log("A collider was not detected on " + transform.name + ". It is strongly recommended that one " +
                    "be manually added to the object using the editor prior to entering play mode, however a box collider " +
                    "and rigidbody have been added to the object for the time being.");
                gameObject.AddComponent<BoxCollider>();
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.mass = mass;
            rb.drag = drag;
        }

        protected void FixedUpdate()
        {
            if (debugVisualization)
            {
                Debug.DrawLine(transform.position, -Vector3.down * targetHeight);
            }

            heightRay = new Ray(transform.position, -Vector3.up);

            //if(Physics.OverlapSphere)
        }
    }
}

