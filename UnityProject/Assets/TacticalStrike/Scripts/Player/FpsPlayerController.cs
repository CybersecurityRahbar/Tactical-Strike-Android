using UnityEngine;
using UnityEngine.EventSystems;

namespace TacticalStrike
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FpsPlayerController : MonoBehaviour
    {
        public Camera PlayerCamera { get; private set; }
        public WeaponController Weapon { get; private set; }
        public float Stamina => stamina;

        private CharacterController character;
        private Vector2 mobileMove;
        private AudioSource footstepSource;
        private AudioClip[] footstepClips;
        private float footstepTimer;
        private float pitch;
        private float freeLookYaw;
        private float verticalVelocity;
        private float cameraBob;
        private float sensitivity = 0.105f;
        private float stamina = 100f;
        private float staminaRecoveryDelay;
        private Vector3 planarVelocity;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool proneHeld;
        private bool jumpRequested;
        private bool aimEnabled;
        private bool isFocusing;

        private const float StandingCameraHeight = 1.62f;

        public static FpsPlayerController Create(Vector3 position, Transform parent)
        {
            GameObject player = new GameObject("Player - Northwind Valley infantry");
            player.transform.SetParent(parent, false);
            player.transform.position = position;
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            return player.AddComponent<FpsPlayerController>();
        }

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            character.height = 1.84f;
            character.radius = 0.34f;
            character.center = new Vector3(0f, 0.92f, 0f);
            character.slopeLimit = 49f;
            character.stepOffset = 0.30f;
            character.skinWidth = 0.035f;
            character.minMoveDistance = 0f;

            GameObject cameraObject = new GameObject("PlayerCamera - infantry eye");
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, StandingCameraHeight, 0f);
            PlayerCamera = cameraObject.AddComponent<Camera>();
            PlayerCamera.fieldOfView = 78f;
            PlayerCamera.nearClipPlane = 0.055f;
            PlayerCamera.farClipPlane = 1800f;
            PlayerCamera.allowHDR = false;
            PlayerCamera.allowMSAA = true;
            PlayerCamera.depth = 0f;

            cameraObject.AddComponent<AudioListener>();
            footstepSource = cameraObject.AddComponent<AudioSource>();
            footstepSource.spatialBlend = 0.38f;
            footstepSource.volume = 0.55f;
            footstepSource.playOnAwake = false;
            footstepSource.dopplerLevel = 0f;
            footstepClips = Resources.LoadAll<AudioClip>("TacticalStrike/Imported/Audio/Footsteps");

            Weapon = gameObject.AddComponent<WeaponController>();
            Weapon.Initialize(this, PlayerCamera);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void SetMoveVector(Vector2 value) { mobileMove = Vector2.ClampMagnitude(value, 1f); }

        public void AddLookDelta(Vector2 delta)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
                freeLookYaw = Mathf.Clamp(freeLookYaw + delta.x * sensitivity, -85f, 85f);
            else
            {
                transform.Rotate(Vector3.up, delta.x * sensitivity, Space.World);
                freeLookYaw = Mathf.MoveTowards(freeLookYaw, 0f, 0.22f);
            }
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -82f, 82f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, freeLookYaw, 0f);
        }

        public void SetSprint(bool held) { sprintHeld = held; }
        public void SetCrouch(bool held)
        {
            if (!proneHeld) crouchHeld = held;
        }
        public void RequestJump() { jumpRequested = true; }

        public void ToggleProne()
        {
            proneHeld = !proneHeld;
            if (proneHeld) crouchHeld = false;
        }

        public void SetAim(bool enabled)
        {
            aimEnabled = enabled;
            if (!enabled) isFocusing = false;
            if (Weapon != null) Weapon.SetAiming(enabled);
        }

        public void AdjustSensitivity(float amount)
        {
            sensitivity = Mathf.Clamp(sensitivity + amount, 0.045f, 0.2f);
            MobileHud.Active?.ShowMessage("LOOK SENSITIVITY: " + sensitivity.ToString("0.000"));
        }

        /// <summary>Recoil changes the camera sight picture and weapon spring together.</summary>
        public void ApplyWeaponRecoil(float verticalDegrees, float horizontalDegrees)
        {
            pitch = Mathf.Clamp(pitch - verticalDegrees, -82f, 82f);
            transform.Rotate(Vector3.up, horizontalDegrees, Space.World);
            PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, freeLookYaw, 0f);
        }

        private void Update()
        {
            if (PlayerCamera == null || character == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            bool pointerOverUi = EventSystem.current != null &&
                                 EventSystem.current.IsPointerOverGameObject();
            bool justLockedCursor = false;

            if (Input.GetMouseButtonDown(0) && !pointerOverUi &&
                Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                justLockedCursor = true;
            }
            if (Cursor.lockState == CursorLockMode.Locked)
                AddLookDelta(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 2.4f);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Weapon.SetFireHeld(false);
                SetAim(false);
                freeLookYaw = 0f;
            }

            Vector2 keyboardMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (keyboardMove.sqrMagnitude > 0.001f)
                mobileMove = Vector2.ClampMagnitude(keyboardMove, 1f);
            else if (!Application.isMobilePlatform)
                mobileMove = Vector2.zero;

            if (Input.GetKey(KeyCode.LeftShift)) sprintHeld = true;
            else if (!Application.isMobilePlatform) sprintHeld = false;
            if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
            if (Input.GetKeyDown(KeyCode.Z)) ToggleProne();
            if (Input.GetKey(KeyCode.C) && !proneHeld) crouchHeld = true;
            else if (!Application.isMobilePlatform) crouchHeld = false;

            bool canUseCombatMouse = Cursor.lockState == CursorLockMode.Locked && !justLockedCursor;
            if (canUseCombatMouse && Input.GetMouseButton(0)) Weapon.SetFireHeld(true);
            else if (!Application.isMobilePlatform) Weapon.SetFireHeld(false);
            if (canUseCombatMouse && Input.GetMouseButtonDown(1)) SetAim(!aimEnabled);
            if (Input.GetKeyDown(KeyCode.R)) Weapon.Reload();
            if (Input.GetKeyDown(KeyCode.T)) Weapon.CycleAmmo();

            if (!Input.GetKey(KeyCode.LeftAlt))
            {
                freeLookYaw = Mathf.MoveTowards(freeLookYaw, 0f, 90f * dt);
                PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, freeLookYaw, 0f);
            }
