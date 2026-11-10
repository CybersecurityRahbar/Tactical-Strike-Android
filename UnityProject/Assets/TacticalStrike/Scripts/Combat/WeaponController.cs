using UnityEngine;
using UnityEngine.Rendering;

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
        private Transform sightAnchor;
        private Transform muzzleAnchor;
        private float swayClock;
        private float staminaFactor = 1f;
        private bool crouched;
        private bool prone;
        private int shotIndex;
        private Texture2D armsAlbedo;
        private Texture2D rifleAlbedo;
        private Texture2D armsNormal;
        private Texture2D rifleNormal;
        private Vector3 viewModelRestPosition = new Vector3(0.27f, -0.32f, 0.60f);
        private Vector3 viewModelRootOffset;
        private float recoilOffset;
        private float fireCooldown;
        private float reloadTimer;
        private bool reloadActive;
        private bool fireHeld;
        private bool aiming;
        private bool moving;
        private bool running;
        private int magazine = 30;
        private int reserve = 120;
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
            audioSource.ignoreListenerPause = true;
            fireClips = Resources.LoadAll<AudioClip>("TacticalStrike/Imported/Audio/Firearms");
            BuildViewModel();
            UpdateHud();
        }

        private void BuildViewModel()
        {
            armsAlbedo = Resources.Load<Texture2D>("TacticalStrike/Imported/Textures/arms_baseColor");
            rifleAlbedo = Resources.Load<Texture2D>("TacticalStrike/Imported/Textures/ak74m_baseColor");
            armsNormal = Resources.Load<Texture2D>("TacticalStrike/Imported/Textures/arms_normal");
            rifleNormal = Resources.Load<Texture2D>("TacticalStrike/Imported/Textures/ak74m_normal");

            GameObject modelPrefab = Resources.Load<GameObject>("TacticalStrike/Imported/AK74M");
            GameObject model;
            if (modelPrefab != null)
            {
                model = Instantiate(modelPrefab, playerCamera.transform);
                model.name = "AK-74M animated first-person viewmodel";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                model.transform.localScale = Vector3.one;

                // Fit the imported full arms-and-rifle rig to its renderer bounds.
                // A fixed 0.72 scale ignored source dimensions and made the rig fill the view.
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                viewModelRootOffset = Vector3.zero;
                if (renderers.Length > 0)
                {
                    Bounds combined = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                        combined.Encapsulate(renderers[i].bounds);

                    // Center the imported rig's visible bounds as well as fitting its
                    // length. Imported FBX pivots are not guaranteed to sit at the rig centre.
                    Vector3 centerInModelSpace = model.transform.InverseTransformPoint(combined.center);
                    float largestDimension = Mathf.Max(combined.size.x,
                        Mathf.Max(combined.size.y, combined.size.z));
                    float fitScale = largestDimension > 0.001f ? 0.84f / largestDimension : 0.32f;
                    fitScale = Mathf.Clamp(fitScale, 0.16f, 0.52f);
                    model.transform.localScale = Vector3.one * fitScale;
                    viewModelRootOffset = -(model.transform.localRotation * (centerInModelSpace * fitScale));
                }

                model.transform.localPosition = viewModelRestPosition + viewModelRootOffset;
                ApplyViewModelMaterials(model);

                foreach (Renderer renderer in renderers)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }

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
                model.transform.localPosition = viewModelRestPosition;
                BuildFallbackRifle(model.transform);
            }
            viewModel = model.transform;
            sightAnchor = FindWeaponAnchor(viewModel, true);
            muzzleAnchor = FindWeaponAnchor(viewModel, false);
            if (sightAnchor == null)
                Debug.LogWarning("AK-74M rig has no named rear-sight/optic anchor; ADS is using the calibrated fallback offset.");
            if (muzzleAnchor == null)
                Debug.LogWarning("AK-74M rig has no named muzzle anchor; projectile origin is using the camera muzzle reference.");
        }

        private static Transform FindWeaponAnchor(Transform root, bool sight)
        {
            Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
            string[] preferred = sight
                ? new[] { "rear_sight", "rear sight", "ironsight", "iron_sight", "optic", "sight" }
                : new[] { "muzzle", "barrel_end", "barrel end", "muzzle_tip", "barrel_tip" };
            for (int p = 0; p < preferred.Length; p++)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    string name = nodes[i].name.ToLowerInvariant().Replace('-', '_');
                    if (name.Contains(preferred[p])) return nodes[i];
                }
            }
            return null;
        }

        private void ApplyViewModelMaterials(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null) continue;

                    string label = (renderer.name + " " + material.name).ToLowerInvariant();
                    bool isArms = label.Contains("arm") || label.Contains("hand") ||
                                  label.Contains("glove") || label.Contains("skin");
                    bool isRifle = label.Contains("ak") || label.Contains("rifle") ||
                                   label.Contains("weapon") || label.Contains("mag");
                    if (!isArms && !isRifle && materials.Length > 1)
                        isArms = i == 0;

                    Texture2D albedo = isArms ? armsAlbedo : rifleAlbedo;
                    Texture2D normal = isArms ? armsNormal : rifleNormal;
                    if (urpLit != null && material.shader != urpLit)
                        material.shader = urpLit;

                    if (albedo != null)
                    {
                        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
                        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
                        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
                        if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
                    }
                    if (normal != null && material.HasProperty("_BumpMap"))
                    {
                        material.SetTexture("_BumpMap", normal);
                        material.EnableKeyword("_NORMALMAP");
                    }
                    if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.24f);
                    if (material.HasProperty("_Metallic"))
                        material.SetFloat("_Metallic", isRifle ? 0.32f : 0.02f);
                }
                renderer.materials = materials;
            }
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

        public void SetHandlingState(float stamina01, bool isMoving, bool isRunning,
            bool isCrouched, bool isProne)
        {
            staminaFactor = Mathf.Clamp01(stamina01);
            moving = isMoving;
            running = isRunning;
            crouched = isCrouched;
            prone = isProne;
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
            recoilOffset = Mathf.MoveTowards(recoilOffset, 0f, dt * (aiming ? 5.4f : 7.0f));
            if (viewModel != null)
            {
                swayClock += dt * (running ? 10.5f : (moving ? 6.4f : 1.45f));
                float fatigue = 1.35f - staminaFactor * 0.48f;
                float stanceScale = prone ? 0.32f : (crouched ? 0.62f : 1f);
                float movementScale = running ? 1.85f : (moving ? 1.05f : 0.48f);
                float swayScale = fatigue * stanceScale * movementScale * (aiming ? 0.34f : 1f);
                float swayX = Mathf.Sin(swayClock * 0.91f) * 0.020f * swayScale;
                float swayY = Mathf.Sin(swayClock * 1.83f + 0.8f) * 0.014f * swayScale;
                float roll = Mathf.Sin(swayClock * 0.73f) * 1.05f * swayScale;
                Quaternion targetRotation = Quaternion.Euler(
                    -recoilOffset * 2.25f + swayY * 42f,
                    180f + swayX * 38f,
                    roll);
                viewModel.localRotation = Quaternion.Slerp(viewModel.localRotation, targetRotation, dt * 13f);

                Vector3 swayOffset = new Vector3(swayX, swayY, 0f);
                Vector3 targetPosition = viewModelRestPosition + viewModelRootOffset + swayOffset;
                if (aiming && sightAnchor != null)
                {
                    // Centre the actual rear iron sight/optic anchor in the camera sight picture.
                    Vector3 anchorLocal = viewModel.InverseTransformPoint(sightAnchor.position);
                    Vector3 desiredSightPoint = new Vector3(0f, -0.022f, 0.46f);
                    targetPosition = desiredSightPoint - viewModel.localRotation * Vector3.Scale(anchorLocal, viewModel.localScale);
                }
                else if (aiming)
                {
                    targetPosition += new Vector3(-0.105f, 0.075f, 0.15f);
                }
                viewModel.localPosition = Vector3.Lerp(viewModel.localPosition, targetPosition, dt * (aiming ? 16f : 10f));
            }
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
                    shotIndex = 0;
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
            fireCooldown = 0.10f;
            Vector3 muzzle = muzzleAnchor != null
                ? muzzleAnchor.position
                : playerCamera.transform.TransformPoint(new Vector3(0.19f, -0.20f, 0.90f));
            float spread = aiming ? 0.16f : 0.52f;
            if (moving) spread += running ? 1.15f : 0.35f;
            if (staminaFactor < 0.55f) spread += (0.55f - staminaFactor) * 1.4f;
            if (crouched) spread *= 0.78f;
            if (prone) spread *= 0.52f;
            Vector3 direction = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread), 0f) * playerCamera.transform.forward;
            BallisticProjectile projectile = new GameObject("5.45x39mm AK-74M projectile").AddComponent<BallisticProjectile>();
            projectile.Initialize(muzzle, direction * MuzzleVelocity, player.transform, ammoMode == 1,
                ammoMode == 2 ? 42f : 38f, ammoMode == 2 ? 0.48f : 0.34f,
                DragCoefficient, MaxRange);
            if (fireClips != null && fireClips.Length > 0)
            {
                AudioClip shot = fireClips[Random.Range(0, fireClips.Length)];
                audioSource.PlayOneShot(shot, aiming ? 0.67f : 0.80f);
            }
            shotIndex++;
            recoilOffset = Mathf.Min(3.2f, recoilOffset + 1.05f);
            float recoilMultiplier = 1f + (moving ? 0.16f : 0f) + (1f - staminaFactor) * 0.18f;
            float verticalKick = (0.34f + Mathf.Min(shotIndex, 4) * 0.045f) * recoilMultiplier;
            float horizontalKick = ((shotIndex % 2 == 0) ? 0.075f : -0.055f) * recoilMultiplier;
            if (player != null) player.ApplyWeaponRecoil(verticalKick, horizontalKick);
            SpawnMuzzleFlash(muzzle, direction);
            motionLockTimer = 0.17f;
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
            glow.range = 3.2f;
            glow.intensity = 1.8f;
            glow.color = new Color(1f, 0.58f, 0.20f);
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