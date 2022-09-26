using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverTest : MonoBehaviour
{
    public Rigidbody rb;

    public LayerMask hoverForceInteractionMask;

    public float yForce = 80f; // requires tuning
    public float dampenFactor = 0.8f; // requires tuning
    public float offsetFactor = 0.5f; // requires tuning

    public float targetHeight = 10f;

    private void FixedUpdate()
    {
        RaycastHit hit;
        
        Physics.Raycast(rb.transform.position, Vector3.down, out hit, Mathf.Infinity, hoverForceInteractionMask);
        //Debug.Log(hit.distance);
        if (hit.distance < targetHeight && hit.distance > 0)
        {
            float availableForce = yForce;

            // cancel out downward velocity
            if (rb.velocity.y < 0)
            {
                // Cap out upward force based on yForce
                float cappedDampenForce = Mathf.Min(dampenFactor * -rb.velocity.y,
                        availableForce);

                // How much force is available for the offset?
                availableForce -= cappedDampenForce;

                rb.AddForce(Vector3.up * cappedDampenForce, ForceMode.Acceleration);
                //Debug.Log("applied dampening force");
            }

            // Find upward force scaled by distance left to target height, and cap that amount
            float cappedOffsetForce = Mathf.Min(offsetFactor * (targetHeight - hit.distance),
                    availableForce);

            rb.AddForce(Vector3.up * cappedOffsetForce, ForceMode.Acceleration);
            //Debug.Log("applied offset force");
        }
    }
    
}
