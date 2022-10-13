# 3rd Person Fluid Camera

A free 3rd person fluid motion camera system designed for use with Unity projects.

## Documentation

### Managing Input

*InputActions_FluidCam*
- 3rd Person Fluid Camera is set up to use the new input system. InputActions_FluidCam
is the asset specific input system which is a premade set of controls specific to the
camera asset. Currently it is set up for mouse and keyboard as well as an xbox controller
and can be found within Assets/Input System

  - Action Maps
    - PlayerController (The only action map for 3rd Person Fluid Camera is PlayerController. All control
    actions are are stored within it.)
  - Control Schemes
    - Mouse_Keyboard
    - Gamepad_Generic (Currently only an xbox controller is supported)
  - Actions
    - Camera (Controls camera movement) - Action Type: *Value* - Control Type: *Vector 2*
      - Bindings
        - Delta [Mouse]
        - RightStick 
          - Composite Type: *2D Vector*
          - Mode: *Analog*
          
    - Movement (Controls player movement in demo scenes) - Action Type: *Value* - Control Type: *Vector 2*
      - Bindings
        - WASD
          - Composite Type: *2D Vector*
          - Mode: *Digital Normalized*
        - LeftStick
          - Composite Type: *2D Vector*
          - Mode: *Analog*
    - CameraProximityChange (Controls the proximity of the camera to the subject. Think of this as zooming
    the camera in and out.)
      - Bindings
        - Scroll [Mouse]
        - D-Pad [Gamepad] (Only the up and down values are utilized though the all D-pad values are 
        accessable through this action.)

*InputManager*
- The input manager component uses the Input System InputActions_FluidCam provided 
with the asset. It watches for input from the mouse and keyboard as well as the 
Xbox controller and stores the captured values for use with the CameraController.
the InputManger component needs to be present on the camera using the CameraController 
compoenet for this asset to function. If using the camera prefab provided with the 
asset, one should already be added to it and no setup is necessary. If for some reason
one is not, simply add one to the camera. No configuration is necessary.

### The Camera Component

*boolean* invertCamera
- This setting will invert the cameras vertical control.



[Header("Camera Objects")]
// camOrbitAxis requires an empty GameObject which has the tag CamOrbitAxis to be
// present in the scene. It is recommended that this GameObject is a child of the
// camera which is using the CameraController component in order to keep things
// organzied. However the only requirement is that one be present in the scene.
// camOrbitAxis is used to project the sphereCast environment sensor and provide
// camera position information based on what the sensor hits.
public Transform camOrbitAxis;

// camOrbitAxisTargetPos requires an empty GameObject which has the tag
// CamOrbitAxisTarget to be present in the scene. It is recommended that
// this GameObject is a child of the camera which is using the CameraController
// component however the only requirement is that one be present in the scene.
// camOrbitAxisTarget is used to provide angle information and position
// information to the camOrbitAxis. 
public Transform camOrbitAxisTarget;

[Tooltip("This is the object the camera will attempt to focus on. It needs " +
    "to be manually set by dragging the object intended as the cameras " +
    "focal point from the hierarchy into this field. This would typically be " +
    "set to something like the players head or cockpit of a car. However it " +
    "can be assigned to any transform so long as it is the object intended for " +
    "the camera to focus on.")]
public Transform camFocusSubject;

[Header("Camera Orbit Axis Dampening")]
[Tooltip("Orbit axis rotation dampening for a controller. The default value is 150.")]
public float camAxisRotDamping_C = 200f;
[Tooltip("Orbit axis rotation dampening for a mouse. The default value is 150.")]
public float camAxisRotDamping_M = 1200f;
[Tooltip("In Unity, mouse input is only one of three values, -1, 0 or 1. Additionally " +
    "Unity only detects movement if an entire pixel of movement is detected. This means " +
    "that sub-pixel movement is not hard coded in. Raw Mouse Input Damping is intended " +
    "to smooth out the jitter from this effect. In short, this value helps smooth out " +
    "mouse movements. It is highly recommended to leave this set to 0.05 (the default " +
    "value) however it can be adjusted for the projects specific needs. This value is " +
    "clamped between 0 and 1.")]
[Range(0.0f, 1.0f)]
public float rawMouseInputDamping = 0.05f;

[Header("Camera Orbit Axis Angle Clamps")]
[Tooltip("The tolerance in degrees with witch Cam Axis Lower and Upper Angle Clamps " +
    "can be to one another. For example if this number is set to 5 then the camera " +
    "clamps will never be closer than 5 degrees apart. This number can not be lower " +
    "than 0 or higher than 10.")]
