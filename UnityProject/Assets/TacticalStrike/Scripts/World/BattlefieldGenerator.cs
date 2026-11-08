using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalStrike
{
    public static class BattlefieldGenerator
    {
        public const float MapSize = 700f;
        private const int Divisions = 256;

        private static Material earth;
        private static Material sandbag;
        private static Material brick;
        private static Material stone;
        private static Material darkMetal;
        private static Material soldier;
        private static Material roof;

        public static void Build(Transform parent)
        {
            CreateMaterials();
            BuildTerrain(parent);
            BuildTrenchAndCover(parent, 205f, false);
            BuildTrenchAndCover(parent, -205f, true);
            BuildMidfieldCover(parent);
            BuildBuildings(parent);
            ScatterRockOutcrops(parent);
            PlaceDistantDefenders(parent);
        }

        public static float HeightAt(float x, float z)
        {
            float warpX = x + Noise(x * 0.004f, z * 0.004f, 20261108) * 46f;
            float warpZ = z + Noise(x * 0.004f + 117f, z * 0.004f - 83f, 20261109) * 54f;
            float broad = Fbm(warpX * 0.0045f, warpZ * 0.0045f, 20261108, 5) * 11.5f;
            float ridgeSample = Fbm(warpX * 0.006f, warpZ * 0.006f, 20261109, 4);
            float ridges = Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(ridgeSample)), 2.1f) * 24f;
            float rolling = Fbm(warpX * 0.027f, warpZ * 0.027f, 741208, 4) * 4.2f;
            float fine = Noise(warpX * 0.11f, warpZ * 0.11f, 741209) * 0.72f;

            float axisA = z - (174f + Mathf.Sin(x * 0.0065f) * 46f + Noise(x * 0.006f, (z + 91f) * 0.006f, 20261110) * 17f);
            float axisB = z - (72f + Mathf.Sin(x * 0.008f) * 34f + Noise((x + 131f) * 0.006f, z * 0.006f, 20261111) * 18f);
            float axisC = z - (-52f + Mathf.Sin(x * 0.0072f) * 38f + Noise((x - 83f) * 0.006f, (z + 47f) * 0.006f, 20261112) * 16f);
            float axisD = z - (-167f + Mathf.Sin(x * 0.009f) * 24f + Noise((x + 193f) * 0.006f, (z - 63f) * 0.006f, 20261113) * 12f);
            float ridgeA = 21f * Mathf.Exp(-Mathf.Pow(axisA / 68f, 2f));
            float ridgeB = 24f * Mathf.Exp(-Mathf.Pow(axisB / 58f, 2f));
            float ridgeC = 19f * Mathf.Exp(-Mathf.Pow(axisC / 55f, 2f));
            float ridgeD = 13f * Mathf.Exp(-Mathf.Pow(axisD / 44f, 2f));
            float gullySample = Mathf.Abs(Fbm(warpX * 0.005f, warpZ * 0.005f, 20261110, 3));
            float gullyCut = Mathf.Pow(Mathf.Max(0f, 0.22f - gullySample), 2f) * 105f;
            float shelf = 6f * Mathf.Exp(-Mathf.Pow((x - (118f + Mathf.Sin(z * 0.009f) * 29f)) / 48f, 2f));
            float valley = -5.5f * Mathf.Exp(-Mathf.Pow((z - 12f) / 39f, 2f));

            float height = broad + ridges + rolling + fine + ridgeA + ridgeB + ridgeC + ridgeD + shelf - gullyCut + valley;
            float attackerTrench = Mathf.Abs(z - (205f + Mathf.Sin(x * 0.055f) * 2.2f));
            float defenderTrench = Mathf.Abs(z - (-205f + Mathf.Sin(x * 0.045f) * 2.4f));
            if (attackerTrench < 3f)
                height -= (1f - attackerTrench / 3f) * 1.65f;
            if (defenderTrench < 3f)
                height -= (1f - defenderTrench / 3f) * 1.75f;
            return height;
        }

        private static float Noise(float x, float y, int seed)
        {
            float offsetX = (seed % 997) * 0.173f;
            float offsetY = (seed % 613) * 0.271f;
            return (Mathf.PerlinNoise(x + offsetX, y + offsetY) - 0.5f) * 2f;
        }

        private static float Fbm(float x, float y, int seed, int octaves)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float total = 0f;
            float weight = 0f;
            for (int i = 0; i < octaves; i++)
            {
                total += Noise(x * frequency, y * frequency, seed + i * 79) * amplitude;
                weight += amplitude;
                frequency *= 2f;
                amplitude *= 0.48f;
            }
            return total / Mathf.Max(0.001f, weight);
        }

        private static void CreateMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            earth = new Material(lit) { name = "Dry plateau - Rocky Gravel PBR", color = new Color(0.91f, 0.84f, 0.72f) };
            sandbag = MakeMaterial(new Color(0.50f, 0.42f, 0.29f), "Weathered canvas sandbags");
            brick = MakeMaterial(new Color(0.39f, 0.27f, 0.20f), "Burnt clay brick");
            stone = MakeMaterial(new Color(0.43f, 0.41f, 0.36f), "Dry limestone");
            darkMetal = MakeMaterial(new Color(0.105f, 0.12f, 0.105f), "Dark gunmetal");
            soldier = MakeMaterial(new Color(0.13f, 0.17f, 0.13f), "Distant olive uniform");
            roof = MakeMaterial(new Color(0.24f, 0.22f, 0.19f), "Weathered corrugated roof");

            Texture2D diffuse = Resources.Load<Texture2D>("TacticalStrike/Imported/Terrain/rocky_gravel_diff_1k");
            Texture2D normal = Resources.Load<Texture2D>("TacticalStrike/Imported/Terrain/rocky_gravel_nor_gl_1k");
            if (diffuse != null)
            {
                earth.SetTexture("_BaseMap", diffuse);
                earth.SetTexture("_MainTex", diffuse);
                diffuse.wrapMode = TextureWrapMode.Repeat;
            }
            if (normal != null)
            {
                earth.SetTexture("_BumpMap", normal);
                earth.EnableKeyword("_NORMALMAP");
                normal.wrapMode = TextureWrapMode.Repeat;
            }
            earth.SetTextureScale("_BaseMap", new Vector2(0.065f, 0.065f));
            earth.SetTextureScale("_MainTex", new Vector2(0.065f, 0.065f));
            earth.SetFloat("_Smoothness", 0.13f);
        }

        private static Material MakeMaterial(Color color, string name)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            return new Material(shader) { color = color, name = name };
        }

        private static void BuildTerrain(Transform parent)
        {
            int side = Divisions + 1;
            Vector3[] vertices = new Vector3[side * side];
            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[Divisions * Divisions * 6];
            float half = MapSize * 0.5f;
            float step = MapSize / Divisions;
            int vertexIndex = 0;
            for (int z = 0; z < side; z++)
            {
                for (int x = 0; x < side; x++)
                {
                    float wx = -half + x * step;
                    float wz = -half + z * step;
                    vertices[vertexIndex] = new Vector3(wx, HeightAt(wx, wz), wz);
                    uv[vertexIndex] = new Vector2(wx * 0.065f, wz * 0.065f);
                    float shade = Noise(wx * 0.31f, wz * 0.31f, 741208) * 0.025f;
                    colors[vertexIndex] = new Color(
                        Mathf.Clamp(0.95f + shade, 0.84f, 1f),
                        Mathf.Clamp(0.92f + shade * 0.8f, 0.82f, 0.99f),
                        Mathf.Clamp(0.87f + shade * 0.5f, 0.76f, 0.97f), 1f);
                    vertexIndex++;
                }
            }

            int ti = 0;
            for (int z = 0; z < Divisions; z++)
            {
                for (int x = 0; x < Divisions; x++)
                {
                    int i00 = z * side + x;
                    int i10 = i00 + 1;
                    int i01 = i00 + side;
                    int i11 = i01 + 1;
                    triangles[ti++] = i00; triangles[ti++] = i01; triangles[ti++] = i10;
                    triangles[ti++] = i10; triangles[ti++] = i01; triangles[ti++] = i11;
                }
            }

            Mesh mesh = new Mesh { name = "Plateau 700m - 256x256 terrain", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject terrain = new GameObject("Terrain - sculpted dusty plateau");
            terrain.transform.SetParent(parent, false);
            MeshFilter filter = terrain.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = terrain.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = earth;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            MeshCollider collider = terrain.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        private static void BuildTrenchAndCover(Transform parent, float lineZ, bool defenders)
        {
            string prefix = defenders ? "Defender" : "Attacker";
            Material wallMaterial = defenders ? brick : stone;
            Mesh capsuleMesh = GetSandbagSourceMesh();
            List<CombineInstance> bagInstances = new List<CombineInstance>(360);

            for (float x = -148f; x <= 148f; x += 2.15f)
            {
                float zOffset = Mathf.Sin(x * 0.045f) * 2.4f;
                float ground = HeightAt(x, lineZ + zOffset);
                if (Mathf.Abs(x) > 132f && Mathf.Sin(x * 0.11f) > 0.45f) continue;

                AddSandbagInstance(bagInstances, capsuleMesh, parent,
                    new Vector3(x, ground + 0.36f, lineZ + zOffset - 5.5f),
                    Mathf.Sin(x * 0.07f) * 3f, new Vector3(0.34f, 0.82f, 0.36f));

                if ((Mathf.Abs(x) % 4.3f) < 2.15f)
                    AddSandbagInstance(bagInstances, capsuleMesh, parent,
                        new Vector3(x + 0.32f, ground + 0.72f, lineZ + zOffset - 5.45f),
                        Mathf.Sin(x * 0.05f) * 2.4f, new Vector3(0.34f, 0.82f, 0.36f));

                if (Mathf.Abs(x) < 125f && ((int)(x + 150f) % 17 == 0))
                    CreateBox(parent, new Vector3(x, ground + 0.55f, lineZ + zOffset + 5.1f),
                        new Vector3(3.2f, 1.1f, 0.55f), wallMaterial, prefix + " firing wall", 90f);
            }

            for (float x = -132f; x <= 132f; x += 2.4f)
            {
                float z = lineZ + Mathf.Sin(x * 0.045f) * 2.4f + 5f;
                float y = HeightAt(x, z);
                AddSandbagInstance(bagInstances, capsuleMesh, parent,
                    new Vector3(x, y + 0.34f, z + 7.6f),
                    Mathf.Sin(x * 0.04f) * 2f, new Vector3(0.34f, 0.82f, 0.36f));
            }

            BuildSandbagBatch(parent, prefix, bagInstances);

            for (int i = 0; i < 12; i++)
            {
                float x = -120f + i * 22f;
                float z = lineZ + Mathf.Sin(x * 0.045f) * 2.4f;
                CreateBox(parent, new Vector3(x, HeightAt(x, z) + 0.6f, z + 12f),
                    new Vector3(3.8f, 1.2f, 0.55f), wallMaterial, prefix + " cover block", 90f);
            }
        }

        private static Mesh GetSandbagSourceMesh()
        {
            GameObject source = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Mesh mesh = source.GetComponent<MeshFilter>().sharedMesh;
            source.SetActive(false);
            Object.Destroy(source);
            return mesh;
        }

        private static void AddSandbagInstance(List<CombineInstance> instances, Mesh mesh,
            Transform parent, Vector3 position, float yaw, Vector3 scale)
        {
            Vector3 localPosition = parent.InverseTransformPoint(position);
            instances.Add(new CombineInstance
            {
                mesh = mesh,
                transform = Matrix4x4.TRS(
                    localPosition,
                    Quaternion.Euler(0f, yaw, 90f),
                    scale)
            });
        }

        private static void BuildSandbagBatch(Transform parent, string prefix,
            List<CombineInstance> instances)
        {
            if (instances.Count == 0) return;
            Mesh combined = new Mesh
            {
                name = prefix + " sandbag battlement - batched",
                indexFormat = IndexFormat.UInt32
            };
            combined.CombineMeshes(instances.ToArray(), true, true, false);
            combined.RecalculateBounds();

            GameObject battlement = new GameObject(prefix + " sandbag battlement - optimized mesh");
            battlement.transform.SetParent(parent, false);
            battlement.AddComponent<MeshFilter>().sharedMesh = combined;
            battlement.AddComponent<MeshRenderer>().sharedMaterial = sandbag;
            MeshCollider collider = battlement.AddComponent<MeshCollider>();
            collider.sharedMesh = combined;
            battlement.AddComponent<BallisticCover>().Initialize(2400f);
        }

        private static void CreateSandbag(Transform parent, Vector3 position, float yaw, string label)
        {
            GameObject bag = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bag.name = label;
            bag.transform.SetParent(parent, true);
            bag.transform.position = position;
            bag.transform.rotation = Quaternion.Euler(0f, yaw, 90f);
            bag.transform.localScale = new Vector3(0.34f, 0.82f, 0.36f);
            bag.GetComponent<Renderer>().sharedMaterial = sandbag;
            Collider col = bag.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;
            bag.AddComponent<BallisticCover>().Initialize(48f);
        }

        private static void BuildMidfieldCover(Transform parent)
        {
            for (int row = 0; row < 3; row++)
            {
                float z = 112f - row * 112f;
                for (int i = 0; i < 8; i++)
                {
                    float x = -112f + i * 32f + Mathf.Sin(i * 2.2f + row) * 8f;
                    float y = HeightAt(x, z);
                    Material material = (i + row) % 2 == 0 ? stone : brick;
                    CreateBox(parent, new Vector3(x, y + 0.68f, z),
                        new Vector3(4.8f + (i % 3), 1.35f, 0.75f), material,
                        "Broken midfield firing cover", Mathf.Sin(i * 1.3f) * 8f);
                }
            }

            // Two low firing positions and a shallow approach wall help break up the open ground.
            for (int i = 0; i < 10; i++)
            {
                float x = -90f + i * 20f;
                float z = 250f + Mathf.Sin(i * 0.8f) * 5f;
                CreateSandbag(parent, new Vector3(x, HeightAt(x, z) + 0.35f, z), Mathf.Sin(i) * 4f, "Attacker forward cover");
            }
        }

        private static void BuildBuildings(Transform parent)
        {
            BuildHouse(parent, new Vector3(-94f, HeightAt(-94f, -218f), -218f), "West defensive house");
            BuildHouse(parent, new Vector3(100f, HeightAt(100f, -186f), -186f), "East defensive house");
        }

        private static void BuildHouse(Transform parent, Vector3 groundPosition, string name)
        {
            Transform house = new GameObject(name + " - masonry shell").transform;
            house.SetParent(parent, false);
            house.position = groundPosition;
            float width = 22f, depth = 15f, height = 3.8f, wall = 0.65f;
            CreateBox(house, new Vector3(0f, height * 0.5f, -depth * 0.5f), new Vector3(width, height, wall), brick, "Rear masonry wall", 0f);
            CreateBox(house, new Vector3(-width * 0.5f, height * 0.5f, 0f), new Vector3(wall, height, depth), brick, "West masonry wall", 0f);
            CreateBox(house, new Vector3(width * 0.5f, height * 0.5f, 0f), new Vector3(wall, height, depth), brick, "East masonry wall", 0f);
            CreateBox(house, new Vector3(0f, height * 0.5f, depth * 0.5f), new Vector3(width, height, wall), brick, "Front masonry wall", 0f);
            CreateBox(house, new Vector3(0f, 0.16f, 0f), new Vector3(width, 0.32f, depth), stone, "Stone floor", 0f);
            CreateBox(house, new Vector3(-5.7f, 4.2f, 0f), new Vector3(12.7f, 0.5f, depth + 1f), roof, "Left pitched roof", -28f);
            CreateBox(house, new Vector3(5.7f, 4.2f, 0f), new Vector3(12.7f, 0.5f, depth + 1f), roof, "Right pitched roof", 28f);
            CreateBox(house, new Vector3(0f, 2.0f, depth * 0.5f + 0.08f), new Vector3(2.0f, 3.5f, 0.18f), darkMetal, "Front doorway", 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                CreateBox(house, new Vector3(side * 6.2f, 2.25f, depth * 0.5f + 0.12f),
                    new Vector3(2.0f, 1.3f, 0.14f), darkMetal, "Dark window recess", 0f);
            }
        }

        private static void ScatterRockOutcrops(Transform parent)
        {
            GameObject source = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh rockMesh = source.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(source);
            List<CombineInstance> batches = new List<CombineInstance>(420);
            System.Random random = new System.Random(11092026);
            for (int i = 0; i < 420; i++)
            {
                float x = (float)(random.NextDouble() * 650.0 - 325.0);
                float z = (float)(random.NextDouble() * 650.0 - 325.0);
                if (Mathf.Abs(x) < 24f && z > 255f) continue;
                float baseScale = 0.28f + (float)random.NextDouble() * 1.35f;
                Vector3 scale = new Vector3(baseScale * (0.75f + (float)random.NextDouble()), baseScale * 0.42f, baseScale);
                Vector3 pos = new Vector3(x, HeightAt(x, z) + scale.y * 0.32f, z);
                Quaternion rotation = Quaternion.Euler(
                    (float)random.NextDouble() * 32f,
                    (float)random.NextDouble() * 360f,
                    (float)random.NextDouble() * 25f);
                batches.Add(new CombineInstance
                {
                    mesh = rockMesh,
                    transform = Matrix4x4.TRS(pos, rotation, scale)
                });
            }

            Mesh combined = new Mesh { name = "Low-profile rocky outcrops", indexFormat = IndexFormat.UInt32 };
            combined.CombineMeshes(batches.ToArray(), true, true);
            GameObject rocks = new GameObject("Sparse low-profile rock outcrops");
            rocks.transform.SetParent(parent, false);
            rocks.AddComponent<MeshFilter>().sharedMesh = combined;
            rocks.AddComponent<MeshRenderer>().sharedMaterial = stone;
        }

        private static void PlaceDistantDefenders(Transform parent)
        {
            float[] positions = { -118f, -66f, 16f, 58f, 132f };
            for (int i = 0; i < positions.Length; i++)
            {
                float x = positions[i];
                float z = -192f + Mathf.Sin(i * 1.7f) * 8f;
                float y = HeightAt(x, z);
                Transform group = new GameObject("Distant defender silhouette " + (i + 1)).transform;
                group.SetParent(parent, false);
                group.position = new Vector3(x, y, z);
                CreatePrimitivePart(PrimitiveType.Capsule, group, new Vector3(0f, 0.76f, 0f),
                    new Vector3(0.32f, 0.57f, 0.23f), soldier, "Uniform silhouette");
                CreatePrimitivePart(PrimitiveType.Sphere, group, new Vector3(0f, 1.52f, 0f),
                    new Vector3(0.22f, 0.25f, 0.22f), soldier, "Head silhouette");
                CreatePrimitivePart(PrimitiveType.Cube, group, new Vector3(0.25f, 1.08f, 0.24f),
                    new Vector3(0.68f, 0.075f, 0.075f), darkMetal, "Rifle silhouette");
            }
        }

        private static void CreatePrimitivePart(PrimitiveType type, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material, string label)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = label;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
        }

        private static void CreateBox(Transform parent, Vector3 position, Vector3 size,
            Material material, string label, float yaw)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = label;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            box.AddComponent<BallisticCover>().Initialize(120f);
        }
    }
}
