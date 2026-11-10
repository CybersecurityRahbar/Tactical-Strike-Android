using UnityEngine;
using UnityEngine.EventSystems;

namespace TacticalStrike
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FpsPlayerController : MonoBehaviour
    {
        public Camera PlayerCamera { get; private set; }
        public WeaponController Weapon { get; private set; }

        private CharacterController character;
        private MobileHud hud;
        private AudioSource footstepSource;
        private AudioClip[] footstepClips;
        private float footstepTimer;
        private Vector2 mobileMove;
        private float pitch;
        private float verticalVelocity;
        private float cameraBob;
        private const float StandingCameraHeight = 1.62f;
        private bool sprintHeld;
        private bool crouchHeld;
        private bool jumpRequested;
        private bool aimEnabled;
        private float sensitivity = 0.105f;

        public static FpsPlayerController Create(Vector3 position, Transform parent)
        {
            GameObject player = new GameObject("Player - first person");
            player.transform.SetParent(parent, false);
            player.transform.position = position;
            // Spawn on the attacker's southern side, facing north toward the distant defenders.
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            return player.AddComponent<FpsPlayerController>();
        }

        private void Awake()
        {
            character = GetComponent<CharacterController>();
            character.height = 1.84f;
            character.radius = 0.34f;
            character.center = new Vector3(0f, 0.92f, 0f);
            character.slopeLimit = 52f;
            character.stepOffset = 0.32f;
            character.skinWidth = 0.035f;
            character.minMoveDistance = 0f;

            GameObject cameraObject = new GameObject("PlayerCamera");
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
            footstepSource.volume = 0.58f;
            footstepSource.playOnAwake = false;
            footstepSource.dopplerLevel = 0f;
            footstepClips = Resources.LoadAll<AudioClip>("TacticalStrike/Imported/Audio/Footsteps");

            Weapon = gameObject.AddComponent<WeaponController>();
            Weapon.Initialize(this, PlayerCamera);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void AttachHud(MobileHud mobileHud) { hud = mobileHud; }
        public void SetMoveVector(Vector2 value) { mobileMove = Vector2.ClampMagnitude(value, 1f); }

        public void AddLookDelta(Vector2 delta)
        {
            transform.Rotate(Vector3.up, delta.x * sensitivity, Space.World);
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -82f, 82f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public void SetSprint(bool held) { sprintHeld = held; }
        public void SetCrouch(bool held) { crouchHeld = held; }
        public void RequestJump() { jumpRequested = true; }

        public void SetAim(bool enabled)
        {
            aimEnabled = enabled;
            if (Weapon != null) Weapon.SetAiming(enabled);
        }

        public void AdjustSensitivity(float amount)
        {
            sensitivity = Mathf.Clamp(sensitivity + amount, 0.045f, 0.2f);
            MobileHud.Active?.ShowMessage("LOOK SENSITIVITY: " + sensitivity.ToString("0.000"));
        }

        private void Update()
        {
            if (PlayerCamera == null || character == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            bool pointerOverUi = EventSystem.current != null &&
                                 EventSystem.current.IsPointerOverGameObject();
            bool justLockedCursor = false;

            // Browsers require a real user gesture before pointer lock is allowed.
            // Do not capture the cursor when the user is clicking HUD buttons.
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
            }

            Vector2 keyboardMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (keyboardMove.sqrMagnitude > 0.001f)
                mobileMove = Vector2.ClampMagnitude(keyboardMove, 1f);
            else if (!Application.isMobilePlatform)
                mobileMove = Vector2.zero;

            if (Input.GetKey(KeyCode.LeftShift)) sprintHeld = true;
            else if (!Application.isMobilePlatform) sprintHeld = false;
            if (Input.GetKeyDown(KeyCode.Space)) jumpRequested = true;
            if (Input.GetKey(KeyCode.C)) crouchHeld = true;
            else if (!Application.isMobilePlatform) crouchHeld = false;

            bool canUseCombatMouse = Cursor.lockState == CursorLockMode.Locked && !justLockedCursor;
            if (canUseCombatMouse && Input.GetMouseButton(0)) Weapon.SetFireHeld(true);
            else if (!Application.isMobilePlatform) Weapon.SetFireHeld(false);
            if (canUseCombatMouse && Input.GetMouseButtonDown(1)) SetAim(!aimEnabled);
            if (Input.GetKeyDown(KeyCode.R)) Weapon.Reload();
            if (Input.GetKeyDown(KeyCode.T)) Weapon.CycleAmmo();
#endif

            float targetCapsuleHeight = crouchHeld ? 1.22f : 1.84f;
            character.height = Mathf.Lerp(character.height, targetCapsuleHeight, dt * 12f);
            character.center = new Vector3(0f, character.height * 0.5f, 0f);

            Vector3 input = Vector3.ClampMagnitude(new Vector3(mobileMove.x, 0f, mobileMove.y), 1f);
            Vector3 moveDirection = transform.TransformDirection(input);
            float speed = crouchHeld ? 2.55f : (sprintHeld ? 7.2f : 5.1f);

            if (character.isGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -1.6f;
                if (jumpRequested && !crouchHeld) verticalVelocity = 4.4f;
            }
            else
            {
                verticalVelocity -= 19.6f * dt;
            }
            jumpRequested = false;
            character.Move((moveDirection * speed + Vector3.up * verticalVelocity) * dt);

            float targetHeight = crouchHeld ? 1.08f : StandingCameraHeight;
            bool moving = input.sqrMagnitude > 0.04f && character.isGrounded;
            bool running = moving && sprintHeld && !crouchHeld;
            cameraBob = moving
                ? cameraBob + dt * (running ? 13f : 9.2f)
                : Mathf.Lerp(cameraBob, 0f, dt * 3f);
            float bob = moving ? Mathf.Abs(Mathf.Sin(cameraBob)) * (running ? 0.028f : 0.017f) : 0f;
            Vector3 cameraPosition = PlayerCamera.transform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetHeight + bob, dt * 8f);
            PlayerCamera.transform.localPosition = cameraPosition;
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, aimEnabled ? 57f : 78f, dt * 9f);

            UpdateFootsteps(dt, moving, running);
            if (Weapon != null) Weapon.SetMovementState(moving, running);
        }

        private void UpdateFootsteps(float deltaTime, bool moving, bool running)
        {
            if (!moving || footstepClips == null || footstepClips.Length == 0)
            {
                footstepTimer = 0f;
                return;
            }

            footstepTimer += deltaTime;
            float interval = running ? 0.34f : (crouchHeld ? 0.52f : 0.46f);
            if (footstepTimer < interval) return;
            footstepTimer = 0f;
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.pitch = Random.Range(0.94f, 1.06f);
            footstepSource.PlayOneShot(clip);
        }
    }
}
