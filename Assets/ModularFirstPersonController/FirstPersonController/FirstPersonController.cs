// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1
// "Fixed camera jitter during sideways movement by moving camera updates to LateUpdate" || version 1.0.2
// "Major anti-jitter improvements: smoothing, interpolation, and frame-rate independent movement" || version 1.0.3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    #region Camera Movement Variables

    public Camera playerCamera;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Smoothing variables
    [Header("Camera Smoothing")]
    public bool enableCameraSmoothing = true;
    [Range(1f, 20f)]
    public float cameraSmoothing = 10f;
    [Range(0.1f, 1f)]
    public float mouseSmoothingFactor = 0.3f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    // Enhanced anti-jitter variables
    private Vector2 _mouseInput;
    private Vector2 _smoothMouseInput;
    private Vector2 _mouseInputVelocity;
    private float _targetYaw;
    private float _targetPitch;
    private float _currentYaw;
    private float _currentPitch;
    private bool _smoothCameraMovement = true;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;
    private float _currentFOV;
    private float _targetFOV;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputVelocity;

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;
    private bool _jumpPressed = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion
    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);
    [Range(0.1f, 1f)]
    public float headBobSmoothing = 0.5f;

    // Internal Variables
    private Vector3 jointOriginalPos;
    private Vector3 _currentJointPos;
    private Vector3 _targetJointPos;
    private float timer = 0;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        _currentFOV = fov;
        _targetFOV = fov;
        originalScale = transform.localScale;
        
        // Initialize smooth camera values
        _currentYaw = transform.eulerAngles.y;
        _currentPitch = playerCamera.transform.localEulerAngles.x;
        _targetYaw = _currentYaw;
        _targetPitch = _currentPitch;
        
        if (joint != null)
        {
            jointOriginalPos = joint.localPosition;
            _currentJointPos = jointOriginalPos;
            _targetJointPos = jointOriginalPos;
        }

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }

        // Set fixed timestep for consistent physics
        Time.fixedDeltaTime = 1f / 60f; // 60 FPS physics
    }

    void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if(useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if(hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion
    }

    private void Update()
    {
        // Cache and smooth input values to prevent jitter
        Vector2 rawMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector2 rawMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        // Smooth mouse input using SmoothDamp for natural feeling
        _mouseInput = Vector2.SmoothDamp(_mouseInput, rawMouseInput, ref _mouseInputVelocity, mouseSmoothingFactor);
        
        // Smooth movement input
        _movementInput = Vector2.SmoothDamp(_movementInput, rawMovementInput, ref _movementInputVelocity, 0.1f);
        
        _jumpPressed = Input.GetKeyDown(jumpKey);

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toggle zoom
            if(Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                isZoomed = !isZoomed;
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if(holdToZoom && !isSprinting)
            {
                if(Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if(Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Set target FOV based on zoom state
            if(isZoomed)
            {
                _targetFOV = zoomFOV;
            }
            else if(!isZoomed && !isSprinting)
            {
                _targetFOV = fov;
            }
        }

        #endregion

        #region Sprint

        if(enableSprint)
        {
            if(isSprinting)
            {
                isZoomed = false;
                _targetFOV = sprintFOV;

                // Drain sprint remaining while sprinting
                if(!unlimitedSprint)
                {
                    sprintRemaining -= Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        isSprinting = false;
                        isSprintCooldown = true;
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining + Time.deltaTime, 0, sprintDuration);
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if(isSprintCooldown)
            {
                sprintCooldown -= Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar 
            if(useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if(enableJump && _jumpPressed && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if(Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }
            
            if(Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                isCrouched = false;
                Crouch();
            }
            else if(Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                isCrouched = true;
                Crouch();
            }
        }

        #endregion

        CheckGround();

        if(enableHeadBob && joint != null)
        {
            HeadBob();
        }

        // Smooth FOV transitions
        _currentFOV = Mathf.Lerp(_currentFOV, _targetFOV, Time.deltaTime * zoomStepTime);
        playerCamera.fieldOfView = _currentFOV;
    }

    // Enhanced camera movement with multiple smoothing techniques
    private void LateUpdate()
    {
        #region Camera Movement - Enhanced Anti-Jitter Fix

        if(cameraCanMove)
        {
            // Calculate target rotations
            _targetYaw += _mouseInput.x * mouseSensitivity;

            if (!invertCamera)
            {
                _targetPitch -= _mouseInput.y * mouseSensitivity;
            }
            else
            {
                _targetPitch += _mouseInput.y * mouseSensitivity;
            }

            // Clamp pitch
            _targetPitch = Mathf.Clamp(_targetPitch, -maxLookAngle, maxLookAngle);

            if (enableCameraSmoothing)
            {
                // Smooth interpolation to target rotations
                float smoothingSpeed = cameraSmoothing * Time.deltaTime;
                
                _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, smoothingSpeed);
                _currentPitch = Mathf.LerpAngle(_currentPitch, _targetPitch, smoothingSpeed);
                
                // Apply smoothed rotations
                transform.localEulerAngles = new Vector3(0, _currentYaw, 0);
                playerCamera.transform.localEulerAngles = new Vector3(_currentPitch, 0, 0);
            }
            else
            {
                // Direct application (original behavior)
                _currentYaw = _targetYaw;
                _currentPitch = _targetPitch;
                
                transform.localEulerAngles = new Vector3(0, _currentYaw, 0);
                playerCamera.transform.localEulerAngles = new Vector3(_currentPitch, 0, 0);
            }
        }

        #endregion
    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving using smoothed input
            Vector3 targetVelocity = new Vector3(_movementInput.x, 0, _movementInput.y);

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if ((Mathf.Abs(targetVelocity.x) > 0.1f || Mathf.Abs(targetVelocity.z) > 0.1f) && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations while sprint is active
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when velocity change is significant
                if (Mathf.Abs(velocityChange.x) > 0.1f || Mathf.Abs(velocityChange.z) > 0.1f)
                {
                    isSprinting = true;

                    if (isCrouched)
                    {
                        Crouch();
                    }

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.fixedDeltaTime;
                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.fixedDeltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion
    }

    // Enhanced ground check with better stability
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .85f; // Slightly increased for better detection

        // Use multiple raycasts for more stable ground detection
        bool centerHit = Physics.Raycast(origin, direction, out RaycastHit hit, distance);
        Vector3 leftOrigin = origin + transform.right * -0.2f;
        Vector3 rightOrigin = origin + transform.right * 0.2f;
        bool leftHit = Physics.Raycast(leftOrigin, direction, distance);
        bool rightHit = Physics.Raycast(rightOrigin, direction, distance);

        isGrounded = centerHit || leftHit || rightHit;

        if (centerHit)
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if(isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if(isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    // Enhanced HeadBob with smoothing
    private void HeadBob()
    {
        if(isWalking)
        {
            // Calculates HeadBob speed during sprint
            if(isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            
            // Calculate target position with reduced amplitude for smoothness
            _targetJointPos = new Vector3(
                jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z
            );
            
            // Smooth interpolation to target position
            _currentJointPos = Vector3.Lerp(_currentJointPos, _targetJointPos, Time.deltaTime * bobSpeed * headBobSmoothing);
            joint.localPosition = _currentJointPos;
        }
        else
        {
            // Smoothly return to original position when not walking
            timer = 0;
            _targetJointPos = jointOriginalPos;
            _currentJointPos = Vector3.Lerp(_currentJointPos, _targetJointPos, Time.deltaTime * bobSpeed);
            joint.localPosition = _currentJointPos;
        }
    }
}

// Custom Editor with new smoothing options
#if UNITY_EDITOR
    [CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
    public class FirstPersonControllerEditor : Editor
    {
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        GUILayout.Label("version 1.0.3 - Enhanced Anti-Jitter Fix", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera, typeof(Camera), true);
        fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View", "The camera's view angle. Changes the player camera directly."), fpc.fov, fpc.zoomFOV, 179f);
        fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."), fpc.cameraCanMove);

        GUI.enabled = fpc.cameraCanMove;
        fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpc.invertCamera);
        fpc.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."), fpc.mouseSensitivity, .1f, 10f);
        fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40, 90);
        
        // New smoothing options
        EditorGUILayout.Space();
        fpc.enableCameraSmoothing = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Smoothing", "Enables smooth camera movement to reduce jitter."), fpc.enableCameraSmoothing);
        if (fpc.enableCameraSmoothing)
        {
            EditorGUI.indentLevel++;
            fpc.cameraSmoothing = EditorGUILayout.Slider(new GUIContent("Camera Smoothing", "How smooth the camera movement is. Higher = smoother but more delayed."), fpc.cameraSmoothing, 1f, 20f);
            fpc.mouseSmoothingFactor = EditorGUILayout.Slider(new GUIContent("Mouse Input Smoothing", "Smooths mouse input to reduce jitter. Lower = smoother."), fpc.mouseSmoothingFactor, 0.1f, 1f);
            EditorGUI.indentLevel--;
        }
        
        GUI.enabled = true;

        fpc.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

        fpc.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpc.crosshair);

        // Only displays crosshair options if crosshair is enabled
        if(fpc.crosshair) 
        { 
            EditorGUI.indentLevel++; 
            EditorGUILayout.BeginHorizontal(); 
            EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair.")); 
            fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpc.crosshairColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--; 
        }

        #endregion

        //Sets any changes from the prefab
        if(GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
    }
}
#endif