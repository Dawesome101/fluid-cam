using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{   
    public class HoverEngine : MonoBehaviour
    {
        public Rigidbody rb;

        public LayerMask hoverForceInteractionMask;

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
        public bool debugVisualization; 
        
        private GameObject debugSphere;

        private Material debugMatHit;
        private Material debugMatMiss;

        

        
        protected void Awake()
        {

            debugMatHit = Resources.Load<Material>("Materials/GreenTransparent");
            debugMatMiss = Resources.Load<Material>("Materials/RedTransparent");

            if (TryGetComponent(out Rigidbody rigidB))
            {
                rb = rigidB;
            }
            else 
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

        }

        protected void FixedUpdate()
        {
            heightRay = new Ray(transform.position, Vector3.down);

            if (debugVisualization && debugSphere == null)
            {
                debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugSphere.transform.localScale = new Vector3(rayBumperRadius * 2, rayBumperRadius * 2, rayBumperRadius * 2);
                Destroy(debugSphere.GetComponent<Collider>());
                
            }

            Collider[] overlaps = Physics.OverlapSphere(transform.position, rayBumperRadius, hoverForceInteractionMask);
            if (overlaps.Length > 0)
            {
                //add force here.
                rb.AddForce(0f, force, 0f);
                if (debugVisualization)
                {
                    debugSphere.transform.position = transform.position;
                    debugSphere.GetComponent<MeshRenderer>().material = debugMatHit;
                }
            }
            else
            {
                if (Physics.Raycast(heightRay, out heightRayHit, targetHeight, hoverForceInteractionMask))
                {
                    //add force here.
                    rb.AddForce(0f, force, 0f);
                    if (debugVisualization)
                    {
                        Debug.DrawLine(transform.position, heightRayHit.point, Color.green);
                        debugSphere.transform.position = heightRayHit.point;
                    }
                }
                else
                {
                    if (debugVisualization)
                    {
                        Debug.DrawLine(transform.position, new Vector3 (transform.position.x, transform.position.y - targetHeight, transform.position.z), Color.red);

                        if (debugSphere != null)
                        {
                            debugSphere.transform.position = new Vector3(transform.position.x, transform.position.y - targetHeight, transform.position.z);
                            debugSphere.GetComponent<MeshRenderer>().material = debugMatMiss;
                        }
                    }
                }
            }
        }
    }
}

