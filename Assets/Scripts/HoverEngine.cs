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
        public float forceDivisor;
        //remove forceActual from public once forces have been tested
        public float forceActual;
        

        [Tooltip("The target height is the distance from the ground the vehicle will attempt to hover. If the" +
            " vehicle is too heavy to reach the target height it will need more engines.")]
        public float targetHeight;

        protected Ray heightRay;
        private RaycastHit heightRayHit;
        public float rayBumperRadius;

        [Header("Stabilization")]
        public bool applyStabilization;
        [Tooltip("This literally divides the force by this number and applys the quotient as a downward " +
            "force to keep the craft from bobbing up and down.")]
        public float stabilizationDivisor;

        [Header("Debug")]
        public bool debugVisualization; 
        
        private GameObject debugSphere;

        private Material debugMatHit;
        private Material debugMatMiss;

        protected virtual void Awake()
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

            //Check to make sure the initial start of the engine force ray has overlapped anything
            Collider[] overlaps = Physics.OverlapSphere(transform.position, rayBumperRadius, hoverForceInteractionMask);
            if (overlaps.Length > 0)
            {
                if (applyStabilization)
                {
                    forceActual = CalculateEngineForce(targetHeight, heightRayHit.distance, force);
                }
                else
                {
                    forceActual = force;
                }
                
                rb.AddForce(0f, forceActual, 0f, ForceMode.Force);

                if (debugVisualization)
                {
                    debugSphere.transform.position = transform.position;
                    debugSphere.GetComponent<MeshRenderer>().material = debugMatHit;
                }
            }
            //If the overlap is clear, fire ray straight downward at the ground looking for a collision
            else
            {
                //If the ray hits something, apply global upward force from the engine center
                if (Physics.Raycast(heightRay, out heightRayHit, targetHeight, hoverForceInteractionMask))
                {
                    if (applyStabilization)
                    {
                        forceActual = CalculateEngineForce(targetHeight, heightRayHit.distance, force);
                    }
                    else
                    {
                        forceActual = force;
                    }
                    
                    rb.AddForce(0f, forceActual, 0f, ForceMode.Force);

                    if (debugVisualization)
                    {
                        Debug.DrawLine(transform.position, heightRayHit.point, Color.green);
                        debugSphere.GetComponent<MeshRenderer>().material = debugMatHit;
                        debugSphere.transform.position = heightRayHit.point;
                    }
                }
                else
                {
                    if (applyStabilization)
                    {
                        rb.AddForce(0f, -force * stabilizationDivisor, 0f);
                    }

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

        protected virtual void SetupRigidbody(float rbMass, float rbDrag, float engineForce) 
        {
            rb.mass = rbMass;
            rb.drag = rbDrag;
            force = engineForce;
        }

        
        private float CalculateEngineForce(float tHeight, float rayHitDistance, float f) 
        {
            float percentToGround = ((tHeight - rayHitDistance) / tHeight) * 100f;
            percentToGround = Mathf.InverseLerp(0f, 100f, percentToGround);
            return Mathf.Lerp((f / forceDivisor), f, percentToGround);
        }
    }
}

