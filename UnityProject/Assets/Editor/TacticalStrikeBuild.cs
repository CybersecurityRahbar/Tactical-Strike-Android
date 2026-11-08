using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TacticalStrike.Editor
{
    [InitializeOnLoad]
    public static class TacticalStrikeBuild
    {
        private const string ScenePath = "Assets/Scenes/TacticalStrike.unity";
        private const string PipelinePath = "Assets/Settings/TacticalStrike_URP.asset";
        private const string RendererPath = "Assets/Settings/TacticalStrike_Renderer.asset";

        static TacticalStrikeBuild()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
                    EnsureProjectReady();
            };
        }

        [MenuItem("Tactical Strike/Prepare Unity Project")]
        public static void EnsureProjectReady()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Settings");
            EnsureUrp();
            EnsureMainScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tactical Strike Unity project setup completed.");
        }

        private static void EnsureUrp()
        {
            UniversalRenderPipelineAsset pipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                UniversalRendererData renderer =
                    AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
                if (renderer == null)
                {
                    renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                    renderer.name = "Tactical Strike - Universal Renderer";
                    AssetDatabase.CreateAsset(renderer, RendererPath);
                }
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "Tactical Strike - Mobile URP";
                pipeline.renderScale = 0.90f;
                pipeline.msaaSampleCount = 2;
                pipeline.supportsHDR = false;
                pipeline.shadowDistance = 85f;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.shadowDistance = 85f;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.lodBias = 1.25f;
            EditorUtility.SetDirty(pipeline);
        }

        private static void EnsureMainScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                    UnityEditor.SceneManagement.NewSceneMode.Single);
                GameObject root = new GameObject("Tactical Strike - runtime bootstrap");
                root.AddComponent<TacticalStrike.TacticalStrikeBootstrap>();
                EditorSceneManager.SaveScene(root.scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        public static void BuildAndroid()
        {
            EnsureProjectReady();
            ValidateRuntimeAssets();

            PlayerSettings.companyName = "CybersecurityRahbar";
            PlayerSettings.productName = "Tactical Strike";
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,
                "com.cybersecurityrahbar.tacticalstrike");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            EditorUserBuildSettings.buildAppBundle = false;

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.GetFullPath(Path.Combine(projectRoot, "..", "build", "Android", "TacticalStrike-debug.apk"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (scenes.Length == 0)
                throw new BuildFailedException("No enabled scenes are configured for Android.");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes.Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Android build failed: " + report.summary.result +
                    " / " + report.summary.totalErrors + " errors.");
            if (!File.Exists(output) || new FileInfo(output).Length < 1024 * 1024)
                throw new BuildFailedException("Android APK is missing or unexpectedly small.");

            Debug.Log("TACTICAL STRIKE ANDROID BUILD PASSED: " + output +
                " (" + report.summary.totalSize + " bytes)");
        }

        private static void ValidateRuntimeAssets()
        {
            string[] required =
            {
                "Assets/Resources/TacticalStrike/Imported/Terrain/rocky_gravel_diff_1k.jpg",
                "Assets/Resources/TacticalStrike/Imported/Terrain/rocky_gravel_nor_gl_1k.jpg",
                "Assets/Resources/TacticalStrike/Imported/AK74M.fbx",
                "Assets/Resources/TacticalStrike/Imported/Audio/Firearms/ak_C_27P.ogg",
                "Assets/Resources/TacticalStrike/Imported/Audio/Firearms/ak_C_36P.ogg",
                "Assets/Resources/TacticalStrike/Imported/Audio/Footsteps/sand_l.ogg",
                "Assets/Resources/TacticalStrike/Imported/Audio/Footsteps/sand_r.ogg"
            };

            foreach (string path in required)
            {
                if (!File.Exists(path))
                    throw new BuildFailedException("Required prepared runtime asset is missing: " + path);
            }

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/TacticalStrike/Imported/AK74M.fbx");
            if (model == null)
                throw new BuildFailedException("AK-74M FBX failed to import as a Unity model.");

            UnityEngine.Object[] imported = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Resources/TacticalStrike/Imported/AK74M.fbx");
            AnimationClip[] clips = imported.OfType<AnimationClip>().ToArray();
            if (clips.Length == 0)
                Debug.LogWarning("AK-74M imported without separate animation clips; the build will retain the procedural viewmodel fallback.");
            else
                Debug.Log("AK-74M animation clips imported: " +
                    string.Join(", ", clips.Select(clip => clip.name)));

            string[] terrainTextures = Directory.GetFiles(
                "Assets/Resources/TacticalStrike/Imported/Terrain", "*.jpg");
            if (terrainTextures.Length < 3)
                throw new BuildFailedException("Expected diffuse, normal and roughness PBR maps in the Unity Resources folder.");

            Debug.Log("Runtime asset validation passed: terrain PBR, AK-74M model and recorded audio are present.");
        }
    }

    public sealed class TerrainPbrTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/TacticalStrike/Imported/Terrain/")) return;
            TextureImporter importer = (TextureImporter)assetImporter;
            string file = Path.GetFileName(assetPath).ToLowerInvariant();

            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 1024;

            if (file.Contains("_nor_gl_"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = file.Contains("_diff_");
            }
        }
    }

    public sealed class AK74MModelPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.EndsWith("/AK74M.fbx", StringComparison.OrdinalIgnoreCase)) return;
            ModelImporter importer = (ModelImporter)assetImporter;
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.optimizeGameObjects = false;
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                string n = clips[i].name.ToLowerInvariant();
                bool loop = n.Contains("idle") || n.Contains("walk") || n.Contains("run");
                clips[i].loop = loop;
                clips[i].loopTime = loop;
            }
            importer.clipAnimations = clips;
        }
    }
}
