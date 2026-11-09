using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalStrike
{
    public sealed class TacticalStrikeBootstrap : MonoBehaviour
    {
        public static TacticalStrikeBootstrap Instance { get; private set; }
        private Light sun;
        private Material skyMaterial;
        private bool nightMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.maxQueuedFrames = 2;
            Input.multiTouchEnabled = true;
            ConfigureEnvironment();

            Transform worldRoot = new GameObject("Battlefield").transform;
            worldRoot.SetParent(transform, false);
            BattlefieldGenerator.Build(worldRoot);

            float spawnHeight = BattlefieldGenerator.HeightAt(0f, 292f);
            FpsPlayerController player = FpsPlayerController.Create(
                new Vector3(0f, spawnHeight + 0.12f, 292f), transform);
            MobileHud hud = MobileHud.Create(player, transform);
            player.AttachHud(hud);
        }

        private void ConfigureEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.48f, 0.57f, 0.43f);
            RenderSettings.fogDensity = 0.000075f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.68f, 0.81f);
            RenderSettings.ambientEquatorColor = new Color(0.40f, 0.47f, 0.36f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.19f, 0.13f);
            RenderSettings.ambientIntensity = 1.0f;

            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                skyMaterial = new Material(skyShader);
                skyMaterial.SetColor("_SkyTint", new Color(0.90f, 0.95f, 1.0f));
                skyMaterial.SetColor("_GroundColor", new Color(0.28f, 0.33f, 0.25f));
                skyMaterial.SetFloat("_AtmosphereThickness", 0.82f);
                RenderSettings.skybox = skyMaterial;
            }

            GameObject sunObject = new GameObject("Plateau Sun");
            sunObject.transform.SetParent(transform, false);
            sunObject.transform.rotation = Quaternion.Euler(57f, -26f, 0f);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.42f;
            sun.color = new Color(1f, 0.98f, 0.90f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.82f;
            sun.shadowBias = 0.045f;
            sun.shadowNormalBias = 0.35f;
            sunObject.name = "Sun - warm highland daylight";
        }

        public void ToggleNightMode()
        {
            nightMode = !nightMode;
            if (sun != null)
            {
                // Night is a low-light environment, not a warm late-afternoon tint.
                sun.intensity = nightMode ? 0.018f : 1.42f;
                sun.color = nightMode ? new Color(0.20f, 0.29f, 0.48f) : new Color(1f, 0.98f, 0.90f);
                sun.shadowStrength = nightMode ? 0.35f : 0.82f;
            }

            RenderSettings.ambientIntensity = nightMode ? 0.20f : 1.0f;
            RenderSettings.ambientSkyColor = nightMode
                ? new Color(0.012f, 0.020f, 0.052f)
                : new Color(0.56f, 0.68f, 0.81f);
            RenderSettings.ambientEquatorColor = nightMode
                ? new Color(0.010f, 0.015f, 0.032f)
                : new Color(0.40f, 0.47f, 0.36f);
            RenderSettings.ambientGroundColor = nightMode
                ? new Color(0.004f, 0.006f, 0.012f)
                : new Color(0.16f, 0.19f, 0.13f);
            RenderSettings.fogColor = nightMode
                ? new Color(0.008f, 0.014f, 0.035f)
                : new Color(0.48f, 0.57f, 0.43f);
            RenderSettings.fogDensity = nightMode ? 0.00042f : 0.000075f;
            if (skyMaterial != null)
            {
                skyMaterial.SetColor("_SkyTint", nightMode
                    ? new Color(0.12f, 0.16f, 0.29f)
                    : new Color(0.90f, 0.95f, 1.0f));
                skyMaterial.SetColor("_GroundColor", nightMode
                    ? new Color(0.006f, 0.010f, 0.022f)
                    : new Color(0.28f, 0.33f, 0.25f));
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (skyMaterial != null)
                Destroy(skyMaterial);
        }
    }
}
