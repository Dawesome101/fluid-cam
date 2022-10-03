using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSky 
{
    public class MathTools : MonoBehaviour
    {
        /// <summary>
        /// Remaps the horizontal/vertical input to a circle instead of a square.
        /// When used in tandom with a analog controler it prevents the issue of characters 
        /// speeding up when moving diagonally while maintaining the analog stick sensivity.
        /// </summary>
        public static Vector2 GetAnalogStickCircle(string horizontal = "Horizontal", string vertical = "Vertical")
        {
            // apply some error margin, because the analog stick typically does not
            // reach the corner entirely
            const float error = 1.1f;

            // clamp input with error margin
            var input = new Vector2(
                Mathf.Clamp(Input.GetAxisRaw(horizontal) * error, -1f, 1f),
                Mathf.Clamp(Input.GetAxisRaw(vertical) * error, -1f, 1f)
            );

            // map square input to circle, to maintain uniform speed in all
            // directions
            return new Vector2(
                input.x * Mathf.Sqrt(1 - input.y * input.y * 0.5f),
                input.y * Mathf.Sqrt(1 - input.x * input.x * 0.5f)
            );
        }

        /// <summary>
        /// Calculates an angle from the input vector. Use angleType 0 for radians, use angleType 1 for -180 to 180 and angleType 2 for 360.
        /// </summary>
        /// <param name="inputVec"></param>
        /// <param name="angleType"></param>
        /// <returns>a radian, a value between -180 to 180 or a value between 0 and 360</returns>
        /// <value>Radians or degrees.</value>
        public static float GetInputAngle(Vector2 inputVec, int angleType)
        {
            if (angleType == 0)
            {
                return Mathf.Atan2(inputVec.x, inputVec.y);
            }
            else if (angleType == 1)
            {
                return Mathf.Atan2(inputVec.x, inputVec.y) * Mathf.Rad2Deg;
            }
            else
            {
                float tempAngle = Mathf.Atan2(inputVec.x, inputVec.y) * Mathf.Rad2Deg;

                if (tempAngle < 0f)
                {
                    tempAngle += 360f;
                }

                return tempAngle;
            }
        }

        /// <summary>
        /// Get the percentage of one numbers difference between two numbers.
        /// </summary>
        /// <param name="lowNum"></param>
        /// <param name="highNum"></param>
        /// <param name="targetNum"></param>
        public static float PercentDiffBetweenTwoNumbers(float startNum, float endNum, float targetNum)
        {
            return (targetNum - startNum) / (endNum - startNum);
        }

        /// <summary>
        /// Gets the angle between the analog stick direction and the player facing in reference to the camera direction.  Output type 0 will give a direction between -1 and 1, other output will be between -180 to 180.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="camera"></param>
        /// <param name="outputType"></param>
        public static float GetInputAngleOfRootRelativeToCamera(Vector2 input, Transform rootTrans, Transform camera, int outputType, bool useDebug)
        {
            Vector3 inputDirection = new Vector3(input.x, 0, input.y);

            Vector3 CamDirection = camera.forward;
            CamDirection.y = 0f;
            Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(CamDirection));

            Vector3 moveDirection = referentialShift * inputDirection;
            Vector3 axisSign = Vector3.Cross(moveDirection, rootTrans.forward);

            if (useDebug)
            {
                Debug.DrawRay(new Vector3(rootTrans.position.x, rootTrans.position.y + 2f, rootTrans.position.z), moveDirection, Color.green);
                Debug.DrawRay(new Vector3(rootTrans.position.x, rootTrans.position.y + 2f, rootTrans.position.z), rootTrans.forward, Color.magenta);
                Debug.DrawRay(new Vector3(rootTrans.position.x, rootTrans.position.y + 2f, rootTrans.position.z), inputDirection, Color.blue);
                Debug.DrawRay(new Vector3(rootTrans.position.x, rootTrans.position.y + 2.5f, rootTrans.position.z), axisSign, Color.red);
            }

            float angleToMove = Vector3.Angle(rootTrans.forward, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

            if (outputType == 0)
            {
                return angleToMove /= 180f;
            }
            else return angleToMove;
        }

        /// <summary>
        /// True linear interpolation of input values intended for use over time.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="startTime"></param>
        /// <param name="timeLength"></param>
        /// <returns>A rounded interpolated value as an Int.</returns>
        public static float IntLerp(float start, float target, float startTime, float timeLength)
        {
            float timeLerping = Time.time - startTime;

            float percentageComplete = timeLerping / timeLength;

            return Mathf.RoundToInt(Mathf.Lerp(start, target, percentageComplete));
        }

        /// <summary>
        /// True float linear interpolation of input values intended for use over time.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="startTime"></param>
        /// <param name="timeLength"></param>
        /// <returns>An interpolated value as a float.</returns>
        public static float FloatLerp(float start, float target, float startTime, float timeLength)
        {
            float timeLerping = Time.time - startTime;

            float percentageComplete = timeLerping / timeLength;

            return Mathf.Lerp(start, target, percentageComplete);
        }

        /// <summary>
        /// True Vector3 linear interpolation of input values intended for use over time.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="startTime"></param>
        /// <param name="timeLength"></param>
        /// <returns>An interpolated value as a Vector3.</returns>
        public static Vector3 Vec3Lerp(Vector3 start, Vector3 target, float startTime, float timeLength)
        {
            float timeLerping = Time.time - startTime;

            float percentageComplete = timeLerping / timeLength;

            return Vector3.Lerp(start, target, percentageComplete);
        }

        /// <summary>
        /// True Quaternion linear interpolation of input values intended for use over time.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <param name="startTime"></param>
        /// <param name="timeLength"></param>
        /// <param name="slerp"></param>
        /// <returns>An interpolated value as a quaternion.</returns>
        public static Quaternion QuatLerpNSlerp(Quaternion start, Quaternion target, float startTime, float timeLength, bool slerp)
        {
            float timeLerping = Time.time - startTime;

            float percentageComplete = timeLerping / timeLength;

            if (!slerp)
            {
                return Quaternion.Lerp(start, target, percentageComplete);
            }
            else 
            {
                return Quaternion.Slerp(start, target, percentageComplete);
            }
        }
    }
}


