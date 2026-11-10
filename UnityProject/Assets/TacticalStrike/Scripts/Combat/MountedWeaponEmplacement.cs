using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalStrike
{
    /// <summary>
    /// Functional single-player crew-served heavy machine-gun station.
    /// This is an original runtime rig; it does not use Squad's proprietary assets.
    /// </summary>
    public sealed class MountedWeaponEmplacement : MonoBehaviour
    {
        public Transform AimPivot { get; private set; }
        public Transform WeaponAssembly { get; private set; }
        public string DisplayName => displayName;
        public bool IsOccupied => operatorPlayer != null;
        public int BeltAmmo => beltAmmo;
        public int BeltReserve => beltReserve;

        private string displayName = "Heavy machine gun";
        private FpsPlayerController operatorPlayer;
        private Camera operatorCamera;
        private Transform previousCameraParent;
        private Vector3 previousCameraLocalPosition;
        private Quaternion previousCameraLocalRotation;
        private float previousCameraFov;
        private Vector3 exitPosition;
        private Quaternion exitRotation;
        private Material gunmetal;
        private Material darkSteel;
        private float yaw;
        private float pitch;
        private float fireCooldown;
        private float reloadTimer;
        private int reloadTransfer;
        private int shotCounter;
        private int beltAmmo = 250;
        private int beltReserve = 750;
        private BallisticProfile ballisticProfile = BallisticProfiles.M2_12_7x99_M33;
        private bool fireHeld;
        private bool emptyNoticeShown;

        private const int BeltCapacity = 250;
        private const float TraverseLimit = 66f;
        private const float ElevationLimit = 24f;
        private const float DepressionLimit = -18f;
        private const float CyclicRate = 9.2f;

        public void Initialize(string weaponName)
        {
            displayName = string.IsNullOrWhiteSpace(weaponName) ? "Heavy machine gun" : weaponName;
            ballisticProfile = displayName.IndexOf("DShK", System.StringComparison.OrdinalIgnoreCase) >= 0
                ? BallisticProfiles.DShK_12_7x108
                : BallisticProfiles.M2_12_7x99_M33;
            if (AimPivot != null) return;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            gunmetal = CreateMaterial(shader, new Color(0.105f, 0.118f, 0.112f), displayName + " - blued gunmetal");
            darkSteel = CreateMaterial(shader, new Color(0.045f, 0.052f, 0.050f), displayName + " - dark steel");

            GameObject pivot = new GameObject(displayName + " - traverse/elevation pivot");
            AimPivot = pivot.transform;
            AimPivot.SetParent(transform, false);
            AimPivot.localPosition = new Vector3(0f, 1.84f, -0.52f);
            AimPivot.localRotation = Quaternion.identity;

            GameObject assembly = new GameObject(displayName + " - receiver/barrel/sights");
            WeaponAssembly = assembly.transform;
            WeaponAssembly.SetParent(AimPivot, false);
            BuildWeaponAssembly();
        }

        private static Material CreateMaterial(Shader shader, Color color, string label)
        {
            Material material = new Material(shader) { name = label };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.72f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.48f);
            material.enableInstancing = true;
            return material;
        }

        private void BuildWeaponAssembly()
        {
            // The weapon assembly, including its iron sights and muzzle, rotates as
            // one rigid mechanism around the fixed cradle.
            CreateGunPart("Receiver", new Vector3(0f, -0.02f, 0.72f),
                new Vector3(0.72f, 0.43f, 0.92f), gunmetal);
            CreateGunPart("Feed cover", new Vector3(0f, 0.25f, 0.74f),
                new Vector3(0.76f, 0.11f, 0.86f), darkSteel);
            CreateGunCylinder("Heavy barrel", new Vector3(0f, 0.075f, 1.82f),
                new Vector3(0.13f, 0.71f, 0.13f), darkSteel);
            CreateGunCylinder("Barrel jacket", new Vector3(0f, 0.075f, 1.54f),
                new Vector3(0.22f, 0.37f, 0.22f), gunmetal);
            CreateGunPart("Muzzle collar", new Vector3(0f, 0.075f, 2.53f),
                new Vector3(0.24f, 0.24f, 0.16f), gunmetal);
            CreateGunPart("Rear sight tower", new Vector3(0f, 0.36f, 0.34f),
                new Vector3(0.16f, 0.19f, 0.14f), darkSteel);
            CreateGunPart("Front sight tower", new Vector3(0f, 0.28f, 2.16f),
                new Vector3(0.12f, 0.22f, 0.11f), darkSteel);
            CreateGunPart("Left grip", new Vector3(-0.47f, -0.18f, 0.60f),
                new Vector3(0.13f, 0.22f, 0.30f), darkSteel);
            CreateGunPart("Right grip", new Vector3(0.47f, -0.18f, 0.60f),
                new Vector3(0.13f, 0.22f, 0.30f), darkSteel);
            CreateGunPart("Charging handle", new Vector3(-0.40f, 0.08f, 0.49f),
                new Vector3(0.28f, 0.09f, 0.12f), gunmetal);
            CreateGunPart("Ammo feed box", new Vector3(-0.50f, -0.30f, 0.92f),
                new Vector3(0.34f, 0.48f, 0.42f), gunmetal);
            CreateGunPart("Rear buffer", new Vector3(0f, -0.09f, -0.04f),
                new Vector3(0.38f, 0.30f, 0.56f), darkSteel);
        }

        private void CreateGunCylinder(string label, Vector3 localPosition,
            Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            part.name = displayName + " - " + label;
            part.transform.SetParent(WeaponAssembly, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private void CreateGunPart(string label, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = displayName + " - " + label;
            part.transform.SetParent(WeaponAssembly, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        public static MountedWeaponEmplacement FindNearest(Vector3 position, float radius)
        {
            MountedWeaponEmplacement[] all = Object.FindObjectsOfType<MountedWeaponEmplacement>();
            MountedWeaponEmplacement nearest = null;
            float bestDistance = radius * radius;
            for (int i = 0; i < all.Length; i++)
            {
                MountedWeaponEmplacement candidate = all[i];
                if (candidate == null || candidate.IsOccupied) continue;
                float distance = (candidate.transform.position - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearest = candidate;
                }
            }
            return nearest;
        }

        public bool Mount(FpsPlayerController player)
        {
            if (player == null || operatorPlayer != null || AimPivot == null) return false;

            operatorPlayer = player;
            operatorCamera = player.PlayerCamera;
            previousCameraParent = operatorCamera.transform.parent;
            previousCameraLocalPosition = operatorCamera.transform.localPosition;
            previousCameraLocalRotation = operatorCamera.transform.localRotation;
            previousCameraFov = operatorCamera.fieldOfView;

            exitPosition = transform.TransformPoint(new Vector3(0f, 0f, -4.25f));
            exitRotation = transform.rotation;
            Vector3 operatorPosition = transform.TransformPoint(new Vector3(0f, 0f, -1.55f));

            yaw = 0f;
            pitch = 0f;
            AimPivot.localRotation = Quaternion.identity;
            operatorCamera.transform.SetParent(AimPivot, false);
            operatorCamera.transform.localPosition = new Vector3(0f, -0.015f, -0.46f);
            operatorCamera.transform.localRotation = Quaternion.identity;
            operatorCamera.fieldOfView = 54f;

            player.BeginMounted(this, operatorPosition, exitRotation);
            player.NotifyMounted(true, displayName);
            player.UpdateMountedWeaponHud(this);
            player.ShowHudMessage(displayName + " READY  |  250-ROUND BELT");
            return true;
        }

        public void Dismount()
        {
            if (operatorPlayer == null) return;
            FpsPlayerController oldOperator = operatorPlayer;
            Camera oldCamera = operatorCamera;
            fireHeld = false;
            operatorPlayer = null;
            operatorCamera = null;

            if (oldCamera != null && previousCameraParent != null)
            {
                oldCamera.transform.SetParent(previousCameraParent, false);
                oldCamera.transform.localPosition = previousCameraLocalPosition;
                oldCamera.transform.localRotation = previousCameraLocalRotation;
                oldCamera.fieldOfView = previousCameraFov;
            }

            oldOperator.EndMounted(this, exitPosition, exitRotation);
            oldOperator.NotifyMounted(false, displayName);
            previousCameraParent = null;
        }

        public void AddLookDelta(Vector2 delta)
        {
            if (operatorPlayer == null || AimPivot == null) return;
            yaw = Mathf.Clamp(yaw + delta.x * 0.105f, -TraverseLimit, TraverseLimit);
            pitch = Mathf.Clamp(pitch - delta.y * 0.105f, DepressionLimit, ElevationLimit);
            AimPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        public void SetFireHeld(bool held)
        {
            fireHeld = operatorPlayer != null && held && reloadTimer <= 0f && beltAmmo > 0;
        }

        public void ReloadBelt()
        {
            if (operatorPlayer == null || reloadTimer > 0f || beltAmmo >= BeltCapacity || beltReserve <= 0)
                return;
            reloadTransfer = Mathf.Min(BeltCapacity - beltAmmo, beltReserve);
            if (reloadTransfer <= 0) return;
            fireHeld = false;
            reloadTimer = 3.35f;
            operatorPlayer.ShowHudMessage("REPLACING 12.7 MM AMMUNITION BELT...");
        }

        public void Tick(float deltaTime)
        {
            if (operatorPlayer == null) return;
            fireCooldown = Mathf.Max(0f, fireCooldown - deltaTime);
            if (reloadTimer > 0f)
            {
                reloadTimer = Mathf.Max(0f, reloadTimer - deltaTime);
                if (reloadTimer <= 0f && reloadTransfer > 0)
                {
                    beltAmmo += reloadTransfer;
                    beltReserve -= reloadTransfer;
                    reloadTransfer = 0;
                    emptyNoticeShown = false;
                    operatorPlayer.UpdateMountedWeaponHud(this);
                    operatorPlayer.ShowHudMessage("BELT LOADED  |  " + beltAmmo + " ROUNDS");
                }
            }

            if (fireHeld && beltAmmo <= 0)
            {
                fireHeld = false;
                if (!emptyNoticeShown)
                {
                    emptyNoticeShown = true;
                    operatorPlayer.ShowHudMessage("HEAVY MACHINE GUN BELT EMPTY  |  PRESS R TO RELOAD");
                }
            }

            if (fireHeld && reloadTimer <= 0f && fireCooldown <= 0f)
                Fire();
        }

        private void Fire()
        {
            if (beltAmmo <= 0) return;
            fireCooldown = 1f / CyclicRate;
            shotCounter++;
            beltAmmo--;
            operatorPlayer?.UpdateMountedWeaponHud(this);

            Vector3 muzzle = AimPivot.TransformPoint(new Vector3(0f, 0.075f, 2.64f));
            Vector3 direction = AimPivot.forward;
            float dispersion = 0.14f + (shotCounter % 5) * 0.035f;
            direction = Quaternion.Euler(
                Random.Range(-dispersion, dispersion),
                Random.Range(-dispersion, dispersion), 0f) * direction;

            BallisticProjectile projectile = new GameObject(displayName + " - " +
                ballisticProfile.AmmunitionName + " projectile").AddComponent<BallisticProjectile>();
            projectile.Initialize(muzzle, direction * ballisticProfile.MuzzleVelocity,
                transform, shotCounter % 3 == 0, ballisticProfile);

            GameObject flash = new GameObject(displayName + " - muzzle flash");
            flash.transform.position = muzzle;
            Light light = flash.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.58f, 0.22f);
            light.intensity = 10f;
            light.range = 6.5f;
            light.shadows = LightShadows.None;
            Destroy(flash, 0.045f);

            float recovery = (shotCounter % 2 == 0) ? -0.075f : 0.055f;
            pitch = Mathf.Clamp(pitch + 0.11f, DepressionLimit, ElevationLimit);
            yaw = Mathf.Clamp(yaw + recovery, -TraverseLimit, TraverseLimit);
            AimPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void OnDisable()
        {
            if (operatorPlayer != null)
                Dismount();
        }
    }
}
