using UnityEngine;

namespace TacticalStrike
{
    public sealed class WeaponController : MonoBehaviour
    {
        private const float MuzzleVelocity = 900f;
        private const float DragCoefficient = 0.0000075f;
        private const float MaxRange = 650f;
        private FpsPlayerController player;
        private Camera playerCamera;
        private Transform viewModel;
        private Animation legacyAnimation;
        private AudioSource audioSource;
        private AudioClip[] fireClips;
        private AudioClip[] footstepClips;
        private float fireCooldown;
        private float reloadTimer;
        private bool reloadActive;
        private bool fireHeld;
        private bool aiming;
        private bool moving;
        private bool running;
        private int magazine = 30;
        private int reserve = 120;
        private int shotCount;
        private int ammoMode;
        private string currentMotion = "";
        private float motionLockTimer;

        public int Magazine => magazine;
        public int Reserve => reserve;
        public string AmmoMode => ammoMode == 0 ? "FMJ" : (ammoMode == 1 ? "TRACER" : "AP");

        public void Initialize(FpsPlayerController owner, Camera camera)
        {
            player = owner;
            playerCamera = camera;
            audioSource = camera.gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.82f;
            audioSource.playOnAwake = false;
            audioSource.dopplerLevel = 0f;
            fireClips = Resources.LoadAll<AudioClip>("TacticalStrike/Imported/Audio/Firearms");
            BuildViewModel();
            UpdateHud();
        }

        private void BuildViewModel()
        {
            GameObject modelPrefab = Resources.Load<GameObject>("TacticalStrike/Imported/AK74M");
            GameObject model;
            if (modelPrefab != null)
            {
                model = Instantiate(modelPrefab, playerCamera.transform);
                model.name = "AK-74M animated first-person viewmodel";
                model.transform.localPosition = new Vector3(0.18f, -0.29f, 0.49f);
                model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                model.transform.localScale = Vector3.one * 0.72f;
                legacyAnimation = model.GetComponent<Animation>();
                if (legacyAnimation == null) legacyAnimation = model.AddComponent<Animation>();
                AnimationClip[] clips = Resources.LoadAll<AnimationClip>("TacticalStrike/Imported");
                foreach (AnimationClip clip in clips)
                {
                    if (clip == null) continue;
                    try { legacyAnimation.AddClip(clip, clip.name); }
                    catch (System.Exception) { }
                }
                PlayMotion("idle");
            }
            else
            {
                model = new GameObject("AK-74M procedural fallback viewmodel");
                model.transform.SetParent(playerCamera.transform, false);
                model.transform.localPosition = new Vector3(0.18f, -0.27f, 0.48f);
                BuildFallbackRifle(model.transform);
            }
            viewModel = model.transform;
        }

