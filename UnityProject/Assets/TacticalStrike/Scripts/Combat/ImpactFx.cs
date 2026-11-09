using UnityEngine;

namespace TacticalStrike
{
    public static class ImpactFx
    {
        private static Material sparkMaterial;
        private static Material dustMaterial;
        private static AudioClip dirtImpact;
        private static AudioClip masonryImpact;
        private static AudioClip metalImpact;

        public static void Spawn(Vector3 position, Vector3 normal, bool heavy, string surface = "")
        {
            Vector3 facing = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
            PlaySurfaceAudio(position, surface);
            CreateSparks(position + facing * 0.025f, facing, heavy);
            if (!surface.Contains("metal"))
                CreateDust(position + facing * 0.035f, facing, heavy, surface);
        }

        private static void CreateSparks(Vector3 position, Vector3 normal, bool heavy)
        {
            GameObject root = new GameObject(heavy ? "Impact - hot heavy sparks" : "Impact - hot sparks");
            root.transform.position = position;
            root.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

            ParticleSystem system = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.duration = 0.34f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, heavy ? 0.40f : 0.29f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(heavy ? 5.5f : 4.0f, heavy ? 12.5f : 9.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.018f, heavy ? 0.055f : 0.038f);
            main.startColor = Color.white;
            main.gravityModifier = 1.15f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = heavy ? 80 : 44;

            Gradient glow = new Gradient();
            glow.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.98f, 0.76f), 0f),
                    new GradientColorKey(new Color(1f, 0.73f, 0.10f), 0.42f),
                    new GradientColorKey(new Color(1f, 0.20f, 0.025f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.82f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });
            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(glow);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(heavy ? 38 : 22)) });

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 36f;
            shape.radius = heavy ? 0.045f : 0.025f;
            shape.length = 0.09f;

            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3.6f;
            renderer.velocityScale = 0.05f;
            if (sparkMaterial == null) sparkMaterial = MakeParticleMaterial(Color.white);
            if (sparkMaterial != null) renderer.sharedMaterial = sparkMaterial;

            system.Play();
            Object.Destroy(root, 0.85f);
        }

        private static void CreateDust(Vector3 position, Vector3 normal, bool heavy, string surface)
        {
            GameObject root = new GameObject(heavy ? "Impact - debris and dust" : "Impact - surface dust");
            root.transform.position = position;
            root.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);

            ParticleSystem system = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.duration = 0.55f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.20f, heavy ? 0.82f : 0.54f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.55f, heavy ? 3.5f : 2.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, heavy ? 0.32f : 0.19f);
            main.startColor = surface.Contains("grass")
                ? new Color(0.28f, 0.34f, 0.20f, 0.52f)
                : new Color(0.40f, 0.34f, 0.25f, 0.56f);
            main.gravityModifier = -0.06f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = heavy ? 54 : 28;

            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.78f, 0.68f, 0.50f), 0f),
                    new GradientColorKey(new Color(0.42f, 0.37f, 0.30f), 1f)
                },
                new[] {
                    new GradientAlphaKey(0.58f, 0f),
                    new GradientAlphaKey(0.42f, 0.16f),
                    new GradientAlphaKey(0f, 1f)
                });
            ParticleSystem.ColorOverLifetimeModule color = system.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(fade);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(heavy ? 24 : 12)) });

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = heavy ? 0.28f : 0.13f;

            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (dustMaterial == null) dustMaterial = MakeParticleMaterial(Color.white);
            if (dustMaterial != null) renderer.sharedMaterial = dustMaterial;

            system.Play();
            Object.Destroy(root, 1.2f);
        }

        private static Material MakeParticleMaterial(Color tint)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return null;

            Material material = new Material(shader)
            {
                name = "Tactical Strike - URP impact particles",
                renderQueue = 3000
            };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            return material;
        }

        private static void PlaySurfaceAudio(Vector3 position, string surface)
        {
            string key = (surface ?? string.Empty).ToLowerInvariant();
            AudioClip clip;
            if (key.Contains("metal"))
            {
                if (metalImpact == null)
                    metalImpact = Resources.Load<AudioClip>("TacticalStrike/Imported/Audio/Impacts/metal-impact");
                clip = metalImpact;
            }
            else if (key.Contains("brick") || key.Contains("stone") ||
                     key.Contains("house") || key.Contains("concrete"))
            {
                if (masonryImpact == null)
                    masonryImpact = Resources.Load<AudioClip>("TacticalStrike/Imported/Audio/Impacts/debris-crash");
                clip = masonryImpact;
            }
            else
            {
                if (dirtImpact == null)
                    dirtImpact = Resources.Load<AudioClip>("TacticalStrike/Imported/Audio/Impacts/heavy-thud");
                clip = dirtImpact;
            }

            if (clip != null) AudioSource.PlayClipAtPoint(clip, position, 0.70f);
        }
    }
}