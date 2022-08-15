using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{   
    public class HoverEngine : MonoBehaviour
    {
        protected Rigidbody rb;
        protected Collider col;

        public float mass;
        public float drag;
        public float force;
        public float rayBumperRadius;

        [Tooltip("The target height is the distance from the ground the vehicle will attempt to hover. If the" +
            " vehicle is too heavy to reach the target height it will need more engines.")]
        public float targetHeight;

        protected Ray heightRay;
        private RaycastHit heightRayHit;

        [Header("Debug")]
        private GameObject debugSphere;
        private Color castSphereColor;
        
        public LayerMask hoverForceInteractionMask;

        public bool debugVisualization;
        protected void Awake()
        {
            castSphereColor = new Vector4(255f, 0f, 0f, 0.5f);
        }

        protected void FixedUpdate()
        {
            heightRay = new Ray(transform.position, Vector3.down);

            if (debugVisualization && debugSphere == null)
            {
                debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.localScale = new Vector3(rayBumperRadius * 2, rayBumperRadius * 2, rayBumperRadius * 2);
                Destroy(debugSphere.GetComponent<Collider>());
                debugSphere.GetComponent<MeshRenderer>().material.color = castSphereColor;
            }

            if (debugVisualization)
            {
                
            }

            

            Collider[] overlaps = Physics.OverlapSphere(transform.position, rayBumperRadius, hoverForceInteractionMask);
            if (overlaps.Length > 0)
            {
                //add force here.
                if (debugVisualization)
                {
                    debugSphere.transform.position = transform.position;
                }
            }
            else
            {
                if (Physics.Raycast(heightRay, out heightRayHit, targetHeight, hoverForceInteractionMask))
                {
                    //add force here.
                    if (debugVisualization)
                    {
                        Debug.DrawLine(transform.position, heightRayHit.point);
                        debugSphere.transform.position = heightRayHit.point;
                    }
                }
                else
                {
                    if (debugVisualization)
                    {
                        Debug.DrawLine(transform.position, new Vector3 (transform.position.x, transform.position.y - targetHeight, transform.position.z));

                        if (debugSphere != null)
                        {

                            Destroy(debugSphere);
                        }
                    }
                }
            }
        }
    }
}