public float camAxisAngleClampTolerance = 10f;
[Tooltip("The degree angle the camera can move vertically downward before stoping. " +
    "This number can not be below -70 or above 80. This number will observe " +
    "Cam Axis Angle Clamp Tolerance as the difference between itself and " +
    "Cam Axis Upper Angle Clamp. Meaning this number will never be closer to " +
    "Cam Axis Upper Angle Clamp than the tolerance.")]
public float camAxisLowerAngleClamp = 80f;
[Tooltip("The degree angle the camera can move vertically upward before stoping. " +
    "This number can not be below -70 or above 80. This number will observe " +
    "Cam Axis Angle Clamp Tolerance as the difference between itself and " +
    "Cam Axis Lower Angle Clamp. Meaning this number will never be closer to " +
    "Cam Axis Lower Angle Clamp than the tolerance.")]
public float camAxisUpperAngleClamp = 80f;
private float camOrbitAxisVerticalAngle;

// This is the desired position for the camera and is used as the target
// destination for position lerping.
private Vector3 camTargetPosition;
// This is the desired rotation for the camera and is used as the target
// destination for rotation lerping.
private Quaternion camTargetRotation;

[Header("Camera Motion Settings")]
[Tooltip("This setting is for the camera itself, not the orbit axis. This is the " +
    "position dampening used to speed up or slow down the cameras ability to " +
    "relocate to its desired position. The default value for this is 8. This can " +
    "be tweaked until getting the desired result. A lower number will cause it to " +
    "move slower while a higher number will make it move faster. If you find the " +
    "camera is trailing to far behind the target, try raising this number to help " +
    "it catch up.")]
public float camPosDamping = 8f;
[Tooltip("This setting is for the camera itself, not the orbit axis. This is the " +
    "rotation dampening used to speed up or slow down the cameras ability to rotate " +
    "in order to focus on the target. The default value for this number is 8 however " +
    "raising it will speed up the cameras rotation and lowering it will slow it down. " +
    "It is safe to tweak this value until getting the desired result.")]
public float camRotDamping = 8f;

[Header("Camera Proximity Settings")]
[Tooltip("How far the camera will move when the player changes the proximity to " +
    "its subject. The default value is 1 and can not go below zero.")]
public float camProximityStepCount = 1f;

[Header("Sensor Settings")]
[Tooltip("Initially this mask is only set to the default layer, however the " +
    "cameras environment sensor will collide with any layer added to this mask " +
    "making the camera adjust its position accordingly. Make sure this mask " +
    "does not include the layer of the object it orbits (i.e. the player layer, " +
    "etc.) or it may collide with it and cause unwanted behaviour.")]
public LayerMask sensorHitLayerMask = 1 << 0;
//This must be globally scoped because it is used in multiple functions.
private RaycastHit sensorHit;
private bool sensorDidHit;
[Tooltip("The size of the probe used to check for the environment. It is important " +
    "to note this value is the radius. If for example the value here is 0.5, the " +
    "diameter of the probe will be 1. Because of this, always use a number exactly " +
    "half the intended size.")]
public float camSensorCastRadius = 0.5f;
[Tooltip("The distance the probe will check for collisions with the environment.")]
public float camSensorProbeDistance = 10f;
[Tooltip("The minimum distance the probe can be to the cameras subject. This is " +
    "mostly useful when setting the cameras proximity at runtime to keep it from " +
    "going closer than intended.")]
public float camSensorProbeDistanceMin = 1f;
[Tooltip("The maximum distance the probe can be to the cameras subject. This is " +
    "mostly useful when setting the cameras proximity at runtime to keep it from " +
    "going farther than intended.")]
public float camSensorProbeDistanceMax = 10f;
[Tooltip("This is the cameras vertical position offset from the end point of the " +
    "sensor ray. This keeps the camera from clipping into geometry and may require " +
    "adjustment for the projects specific needs.")]
public float camHeightOffset = 1f;
[Tooltip("The imaginary size of the camera. This helps the sensors more accurately " +
    "detect the environment. The default is 0.5 however this can be changed to meet " +
    "a projects specific needs.")]
public float simulatedCamSize = 0.5f;

[Header("Debug Settings")]
public bool enableDebug = false;
public Color debugProbeHitColor = Color.red;
public Color debugProbeMissColor = Color.green;




