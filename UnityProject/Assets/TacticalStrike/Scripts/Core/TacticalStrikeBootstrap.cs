using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TacticalStrike
{
    public sealed class TacticalStrikeBootstrap : MonoBehaviour
    {
        public static TacticalStrikeBootstrap Instance { get; private set; }
        private Light sun;
        private Material skyMaterial;
        private VolumeProfile tracerBloomProfile;
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
            QualitySettings.shadowDistance = 220f;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.lodBias = 1.75f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.pixelLightCount = 4;
            Input.multiTouchEnabled = true;
            ConfigureEnvironment();
            ConfigureTracerBloom();

            Transform worldRoot = new GameObject("Battlefield").transform;
            worldRoot.SetParent(transform, false);
            NorthwindValleyGenerator.Build(worldRoot);
            Physics.SyncTransforms();

            float spawnHeight = NorthwindValleyGenerator.HeightAt(0f, 304f);
            FpsPlayerController player = FpsPlayerController.Create(
                new Vector3(0f, spawnHeight + 0.62f, 304f), transform);
            MobileHud hud = MobileHud.Create(player, transform);
            player.AttachHud(hud);
        }

        private void ConfigureEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.38f, 0.43f, 0.44f);
            RenderSettings.fogDensity = 0.00016f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.46f, 0.55f, 0.64f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.34f, 0.31f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.14f, 0.12f);
            RenderSettings.ambientIntensity = 0.92f;

            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                skyMaterial = new Material(skyShader);
                skyMaterial.SetColor("_SkyTint", new Color(0.82f, 0.89f, 0.96f));
                skyMaterial.SetColor("_GroundColor", new Color(0.20f, 0.22f, 0.20f));
                skyMaterial.SetFloat("_AtmosphereThickness", 1.08f);
                RenderSettings.skybox = skyMaterial;
            }

            GameObject sunObject = new GameObject("Plateau Sun");
            sunObject.transform.SetParent(transform, false);
            sunObject.transform.rotation = Quaternion.Euler(43f, -32f, 0f);
            sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.18f;
            sun.color = new Color(1f, 0.985f, 0.95f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.88f;
            sun.shadowBias = 0.035f;
            sun.shadowNormalBias = 0.28f;
            sunObject.name = "Sun - warm highland daylight";
        }

        private void ConfigureTracerBloom()
        {
            GameObject volumeObject = new GameObject("Global Post Processing - controlled tracer bloom");
            volumeObject.transform.SetParent(transform, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.weight = 1f;

            tracerBloomProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            Bloom bloom = tracerBloomProfile.Add<Bloom>(true);
            bloom.threshold.Override(1.12f);
            bloom.intensity.Override(0.38f);
            bloom.scatter.Override(0.52f);
            Tonemapping tone = tracerBloomProfile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            ColorAdjustments grade = tracerBloomProfile.Add<ColorAdjustments>(true);
            grade.postExposure.Override(0f);
            grade.contrast.Override(12f);
            grade.saturation.Override(-7f);
            volume.sharedProfile = tracerBloomProfile;
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
            if (tracerBloomProfile != null)
                Destroy(tracerBloomProfile);
        }
    }
}