        private void BuildFallbackRifle(Transform root)
        {
            Material receiver = MakeWeaponMaterial(new Color(0.105f, 0.12f, 0.105f));
            Material wood = MakeWeaponMaterial(new Color(0.30f, 0.20f, 0.12f));
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0f, 0.16f), new Vector3(0.15f, 0.15f, 0.43f), receiver, Vector3.zero);
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0.01f, -0.20f), new Vector3(0.14f, 0.12f, 0.30f), wood, Vector3.zero);
            Part(root, PrimitiveType.Cube, new Vector3(0f, -0.02f, 0.40f), new Vector3(0.10f, 0.075f, 0.48f), receiver, Vector3.zero);
            Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.0f, 0.72f), new Vector3(0.022f, 0.24f, 0.022f), receiver, new Vector3(90f, 0f, 0f));
            Part(root, PrimitiveType.Cube, new Vector3(0.015f, -0.19f, 0.18f), new Vector3(0.10f, 0.27f, 0.16f), receiver, new Vector3(-12f, 0f, 0f));
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0.115f, 0.11f), new Vector3(0.035f, 0.055f, 0.35f), darkMaterial(), Vector3.zero);
        }

        private Material darkMaterial() => MakeWeaponMaterial(new Color(0.08f, 0.09f, 0.08f));

        private static Material MakeWeaponMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader) { color = color, name = "AK-74M component" };
        }

        private static void Part(Transform parent, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Vector3 rotation)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localRotation = Quaternion.Euler(rotation);
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider col = part.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        public void SetFireHeld(bool held) { fireHeld = held; }
        public void SetAiming(bool value) { aiming = value; }
        public void SetMovementState(bool isMoving, bool isRunning)
        {
            moving = isMoving;
            running = isRunning;
            if (!reloadActive && motionLockTimer <= 0f)
                PlayMotion(running ? "run" : (moving ? "walk" : "idle"));
        }

        public void Reload()
        {
            if (reloadActive || magazine >= 30 || reserve <= 0) return;
            reloadActive = true;
            reloadTimer = 2.25f;
            motionLockTimer = reloadTimer;
            currentMotion = "";
            PlayMotion("reload");
            UpdateHud();
        }

        public void CycleAmmo()
        {
            ammoMode = (ammoMode + 1) % 3;
            UpdateHud();
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            fireCooldown = Mathf.Max(0f, fireCooldown - dt);
            if (motionLockTimer > 0f)
                motionLockTimer = Mathf.Max(0f, motionLockTimer - dt);

            if (reloadActive)
            {
                reloadTimer -= dt;
                if (reloadTimer <= 0f)
                {
                    int transfer = Mathf.Min(30 - magazine, reserve);
                    magazine += transfer;
                    reserve -= transfer;
                    reloadActive = false;
                    motionLockTimer = 0f;
                    currentMotion = "";
                    PlayMotion(running ? "run" : (moving ? "walk" : "idle"));
                    UpdateHud();
                }
            }
            if (fireHeld && fireCooldown <= 0f && !reloadActive)
                Fire();
        }

        private void Fire()
        {
            if (magazine <= 0)
            {
                fireCooldown = 0.25f;
                MobileHud.Active?.ShowMessage("MAGAZINE EMPTY - RELOAD");
                return;
            }

            magazine--;
            shotCount++;
            fireCooldown = 0.10f;
            Vector3 muzzle = playerCamera.transform.TransformPoint(new Vector3(0.18f, -0.17f, 0.83f));
            Vector3 direction = playerCamera.transform.forward;
            BallisticProjectile projectile = new GameObject("5.45mm projectile").AddComponent<BallisticProjectile>();
            projectile.Initialize(muzzle, direction * MuzzleVelocity, player.transform, ammoMode == 1,
                ammoMode == 2 ? 42f : 38f, ammoMode == 2 ? 0.48f : 0.34f,
                DragCoefficient, MaxRange);
            if (fireClips != null && fireClips.Length > 0)
                audioSource.PlayOneShot(fireClips[Random.Range(0, fireClips.Length)], aiming ? 0.68f : 0.82f);
            SpawnMuzzleFlash(muzzle, direction);
            motionLockTimer = 0.30f;
            currentMotion = "";
            PlayMotion("shot");
            MobileHud.Active?.SetWeaponInfo(magazine, reserve, AmmoMode);
        }

        private void SpawnMuzzleFlash(Vector3 position, Vector3 direction)
        {
            GameObject flash = new GameObject("Muzzle flash");
            flash.transform.position = position;
            flash.transform.rotation = Quaternion.LookRotation(direction);
            Light glow = flash.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 5.5f;
            glow.intensity = 2.6f;
            glow.color = new Color(1f, 0.43f, 0.13f);
            Destroy(flash, 0.055f);
        }

        private void PlayMotion(string requested)
        {
            if (legacyAnimation == null || currentMotion == requested) return;
            string token = requested.ToLowerInvariant();
            foreach (AnimationState state in legacyAnimation)
            {
                if (state == null) continue;
                string name = state.name.ToLowerInvariant();
                if (name.Contains(token))
                {
                    legacyAnimation.CrossFade(state.name, 0.12f);
                    currentMotion = requested;
                    return;
                }
            }
            currentMotion = requested;
        }

        private void UpdateHud()
        {
            MobileHud.Active?.SetWeaponInfo(magazine, reserve, AmmoMode);
        }

        private void OnDisable()
        {
            fireHeld = false;
        }
    }
}