#endif

            Vector2 input = Vector2.ClampMagnitude(mobileMove, 1f);
            bool hasMoveInput = input.sqrMagnitude > 0.035f;
            bool grounded = character.isGrounded;

            bool sprinting = sprintHeld && hasMoveInput && !crouchHeld && !proneHeld &&
                             stamina > 7f && !aimEnabled;
            isFocusing = aimEnabled && !hasMoveInput && sprintHeld && stamina > 18f;

            UpdateStamina(dt, sprinting, isFocusing, grounded);

            float targetCapsuleHeight = proneHeld ? 0.78f : (crouchHeld ? 1.22f : 1.84f);
            character.height = Mathf.Lerp(character.height, targetCapsuleHeight, dt * 10f);
            character.center = new Vector3(0f, character.height * 0.5f, 0f);

            Vector3 localInput = new Vector3(input.x, 0f, input.y);
            Vector3 moveDirection = transform.TransformDirection(localInput);
            float speed = proneHeld ? 1.05f : (crouchHeld ? 2.20f : (sprinting ? 5.85f : 3.85f));
            if (aimEnabled) speed *= 0.82f;
            if (sprinting && stamina < 28f)
                speed *= Mathf.Lerp(0.72f, 1f, stamina / 28f);

            Vector3 targetPlanarVelocity = moveDirection * speed;
            float acceleration = !grounded ? 4.5f : (hasMoveInput ? (sprinting ? 10f : 15f) : 21f);
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetPlanarVelocity, acceleration * dt);

            if (grounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -1.8f;
                if (jumpRequested && !crouchHeld && !proneHeld && stamina >= 9f)
                {
                    verticalVelocity = 4.15f;
                    stamina = Mathf.Max(0f, stamina - 9f);
                    staminaRecoveryDelay = 1.2f;
                }
            }
            else
            {
                verticalVelocity -= 19.0f * dt;
            }
            jumpRequested = false;
            character.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);

            bool moving = hasMoveInput && grounded && planarVelocity.sqrMagnitude > 0.20f;
            bool running = sprinting && moving;
            float targetEyeHeight = proneHeld ? 0.58f : (crouchHeld ? 1.06f : StandingCameraHeight);
            float bobScale = proneHeld ? 0.004f : (aimEnabled ? 0.006f : (running ? 0.034f : 0.019f));
            cameraBob = moving
                ? cameraBob + dt * (running ? 11f : (proneHeld ? 4f : 8f))
                : Mathf.Lerp(cameraBob, 0f, dt * 3.5f);
            float bob = moving ? Mathf.Sin(cameraBob) * bobScale : 0f;

            Vector3 cameraPosition = PlayerCamera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetEyeHeight + bob, dt * 9f);
            PlayerCamera.transform.localPosition = cameraPosition;

            float targetFov = isFocusing ? 48f : (aimEnabled ? 55f : 78f);
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, targetFov, dt * 8.5f);

            UpdateFootsteps(dt, moving, running);
            if (Weapon != null)
            {
                Weapon.SetMovementState(moving, running);
                Weapon.SetHandlingState(stamina / 100f, moving, running, crouchHeld, proneHeld);
            }
            MobileHud.Active?.SetStamina(stamina);
        }

        private void UpdateStamina(float deltaTime, bool sprinting, bool focusing, bool grounded)
        {
            if (sprinting)
            {
                stamina = Mathf.Max(0f, stamina - 19.5f * deltaTime);
                staminaRecoveryDelay = 1.05f;
            }
            else if (focusing)
            {
                stamina = Mathf.Max(0f, stamina - 5.0f * deltaTime);
                staminaRecoveryDelay = 0.35f;
            }
            else
            {
                staminaRecoveryDelay = Mathf.Max(0f, staminaRecoveryDelay - deltaTime);
                if (staminaRecoveryDelay <= 0f)
                    stamina = Mathf.Min(100f, stamina + (grounded ? 13.5f : 8f) * deltaTime);
            }
        }

        private void UpdateFootsteps(float deltaTime, bool moving, bool running)
        {
            if (!moving || footstepClips == null || footstepClips.Length == 0)
            {
                footstepTimer = 0f;
                return;
            }

            footstepTimer += deltaTime;
            float interval = proneHeld ? 0.72f : (running ? 0.40f : (crouchHeld ? 0.64f : 0.52f));
            if (footstepTimer < interval) return;
            footstepTimer = 0f;
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = Random.Range(0.94f, 1.06f);
            footstepSource.PlayOneShot(clip);
        }
    }
}
