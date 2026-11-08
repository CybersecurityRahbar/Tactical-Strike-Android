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
            RenderSettings.fogColor = new Color(0.58f, 0.49f, 0.38f);
            RenderSettings.fogDensity = 0.00036f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.43f, 0.48f, 0.50f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.38f, 0.31f);
            RenderSettings.ambientGroundColor = new Color(0.17f, 0.15f, 0.12f);
            RenderSettings.ambientIntensity = 1.05f;

            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                skyMaterial = new Material(skyShader);
                skyMaterial.SetColor("_SkyTint", new Color(0.73f, 0.76f, 0.76f));
                skyMaterial.SetColor("_GroundColor", new Color(0.29f, 0.24f, 0.18f));
                skyMaterial.SetFloat("_AtmosphereThickness", 1.15f);
                RenderSettings.skybox = skyMaterial;
            }

            GameObject sunObject = new GameObject("Plateau Sun");
            sunObject.transform.SetParent(transform, false);
            sunObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.91f, 0.77f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.72f;
            sun.shadowBias = 0.045f;
            sun.shadowNormalBias = 0.35f;
            sunObject.name = "Sun - warm highland daylight";
        }

        public void ToggleNightMode()
        {
            nightMode = !nightMode;
            if (sun != null)
            {
                sun.intensity = nightMode ? 0.13f : 1.15f;
                sun.color = nightMode ? new Color(0.39f, 0.48f, 0.72f) : new Color(1f, 0.91f, 0.77f);
            }

            RenderSettings.ambientSkyColor = nightMode
                ? new Color(0.08f, 0.12f, 0.20f)
                : new Color(0.43f, 0.48f, 0.50f);
            RenderSettings.ambientEquatorColor = nightMode
                ? new Color(0.07f, 0.09f, 0.13f)
                : new Color(0.42f, 0.38f, 0.31f);
            RenderSettings.ambientGroundColor = nightMode
                ? new Color(0.025f, 0.035f, 0.055f)
                : new Color(0.17f, 0.15f, 0.12f);
            RenderSettings.fogColor = nightMode
                ? new Color(0.08f, 0.11f, 0.17f)
                : new Color(0.58f, 0.49f, 0.38f);
            if (skyMaterial != null)
                skyMaterial.SetColor("_SkyTint", nightMode
                    ? new Color(0.24f, 0.31f, 0.48f)
                    : new Color(0.73f, 0.76f, 0.76f));
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
