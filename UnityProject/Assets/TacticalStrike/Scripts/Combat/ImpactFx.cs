using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalStrike
{
    public static class ImpactFx
    {
        private static Material sparkMaterial;
        private static Material dustMaterial;
        private static Material soilScarMaterial;
        private static Material soilRimMaterial;
        private static AudioClip dirtImpact;
        private static AudioClip masonryImpact;
        private static AudioClip metalImpact;

        public static void Spawn(Vector3 position, Vector3 normal, bool heavy, string surface = "")
        {
            Vector3 facing = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
            string key = (surface ?? string.Empty).ToLowerInvariant();
            bool metal = key.Contains("metal");
            bool masonry = key.Contains("stone") || key.Contains("brick") ||
                           key.Contains("concrete") || key.Contains("masonry");

            PlaySurfaceAudio(position, key);
            if (metal || masonry)
                CreateSparks(position + facing * 0.025f, facing, heavy);
            if (!metal)
                CreateDust(position + facing * 0.035f, facing, heavy, key);
            if (!metal && !masonry && facing.y > 0.58f)
                CreateGroundImpactMark(position, facing, heavy);
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
            main.gravityModifier = 0.18f;
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

        private static void CreateGroundImpactMark(Vector3 position, Vector3 normal, bool heavy)
        {
            // A temporary surface mesh leaves a visible compacted impact scar; this
            // is not destructive TerrainData sculpting.
            float radius = heavy ? 0.28f : 0.15f;
            GameObject root = new GameObject("Ground impact - compacted soil mark");
            root.transform.position = position + normal * 0.024f;
            root.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) *
                                      Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            if (soilScarMaterial == null)
                soilScarMaterial = MakeSolidMaterial("Impact scar - dark compacted soil",
                    new Color(0.095f, 0.073f, 0.052f));
            if (soilRimMaterial == null)
                soilRimMaterial = MakeSolidMaterial("Impact scar - disturbed earth rim",
                    new Color(0.30f, 0.235f, 0.155f));

            CreateScarDisk(root.transform, "Depressed dark centre", radius * 0.68f,
                -0.006f, soilScarMaterial);
            CreateScarRing(root.transform, "Raised irregular soil lip", radius * 0.70f,
                radius, -0.002f, 0.012f, soilRimMaterial);
            Object.Destroy(root, 32f);
        }

        private static void CreateScarDisk(Transform parent, string label, float radius,
            float height, Material material)
        {
            const int segments = 20;
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            vertices[0] = new Vector3(0f, height, 0f);
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                float irregular = 0.88f + Random.Range(0f, 0.20f);
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius * irregular,
                    height + Random.Range(-0.0015f, 0.0015f),
                    Mathf.Sin(angle) * radius * irregular);
            }
            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                int current = i + 1;
                int next = ((i + 1) % segments) + 1;
                triangles[t++] = 0;
                triangles[t++] = next;
                triangles[t++] = current;
            }
            Mesh mesh = new Mesh { name = label };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject disc = new GameObject(label);
            disc.transform.SetParent(parent, false);
            disc.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = disc.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void CreateScarRing(Transform parent, string label, float innerRadius,
            float outerRadius, float innerHeight, float outerHeight, Material material)
        {
            const int segments = 20;
            Vector3[] vertices = new Vector3[segments * 2];
            int[] triangles = new int[segments * 6];
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                float irregular = 0.88f + Random.Range(0f, 0.20f);
                float x = Mathf.Cos(angle) * irregular;
                float z = Mathf.Sin(angle) * irregular;
                vertices[i * 2] = new Vector3(x * innerRadius, innerHeight, z * innerRadius);
                vertices[i * 2 + 1] = new Vector3(x * outerRadius, outerHeight, z * outerRadius);
            }
            int t = 0;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int inner0 = i * 2;
                int outer0 = inner0 + 1;
                int inner1 = next * 2;
                int outer1 = inner1 + 1;
                triangles[t++] = inner0;
                triangles[t++] = outer1;
                triangles[t++] = outer0;
                triangles[t++] = inner0;
                triangles[t++] = inner1;
                triangles[t++] = outer1;
            }
            Mesh mesh = new Mesh { name = label };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject ring = new GameObject(label);
            ring.transform.SetParent(parent, false);
            ring.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = ring.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material MakeSolidMaterial(string label, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = label };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.035f);
            return material;
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