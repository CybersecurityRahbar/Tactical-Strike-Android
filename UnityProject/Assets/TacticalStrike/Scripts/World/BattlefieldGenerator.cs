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
            BuildForestAndGrass(parent);
            PlaceDistantDefenders(parent);
        }

        public static float HeightAt(float x, float z)
        {
            // A broad green valley with raised mountain shoulders. The central
            // north/south engagement lane stays readable while both flanks rise.
            float macro = Fbm(x * 0.0042f, z * 0.0042f, 20261108, 5) * 5.8f;
            float rolling = Fbm(x * 0.018f, z * 0.018f, 20261109, 4) * 4.3f;
            float fine = Noise(x * 0.09f, z * 0.09f, 741209) * 0.48f;
            float westSpine = 43f * Mathf.Exp(-Mathf.Pow(
                (x + 228f + Mathf.Sin(z * 0.008f) * 28f) / 72f, 2f));
            float eastSpine = 37f * Mathf.Exp(-Mathf.Pow(
                (x - 236f + Mathf.Sin(z * 0.007f) * 31f) / 82f, 2f));
            float northernShoulder = 17f * Mathf.Exp(-Mathf.Pow((z + 292f) / 61f, 2f));
            float southernRise = 12f * Mathf.Exp(-Mathf.Pow((z - 314f) / 58f, 2f));
            float centralSaddle = -2.2f * Mathf.Exp(-Mathf.Pow(z / 68f, 2f));
            float height = macro + rolling + fine + westSpine + eastSpine +
                           northernShoulder + southernRise + centralSaddle;

            float attackerTrench = Mathf.Abs(z - (205f + Mathf.Sin(x * 0.055f) * 2.2f));
            float defenderTrench = Mathf.Abs(z - (-205f + Mathf.Sin(x * 0.045f) * 2.4f));
            if (attackerTrench < 3.6f)
                height -= (1f - attackerTrench / 3.6f) * 1.8f;
            if (defenderTrench < 3.6f)
                height -= (1f - defenderTrench / 3.6f) * 1.9f;
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
            earth = new Material(lit)
            {
                name = "Greenridge - layered sparse grass PBR",
                color = new Color(0.88f, 0.97f, 0.82f)
            };
            sandbag = MakeMaterial(new Color(0.48f, 0.48f, 0.32f), "Olive canvas sandbags");
            brick = MakeMaterial(new Color(0.49f, 0.39f, 0.31f), "Weathered village brick");
            stone = MakeMaterial(new Color(0.39f, 0.43f, 0.39f), "Mossed field stone");
            darkMetal = MakeMaterial(new Color(0.075f, 0.09f, 0.075f), "Field gunmetal");
            soldier = MakeMaterial(new Color(0.11f, 0.16f, 0.10f), "Distant olive uniform");
            roof = MakeMaterial(new Color(0.19f, 0.22f, 0.18f), "Weathered green-grey roof");

            Texture2D diffuse = Resources.Load<Texture2D>("TacticalStrike/Imported/Terrain/sparse_grass_diff_1k");
            Texture2D normal = Resources.Load<Texture2D>("TacticalStrike/Imported/Terrain/sparse_grass_nor_gl_1k");
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
            if (earth.HasProperty("_BaseColor"))
                earth.SetColor("_BaseColor", new Color(0.92f, 1f, 0.86f));
            earth.SetFloat("_Smoothness", 0.20f);
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
                    uv[vertexIndex] = new Vector2(wx * 0.055f, wz * 0.055f);
                    float shade = Noise(wx * 0.22f, wz * 0.22f, 741208) * 0.055f;
                    colors[vertexIndex] = new Color(
                        Mathf.Clamp(0.82f + shade, 0.68f, 0.94f),
                        Mathf.Clamp(0.93f + shade, 0.78f, 1f),
                        Mathf.Clamp(0.78f + shade * 0.72f, 0.63f, 0.91f), 1f);
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

            Mesh mesh = new Mesh { name = "Greenridge Valley 700m - rolling mountain terrain", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject terrain = new GameObject("Terrain - Greenridge mountain valley");
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
            // Both trench lines face their opposing sector: attackers face -Z, defenders face +Z.
            float enemyDirection = defenders ? 1f : -1f;
            Mesh capsuleMesh = GetSandbagSourceMesh();
            List<CombineInstance> bagInstances = new List<CombineInstance>(360);

            for (float x = -148f; x <= 148f; x += 2.15f)
            {
                float zOffset = Mathf.Sin(x * 0.045f) * 2.4f;
                float ground = HeightAt(x, lineZ + zOffset);
                if (Mathf.Abs(x) > 132f && Mathf.Sin(x * 0.11f) > 0.45f) continue;

                AddSandbagInstance(bagInstances, capsuleMesh, parent,
                    new Vector3(x, ground + 0.36f, lineZ + zOffset + enemyDirection * 5.5f),
                    Mathf.Sin(x * 0.07f) * 3f, new Vector3(0.34f, 0.82f, 0.36f));

                if ((Mathf.Abs(x) % 4.3f) < 2.15f)
                    AddSandbagInstance(bagInstances, capsuleMesh, parent,
                        new Vector3(x + 0.32f, ground + 0.72f, lineZ + zOffset + enemyDirection * 5.45f),
                        Mathf.Sin(x * 0.05f) * 2.4f, new Vector3(0.34f, 0.82f, 0.36f));

                if (Mathf.Abs(x) < 125f && ((int)(x + 150f) % 17 == 0))
                    CreateBox(parent, new Vector3(x, ground + 0.55f, lineZ + zOffset - enemyDirection * 5.1f),
                        new Vector3(3.2f, 1.1f, 0.55f), wallMaterial, prefix + " firing wall", 90f);
            }

            for (float x = -132f; x <= 132f; x += 2.4f)
            {
                float z = lineZ + Mathf.Sin(x * 0.045f) * 2.4f - enemyDirection * 5f;
                float y = HeightAt(x, z);
                AddSandbagInstance(bagInstances, capsuleMesh, parent,
                    new Vector3(x, y + 0.34f, z - enemyDirection * 7.6f),
                    Mathf.Sin(x * 0.04f) * 2f, new Vector3(0.34f, 0.82f, 0.36f));
            }

            BuildSandbagBatch(parent, prefix, bagInstances);

            for (int i = 0; i < 12; i++)
            {
                float x = -120f + i * 22f;
                float z = lineZ + Mathf.Sin(x * 0.045f) * 2.4f;
                CreateBox(parent, new Vector3(x, HeightAt(x, z) + 0.6f, z - enemyDirection * 12f),
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
            // Alternating lateral firing pockets preserve an open centre lane.
            Vector3[] positions =
            {
                new Vector3(-116f, 0f, 142f), new Vector3(-66f, 0f, 128f),
                new Vector3(75f, 0f, 136f), new Vector3(123f, 0f, 118f),
                new Vector3(-138f, 0f, 48f), new Vector3(-92f, 0f, 34f),
                new Vector3(96f, 0f, 54f), new Vector3(145f, 0f, 32f),
                new Vector3(-122f, 0f, -58f), new Vector3(-72f, 0f, -76f),
                new Vector3(82f, 0f, -48f), new Vector3(132f, 0f, -72f),
                new Vector3(-145f, 0f, -142f), new Vector3(-92f, 0f, -128f),
                new Vector3(92f, 0f, -136f), new Vector3(146f, 0f, -150f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 p = positions[i];
                float y = HeightAt(p.x, p.z);
                Material material = i % 3 == 0 ? brick : stone;
                float yaw = Mathf.Sin(i * 1.73f) * 21f;
                Vector3 size = new Vector3(4.0f + (i % 4) * 0.85f,
                    1.05f + (i % 3) * 0.16f, 0.55f);
                CreateBox(parent, new Vector3(p.x, y + size.y * 0.5f, p.z),
                    size, material, "Staggered field firing barrier", yaw);
            }

            // Short olive sandbag nests protect the two forward shoulders.
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    float x = side * (116f + i * 5.4f);
                    float z = 225f + Mathf.Sin(i * 0.9f + side) * 4f;
                    CreateSandbag(parent, new Vector3(x, HeightAt(x, z) + 0.34f, z),
                        side * (i * 3.1f), "Forward shoulder sandbag nest");
                }
            }
        }

        private static void BuildBuildings(Transform parent)
        {
            BuildHouse(parent, new Vector3(-94f, HeightAt(-94f, -218f), -218f), "West defensive house");
            BuildHouse(parent, new Vector3(100f, HeightAt(100f, -186f), -186f), "East defensive house");
            BuildHouse(parent, new Vector3(-164f, HeightAt(-164f, -92f), -92f), "West village farmhouse");
            BuildHouse(parent, new Vector3(170f, HeightAt(170f, -118f), -118f), "East village farmhouse");
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

        private static void BuildForestAndGrass(Transform parent)
        {
            GameObject[] trees =
            {
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-pinerounda"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-pineroundb"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-oak"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-default")
            };
            GameObject bush = Resources.Load<GameObject>("TacticalStrike/Imported/Nature/plant-bushdetailed");
            GameObject grass = Resources.Load<GameObject>("TacticalStrike/Imported/Nature/grass-large");
            bool hasTrees = false;
            foreach (GameObject tree in trees) if (tree != null) hasTrees = true;
            if (!hasTrees) return;

            System.Random random = new System.Random(20261110);
            int placed = 0;
            for (int i = 0; i < 280; i++)
            {
                float side = random.Next(0, 2) == 0 ? -1f : 1f;
                float x = side * (112f + (float)random.NextDouble() * 214f);
                float z = (float)random.NextDouble() * 640f - 320f;
                if (Mathf.Abs(z - 205f) < 22f || Mathf.Abs(z + 205f) < 24f) continue;
                if (Mathf.Abs(x + 94f) < 29f && Mathf.Abs(z + 218f) < 27f) continue;
                if (Mathf.Abs(x - 100f) < 31f && Mathf.Abs(z + 186f) < 28f) continue;
                if (Mathf.Abs(x + 164f) < 30f && Mathf.Abs(z + 92f) < 29f) continue;
                if (Mathf.Abs(x - 170f) < 32f && Mathf.Abs(z + 118f) < 30f) continue;
                if (Mathf.Abs(x) < 128f && z > 230f) continue;

                GameObject prefab = trees[random.Next(0, trees.Length)];
                if (prefab == null)
                {
                    for (int j = 0; j < trees.Length; j++)
                        if (trees[j] != null) { prefab = trees[j]; break; }
                }
                if (prefab == null) continue;

                GameObject treeObject = Object.Instantiate(prefab,
                    new Vector3(x, HeightAt(x, z), z),
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);
                float scale = 3.7f + (float)random.NextDouble() * 1.7f;
                treeObject.transform.localScale = Vector3.one * scale;
                treeObject.name = "Field tree - " + prefab.name;
                foreach (Renderer renderer in treeObject.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.shadowCastingMode = Mathf.Abs(x) < 205f
                        ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    renderer.receiveShadows = true;
                    foreach (Material material in renderer.sharedMaterials)
                        if (material != null) material.enableInstancing = true;
                }
                if (treeObject.GetComponentInChildren<Collider>() == null)
                {
                    CapsuleCollider trunk = treeObject.AddComponent<CapsuleCollider>();
                    trunk.center = new Vector3(0f, 0.58f, 0f);
                    trunk.height = 1.16f;
                    trunk.radius = 0.105f;
                }
                placed++;

                if (bush != null && i % 2 == 0)
                {
                    float bx = x + (float)random.NextDouble() * 12f - 6f;
                    float bz = z + (float)random.NextDouble() * 12f - 6f;
                    GameObject bushObject = Object.Instantiate(bush,
                        new Vector3(bx, HeightAt(bx, bz), bz),
                        Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);
                    bushObject.transform.localScale = Vector3.one * (1.6f + (float)random.NextDouble() * 1.15f);
                    bushObject.name = "Field shrub";
                }
            }

            if (grass != null)
            {
                for (int i = 0; i < 44; i++)
                {
                    float x = (float)random.NextDouble() * 610f - 305f;
                    float z = (float)random.NextDouble() * 610f - 305f;
                    if (Mathf.Abs(x) < 75f && z > -160f && z < 175f) continue;
                    GameObject patch = Object.Instantiate(grass,
                        new Vector3(x, HeightAt(x, z) - 0.02f, z),
                        Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);
                    patch.transform.localScale = Vector3.one * (1.25f + (float)random.NextDouble() * 0.85f);
                    patch.name = "Ground grass clump";
                    foreach (Renderer renderer in patch.GetComponentsInChildren<Renderer>(true))
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            Debug.Log("Greenridge nature dressing placed: " + placed + " trees, shrubs and grass patches.");
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
