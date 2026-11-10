using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalStrike
{
    /// <summary>
    /// Runtime-authored Northwind Valley battlefield. Coordinates follow the plan:
    /// attackers deploy to the south (+Z), defenders hold the north (-Z).
    /// The terrain, carved communications trenches, compounds and movement lanes
    /// are actual collidable game geometry, not a top-down illustration.
    /// </summary>
    public static class NorthwindValleyGenerator
    {
        public const float MapSize = 700f;
        private static Material soilMaterial;
        private static Material trenchSoilMaterial;
        private static Material sandbagMaterial;
        private static Material masonryMaterial;
        private static Material concreteMaterial;
        private static Material roofMaterial;
        private static Material roadMaterial;
        private static Material waterMaterial;
        private static Material metalMaterial;
        private static Material foliageMaterial;

        private static readonly Vector2[][] DefenderNetwork =
        {
            new[] {
                new Vector2(-326,-235), new Vector2(-292,-224), new Vector2(-258,-232),
                new Vector2(-225,-220), new Vector2(-192,-229), new Vector2(-158,-216),
                new Vector2(-124,-226), new Vector2(-91,-214), new Vector2(-57,-222),
                new Vector2(-23,-211), new Vector2(13,-222), new Vector2(49,-212),
                new Vector2(84,-225), new Vector2(120,-214), new Vector2(156,-228),
                new Vector2(192,-216), new Vector2(228,-231), new Vector2(264,-219),
                new Vector2(300,-235), new Vector2(328,-228)
            },
            new[] { new Vector2(-258,-232), new Vector2(-267,-259), new Vector2(-244,-281), new Vector2(-211,-296) },
            new[] { new Vector2(-124,-226), new Vector2(-145,-252), new Vector2(-128,-278), new Vector2(-91,-293) },
            new[] { new Vector2(-23,-211), new Vector2(-42,-242), new Vector2(-21,-266), new Vector2(14,-285) },
            new[] { new Vector2(120,-214), new Vector2(145,-244), new Vector2(137,-273), new Vector2(167,-298) },
            new[] { new Vector2(228,-231), new Vector2(252,-257), new Vector2(239,-284), new Vector2(270,-302) },
            new[] { new Vector2(-290,-224), new Vector2(-305,-198), new Vector2(-282,-176), new Vector2(-250,-166) },
            new[] { new Vector2(290,-228), new Vector2(310,-201), new Vector2(286,-178), new Vector2(252,-166) }
        };

        private static readonly Vector2[][] AttackerNetwork =
        {
            new[] {
                new Vector2(-285,224), new Vector2(-250,214), new Vector2(-214,225),
                new Vector2(-178,213), new Vector2(-142,225), new Vector2(-106,212),
                new Vector2(-70,224), new Vector2(-34,213), new Vector2(2,225),
                new Vector2(38,213), new Vector2(74,224), new Vector2(110,212),
                new Vector2(146,225), new Vector2(182,213), new Vector2(218,225),
                new Vector2(254,214), new Vector2(286,226)
            },
            new[] { new Vector2(-214,225), new Vector2(-228,251), new Vector2(-204,277) },
            new[] { new Vector2(-34,213), new Vector2(-12,246), new Vector2(18,265) },
            new[] { new Vector2(182,213), new Vector2(202,244), new Vector2(184,273) }
        };

        private static readonly Vector2[][] DrainageNetwork =
        {
            new[] { new Vector2(-282,160), new Vector2(-257,112), new Vector2(-270,65), new Vector2(-239,18), new Vector2(-223,-44), new Vector2(-248,-91), new Vector2(-236,-146) },
            new[] { new Vector2(280,176), new Vector2(251,126), new Vector2(270,71), new Vector2(243,22), new Vector2(255,-41), new Vector2(228,-97), new Vector2(246,-150) }
        };

        public static void Build(Transform parent)
        {
            CreateMaterials();
            BuildTerrain(parent);
            BuildGroundPatches(parent);
            BuildTrenchNetwork(parent, DefenderNetwork, true);
            BuildTrenchNetwork(parent, AttackerNetwork, false);
            BuildDugouts(parent);
            BuildRoadNetwork(parent);
            BuildDrainageAndStream(parent);
            BuildDefensiveCompounds(parent);
            BuildVillageAndFarm(parent);
            BuildMountedWeaponPositions(parent);
            BuildWoodlandBelts(parent);
            BuildRidgeRocks(parent);
            BuildFieldDetails(parent);
            BuildBattlefieldDressing(parent);
            Debug.Log("NORTHWIND VALLEY BUILT: 700m x 700m terrain, continuous roads/trenches, defensive compounds and authored CC0 battlefield props.");
        }

        public static float HeightAt(float x, float z)
        {
            float macro = Fbm(x * 0.0036f, z * 0.0036f, 20261110, 5) * 7.0f;
            float rolling = Fbm(x * 0.014f, z * 0.014f, 20261111, 4) * 3.8f;
            float fine = Noise(x * 0.065f, z * 0.065f, 741209) * 0.68f;

            float westRidge = 42f * Mathf.Exp(-Square((x + 247f + Mathf.Sin(z * 0.008f) * 34f) / 74f));
            float eastRidge = 38f * Mathf.Exp(-Square((x - 244f + Mathf.Sin(z * 0.007f) * 29f) / 81f));
            float northRidge = 20f * Mathf.Exp(-Square((z + 318f + Mathf.Sin(x * 0.008f) * 12f) / 48f));
            float southShoulder = 13f * Mathf.Exp(-Square((z - 324f) / 53f));
            float valleyFloor = -3.8f * Mathf.Exp(-Square(x / 112f));
            float centralSaddle = -2.2f * Mathf.Exp(-Square(z / 82f));
            float gullies = -2.5f * Mathf.Exp(-Square((x + 105f + z * 0.12f) / 24f))
                          -2.0f * Mathf.Exp(-Square((x - 158f - z * 0.08f) / 21f));
            float height = macro + rolling + fine + westRidge + eastRidge +
                           northRidge + southShoulder + valleyFloor + centralSaddle + gullies;

            // Recess the two flank drainage channels into the heightfield itself;
            // the water meshes then sit visibly in the cut channel, not under the grass.
            float drainageDistance = DrainageNetworkDistance(new Vector2(x, z));
            if (drainageDistance < 8f)
            {
                float t = 1f - drainageDistance / 8f;
                height -= 1.9f * t * t * (3f - 2f * t);
            }

            float trenchDistance = TrenchNetworkDistance(new Vector2(x, z));
            if (trenchDistance < 3.0f)
            {
                float t = 1f - trenchDistance / 3.0f;
                height -= 1.55f * t * t * (3f - 2f * t);
            }
            return height;
        }

        private static float Square(float value) { return value * value; }

        private static float Noise(float x, float y, int seed)
        {
            float ox = (seed % 997) * 0.173f;
            float oy = (seed % 613) * 0.271f;
            return (Mathf.PerlinNoise(x + ox, y + oy) - 0.5f) * 2f;
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

        private static float DrainageNetworkDistance(Vector2 point)
        {
            float best = 999999f;
            for (int p = 0; p < DrainageNetwork.Length; p++)
            {
                Vector2[] path = DrainageNetwork[p];
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Vector2 a = path[i];
                    Vector2 b = path[i + 1];
                    Vector2 ab = b - a;
                    float lengthSq = ab.sqrMagnitude;
                    float t = lengthSq > 0.0001f
                        ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq) : 0f;
                    float distanceSq = (point - (a + ab * t)).sqrMagnitude;
                    if (distanceSq < best) best = distanceSq;
                }
            }
            return Mathf.Sqrt(best);
        }

        private static float TrenchNetworkDistance(Vector2 point)
        {
            float best = 999999f;
            best = NetworkDistance(point, DefenderNetwork, best);
            best = NetworkDistance(point, AttackerNetwork, best);
            return Mathf.Sqrt(best);
        }

        private static float NetworkDistance(Vector2 point, Vector2[][] paths, float best)
        {
            for (int p = 0; p < paths.Length; p++)
            {
                Vector2[] path = paths[p];
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Vector2 a = path[i];
                    Vector2 b = path[i + 1];
                    Vector2 ab = b - a;
                    float lengthSq = ab.sqrMagnitude;
                    float t = lengthSq > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq) : 0f;
                    float d = (point - (a + ab * t)).sqrMagnitude;
                    if (d < best) best = d;
                }
            }
            return best;
        }

        private static void CreateMaterials()
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");

            soilMaterial = MakeMaterial(lit, new Color(0.78f, 0.72f, 0.62f), "Northwind - exposed field earth PBR");
            trenchSoilMaterial = MakeMaterial(lit, new Color(0.53f, 0.48f, 0.40f), "Northwind - disturbed trench earth PBR");
            sandbagMaterial = MakeMaterial(lit, new Color(0.43f, 0.42f, 0.30f), "Northwind - weathered olive canvas sandbags");
            masonryMaterial = MakeMaterial(lit, new Color(0.77f, 0.73f, 0.67f), "Northwind - weathered brick masonry PBR");
            concreteMaterial = MakeMaterial(lit, new Color(0.82f, 0.81f, 0.76f), "Northwind - chipped concrete PBR");
            roofMaterial = MakeMaterial(lit, new Color(0.46f, 0.47f, 0.43f), "Northwind - weathered slate and roof PBR");
            roadMaterial = MakeMaterial(lit, new Color(0.77f, 0.73f, 0.66f), "Northwind - compacted road PBR");
            waterMaterial = MakeMaterial(lit, new Color(0.13f, 0.22f, 0.23f), "Northwind - drainage water");
            metalMaterial = MakeMaterial(lit, new Color(0.075f, 0.085f, 0.078f), "Northwind - dark gunmetal");
            foliageMaterial = MakeMaterial(lit, new Color(0.21f, 0.28f, 0.19f), "Northwind - woodland undergrowth");

            ApplySurfaceTextures(soilMaterial, "ground", new Vector2(1.7f, 1.7f), 0.12f);
            ApplySurfaceTextures(trenchSoilMaterial, "ground", new Vector2(2.4f, 2.4f), 0.10f);
            ApplySurfaceTextures(masonryMaterial, "brick", new Vector2(2.1f, 1.6f), 0.18f);
            ApplySurfaceTextures(concreteMaterial, "concrete", new Vector2(1.8f, 1.8f), 0.20f);
            ApplySurfaceTextures(roofMaterial, "concrete", new Vector2(2.3f, 2.0f), 0.12f);
            ApplySurfaceTextures(roadMaterial, "road", new Vector2(3.2f, 1.7f), 0.10f);
        }

        private static void ApplySurfaceTextures(Material material, string surface,
            Vector2 tiling, float smoothness)
        {
            string resourceBase = "TacticalStrike/Imported/Terrain/northwind_" + surface;
            Texture2D diffuse = Resources.Load<Texture2D>(resourceBase + "_diff_1k");
            Texture2D normal = Resources.Load<Texture2D>(resourceBase + "_nor_gl_1k");

            if (diffuse != null)
            {
                diffuse.wrapMode = TextureWrapMode.Repeat;
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", diffuse);
                    material.SetTextureScale("_BaseMap", tiling);
                }
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", diffuse);
                    material.SetTextureScale("_MainTex", tiling);
                }
            }

            if (normal != null && material.HasProperty("_BumpMap"))
            {
                normal.wrapMode = TextureWrapMode.Repeat;
                material.SetTexture("_BumpMap", normal);
                material.SetTextureScale("_BumpMap", tiling);
                material.EnableKeyword("_NORMALMAP");
            }

            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        }

        private static Material MakeMaterial(Shader shader, Color color, string name)
        {
            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.18f);
            material.enableInstancing = true;
            return material;
        }

        private static TerrainLayer CreateTerrainLayer(string label, string diffuseResource,
            string normalResource, Vector2 tileSize)
        {
            Texture2D diffuse = Resources.Load<Texture2D>(diffuseResource);
            Texture2D normal = Resources.Load<Texture2D>(normalResource);
            if (diffuse == null || normal == null)
                throw new System.InvalidOperationException(
                    "Northwind terrain layer is missing a prepared PBR texture: " + label);

            TerrainLayer layer = new TerrainLayer();
            layer.name = label;
            layer.diffuseTexture = diffuse;
            layer.normalMapTexture = normal;
            layer.tileSize = tileSize;
            layer.tileOffset = Vector2.zero;
            layer.metallic = 0f;
            layer.smoothness = 0.12f;
            return layer;
        }

        private static void BuildTerrain(Transform parent)
        {
            // 513 height samples reduce spacing to about 1.37m across 700m.
            const int heightResolution = 513;
            const int alphaResolution = 512;
            const float baseHeight = -25f;
            const float verticalRange = 120f;
            float half = MapSize * 0.5f;
            float heightStep = MapSize / (heightResolution - 1);

            // Unity Terrain provides a native tiled heightfield collider and proper
            // splat-map blending instead of a single green mesh material.
            float[,] heights = new float[heightResolution, heightResolution];
            for (int z = 0; z < heightResolution; z++)
            {
                float worldZ = -half + z * heightStep;
                for (int x = 0; x < heightResolution; x++)
                {
                    float worldX = -half + x * heightStep;
                    float y = HeightAt(worldX, worldZ);
                    heights[z, x] = Mathf.Clamp01((y - baseHeight) / verticalRange);
                }
            }

            TerrainData data = new TerrainData
            {
                heightmapResolution = heightResolution,
                alphamapResolution = alphaResolution,
                size = new Vector3(MapSize, verticalRange, MapSize),
                baseMapResolution = 1024
            };
            data.name = "Northwind Valley - 700m sculpted highland TerrainData";
            data.SetHeights(0, 0, heights);

            TerrainLayer grassLayer = CreateTerrainLayer(
                "Northwind - Highland Grass",
                "TacticalStrike/Imported/Terrain/northwind_grass_diff_1k",
                "TacticalStrike/Imported/Terrain/northwind_grass_nor_gl_1k",
                new Vector2(24f, 24f));
            TerrainLayer earthLayer = CreateTerrainLayer(
                "Northwind - Exposed Earth",
                "TacticalStrike/Imported/Terrain/northwind_ground_diff_1k",
                "TacticalStrike/Imported/Terrain/northwind_ground_nor_gl_1k",
                new Vector2(17f, 17f));
            TerrainLayer rockLayer = CreateTerrainLayer(
                "Northwind - Rocky Gravel",
                "TacticalStrike/Imported/Terrain/rocky_gravel_diff_1k",
                "TacticalStrike/Imported/Terrain/rocky_gravel_nor_gl_1k",
                new Vector2(11f, 11f));
            data.terrainLayers = new[] { grassLayer, earthLayer, rockLayer };

            // Paint the terrain procedurally by slope, height and broad natural variation.
            // Ridge faces expose gravel; low valley shoulders and drainage lines expose
            // earth; the gentler basin remains grass. Weights are normalized per splat texel.
            float[,,] alpha = new float[alphaResolution, alphaResolution, 3];
            float alphaStep = MapSize / (alphaResolution - 1);
            for (int z = 0; z < alphaResolution; z++)
            {
                float worldZ = -half + z * alphaStep;
                int hz = Mathf.Clamp(Mathf.RoundToInt(z / (float)(alphaResolution - 1) *
                    (heightResolution - 1)), 1, heightResolution - 2);
                for (int x = 0; x < alphaResolution; x++)
                {
                    float worldX = -half + x * alphaStep;
                    int hx = Mathf.Clamp(Mathf.RoundToInt(x / (float)(alphaResolution - 1) *
                        (heightResolution - 1)), 1, heightResolution - 2);

                    float centerHeight = baseHeight + heights[hz, hx] * verticalRange;
                    float left = baseHeight + heights[hz, hx - 1] * verticalRange;
                    float right = baseHeight + heights[hz, hx + 1] * verticalRange;
                    float back = baseHeight + heights[hz - 1, hx] * verticalRange;
                    float forward = baseHeight + heights[hz + 1, hx] * verticalRange;
                    float dx = (right - left) / (2f * heightStep);
                    float dz = (forward - back) / (2f * heightStep);
                    float slope = Mathf.Sqrt(dx * dx + dz * dz);

                    float rock = Mathf.SmoothStep(0.24f, 0.78f, slope) * 0.92f;
                    float lowland = Mathf.Clamp01((13f - centerHeight) / 42f);
                    float variation = Mathf.PerlinNoise(worldX * 0.009f + 47.3f,
                        worldZ * 0.009f + 19.7f);
                    float earth = Mathf.Clamp01(0.13f + lowland * 0.34f +
                        (0.42f - variation) * 0.25f);
                    earth *= 1f - rock;
                    float grass = Mathf.Max(0.025f, 1f - rock - earth);
                    float sum = grass + earth + rock;

                    alpha[z, x, 0] = grass / sum;
                    alpha[z, x, 1] = earth / sum;
                    alpha[z, x, 2] = rock / sum;
                }
            }
            data.SetAlphamaps(0, 0, alpha);

            GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "01 - Northwind Valley playable multi-layer terrain";
            terrainObject.transform.SetParent(parent, false);
            terrainObject.transform.localPosition = new Vector3(-half, baseHeight, -half);
            Terrain terrain = terrainObject.GetComponent<Terrain>();
            TerrainCollider terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            if (terrainCollider == null)
                terrainCollider = terrainObject.AddComponent<TerrainCollider>();
            terrainCollider.terrainData = data;
            terrainCollider.enabled = true;
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 2.5f;
            terrain.basemapDistance = 900f;
            terrain.detailObjectDistance = 65f;
            terrain.treeDistance = 700f;
            terrain.allowAutoConnect = false;
        }



        private static void BuildGroundPatches(Transform parent)
        {
            CreateGroundPatch(parent, new Vector2(0f, 300f), new Vector2(172f, 48f),
                "Southern attacker assembly - churned earth and vehicle staging");
            CreateGroundPatch(parent, new Vector2(0f, -304f), new Vector2(148f, 28f),
                "Northern defender reserve - disturbed field earth");
            CreateGroundPatch(parent, new Vector2(-112f, -168f), new Vector2(42f, 40f),
                "West defensive compound - exposed courtyard earth");
            CreateGroundPatch(parent, new Vector2(116f, -174f), new Vector2(42f, 40f),
                "East defensive compound - exposed courtyard earth");
            CreateGroundPatch(parent, new Vector2(-190f, -54f), new Vector2(38f, 36f),
                "West farmstead - packed yard earth");
            CreateGroundPatch(parent, new Vector2(184f, -70f), new Vector2(38f, 36f),
                "East village - packed yard earth");
            CreateGroundPatch(parent, new Vector2(-207f, -201f), new Vector2(24f, 18f),
                "West heavy machine gun - scraped firing position");
            CreateGroundPatch(parent, new Vector2(207f, -201f), new Vector2(24f, 18f),
                "East heavy machine gun - scraped firing position");
        }

        private static void CreateGroundPatch(Transform parent, Vector2 center,
            Vector2 size, string label)
        {
            const float sampleSpacing = 3.0f;
            int segmentsX = Mathf.Max(2, Mathf.CeilToInt(size.x / sampleSpacing));
            int segmentsZ = Mathf.Max(2, Mathf.CeilToInt(size.y / sampleSpacing));
            int sideX = segmentsX + 1;
            int sideZ = segmentsZ + 1;
            Vector3[] vertices = new Vector3[sideX * sideZ];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[segmentsX * segmentsZ * 6];
            int v = 0;
            for (int z = 0; z < sideZ; z++)
            {
                float wz = center.y - size.y * 0.5f + size.y * z / segmentsZ;
                for (int x = 0; x < sideX; x++)
                {
                    float wx = center.x - size.x * 0.5f + size.x * x / segmentsX;
                    vertices[v] = parent.InverseTransformPoint(
                        new Vector3(wx, HeightAt(wx, wz) + 0.16f, wz));
                    uv[v] = new Vector2(wx * 0.055f, wz * 0.055f);
                    v++;
                }
            }
            int t = 0;
            for (int z = 0; z < segmentsZ; z++)
            for (int x = 0; x < segmentsX; x++)
            {
                int index = z * sideX + x;
                triangles[t++] = index;
                triangles[t++] = index + sideX;
                triangles[t++] = index + 1;
                triangles[t++] = index + 1;
                triangles[t++] = index + sideX;
                triangles[t++] = index + sideX + 1;
            }
            Mesh mesh = new Mesh { name = label + " - conforming PBR mesh" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GameObject patch = new GameObject(label);
            patch.transform.SetParent(parent, false);
            patch.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshCollider groundCollider = patch.AddComponent<MeshCollider>();
            groundCollider.sharedMesh = mesh;
            groundCollider.convex = false;
            MeshRenderer renderer = patch.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = soilMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }

        private static void BuildTrenchNetwork(Transform parent, Vector2[][] network, bool defenders)
        {
            Mesh cube = GetPrimitiveMesh(PrimitiveType.Cube);
            List<CombineInstance> parapets = new List<CombineInstance>(1200);
            List<CombineInstance> traverses = new List<CombineInstance>(480);
            string sideName = defenders ? "DEFENDER" : "ATTACKER";
            float facingSign = defenders ? 1f : -1f;

            for (int p = 0; p < network.Length; p++)
            {
                Vector2[] path = network[p];
                CreateConformingRibbon(parent, sideName + " trench floor " + (p + 1),
                    path, 4.65f, 0.075f, trenchSoilMaterial, false);

                for (int s = 0; s < path.Length - 1; s++)
                {
                    Vector2 a = path[s];
                    Vector2 b = path[s + 1];
                    Vector2 delta = b - a;
                    float length = delta.magnitude;
                    if (length < 0.1f) continue;
                    Vector2 direction2 = delta / length;
                    Vector3 direction = new Vector3(direction2.x, 0f, direction2.y).normalized;
                    Vector3 lateral3 = Vector3.Cross(direction, Vector3.up).normalized * facingSign;
                    Vector2 lateral = new Vector2(lateral3.x, lateral3.z);
                    int count = Mathf.Max(1, Mathf.CeilToInt(length / 2.2f));

                    for (int i = 0; i < count; i++)
                    {
                        float along = (i + 0.5f) * length / count;
                        Vector2 point = a + direction2 * along;
                        bool firingGap = (i % 10 == 8) || (p > 0 && i % 8 == 6);
                        float blockLength = Mathf.Min(2.15f, length / count + 0.16f);
                        if (!firingGap)
                        {
                            Vector2 front = point + lateral * 3.45f;
                            AddTerrainAlignedBlock(parapets, cube, parent, front, direction2,
                                new Vector3(0.72f, 0.92f, blockLength), 0.015f);
                        }

                        Vector2 rear = point - lateral * 3.0f;
                        AddTerrainAlignedBlock(traverses, cube, parent, rear, direction2,
                            new Vector3(0.58f, 0.52f, Mathf.Min(2.05f, blockLength)), 0.01f);
                    }
                }
            }

            CreateCombined(parent, sideName + " - continuous fighting trench parapets", parapets,
                sandbagMaterial, true, 4200f);
            CreateCombined(parent, sideName + " - communication trench traverses and rear revetments", traverses,
                trenchSoilMaterial, true, 2600f);

            for (int i = 0; i < 8; i++)
            {
                Vector2 point = defenders
                    ? new Vector2(-250f + i * 70f, -224f + Mathf.Sin(i * 1.1f) * 8f)
                    : new Vector2(-224f + i * 64f, 218f + Mathf.Sin(i * 1.2f) * 7f);
                BuildFireBay(parent, point, defenders);
            }
        }

        private static void AddTerrainAlignedBlock(List<CombineInstance> list, Mesh cube,
            Transform parent, Vector2 center, Vector2 direction, Vector3 size, float verticalOffset)
        {
            Vector2 along = direction.normalized;
            Vector2 start = center - along * (size.z * 0.5f);
            Vector2 end = center + along * (size.z * 0.5f);
            Vector3 startWorld = new Vector3(start.x, HeightAt(start.x, start.y), start.y);
            Vector3 endWorld = new Vector3(end.x, HeightAt(end.x, end.y), end.y);
            Vector3 tangent = (endWorld - startWorld).normalized;
            if (tangent.sqrMagnitude < 0.0001f) tangent = new Vector3(along.x, 0f, along.y);
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);
            Vector3 centerWorld = (startWorld + endWorld) * 0.5f +
                                  Vector3.up * (size.y * 0.5f + verticalOffset);
            AddCube(list, cube, parent, centerWorld, size, rotation);
        }

        private static void BuildFireBay(Transform parent, Vector2 point, bool defenders)
        {
            float forward = defenders ? 1f : -1f;
            float leftX = point.x - 2.2f;
            float rightX = point.x + 2.2f;
            float cheekZ = point.y + forward * 3.4f;
            CreateBox(parent, new Vector3(leftX, HeightAt(leftX, cheekZ) + 0.58f, cheekZ),
                new Vector3(1.0f, 1.15f, 1.0f), sandbagMaterial, Quaternion.identity, "Fighting bay - left cheek");
            CreateBox(parent, new Vector3(rightX, HeightAt(rightX, cheekZ) + 0.58f, cheekZ),
                new Vector3(1.0f, 1.15f, 1.0f), sandbagMaterial, Quaternion.identity, "Fighting bay - right cheek");
            float rearZ = point.y - forward * 2.3f;
            CreateBox(parent, new Vector3(point.x, HeightAt(point.x, rearZ) + 0.30f, rearZ),
                new Vector3(5.0f, 0.58f, 0.8f), trenchSoilMaterial, Quaternion.identity, "Fighting bay - rear firing step");
        }

        private static void BuildDugouts(Transform parent)
        {
            BuildDugout(parent, new Vector3(-211f, HeightAt(-211f, -296f) + 0.14f, -296f), "West reserve dugout");
            BuildDugout(parent, new Vector3(14f, HeightAt(14f, -286f) + 0.14f, -286f), "Central command dugout");
            BuildDugout(parent, new Vector3(267f, HeightAt(267f, -301f) + 0.14f, -301f), "East reserve dugout");
            // Keep the attacker assembly shelter to the flank. The previous position
            // sat almost directly in the player's forward sightline only ~38m from spawn.
            BuildDugout(parent, new Vector3(-132f, HeightAt(-132f, 266f) + 0.14f, 266f), "Attacker covered assembly shelter");
        }

        private static void BuildDugout(Transform parent, Vector3 ground, string label)
        {
            Transform root = new GameObject(label + " - timber and earth shelter").transform;
            root.SetParent(parent, false);
            root.position = ground;
            for (int side = -1; side <= 1; side += 2)
            {
                CreateBox(root, new Vector3(side * 2.1f, 0.95f, -0.1f),
                    new Vector3(0.55f, 1.9f, 6.4f), trenchSoilMaterial, Quaternion.identity, "Earth retaining wall");
                CreateBox(root, new Vector3(side * 2.1f, 1.98f, -0.1f),
                    new Vector3(0.22f, 0.20f, 6.4f), roofMaterial, Quaternion.identity, "Timber roof beam");
            }
            CreateBox(root, new Vector3(0f, 2.15f, -0.1f), new Vector3(4.8f, 0.38f, 7.0f),
                roofMaterial, Quaternion.identity, "Covered dugout roof");
            CreateBox(root, new Vector3(0f, 0.10f, -0.1f), new Vector3(4.1f, 0.18f, 6.0f),
                trenchSoilMaterial, Quaternion.identity, "Dugout floor");
        }

        private static void BuildRoadNetwork(Transform parent)
        {
            Vector2[][] paths =
            {
                new[] { new Vector2(244,350), new Vector2(225,300), new Vector2(192,246), new Vector2(208,190), new Vector2(235,132), new Vector2(216,72), new Vector2(231,12), new Vector2(208,-52), new Vector2(240,-112), new Vector2(230,-175), new Vector2(270,-236), new Vector2(285,-320) },
                new[] { new Vector2(-300,336), new Vector2(-266,284), new Vector2(-240,220), new Vector2(-252,158), new Vector2(-222,98), new Vector2(-190,40), new Vector2(-202,-28), new Vector2(-221,-92), new Vector2(-246,-156), new Vector2(-266,-226) },
                new[] { new Vector2(8,330), new Vector2(-12,278), new Vector2(14,220), new Vector2(28,160), new Vector2(4,102), new Vector2(-17,43), new Vector2(5,-18), new Vector2(-18,-80), new Vector2(3,-143), new Vector2(12,-190) }
            };
            for (int p = 0; p < paths.Length; p++)
                CreateConformingRibbon(parent, "Compacted service road " + (p + 1),
                    paths[p], p == 2 ? 4.8f : 6.5f, 0.055f, roadMaterial, true);
        }

        private static List<Vector2> SmoothPath(Vector2[] path, float spacing)
        {
            List<Vector2> samples = new List<Vector2>(path.Length * 8);
            if (path == null || path.Length == 0) return samples;
            if (path.Length == 1) { samples.Add(path[0]); return samples; }
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 p0 = path[Mathf.Max(0, i - 1)];
                Vector2 p1 = path[i];
                Vector2 p2 = path[i + 1];
                Vector2 p3 = path[Mathf.Min(path.Length - 1, i + 2)];
                int steps = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(p1, p2) /
                    Mathf.Max(0.25f, spacing)));
                for (int j = 0; j < steps; j++)
                {
                    float t = j / (float)steps;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    Vector2 point = 0.5f * (
                        2f * p1 +
                        (-p0 + p2) * t +
                        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                        (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                    if (samples.Count == 0 ||
                        (samples[samples.Count - 1] - point).sqrMagnitude > 0.0001f)
                        samples.Add(point);
                }
            }
            samples.Add(path[path.Length - 1]);
            return samples;
        }

        private static void CreateConformingRibbon(Transform parent, string label,
            Vector2[] controlPath, float width, float surfaceOffset, Material material,
            bool addCollider)
        {
            List<Vector2> samples = SmoothPath(controlPath, 1.8f);
            if (samples.Count < 2) return;
            Vector3[] vertices = new Vector3[samples.Count * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(samples.Count - 1) * 6];
            float travelled = 0f;
            for (int i = 0; i < samples.Count; i++)
            {
                Vector2 tangent;
                if (i == 0) tangent = (samples[1] - samples[0]).normalized;
                else if (i == samples.Count - 1) tangent = (samples[i] - samples[i - 1]).normalized;
                else tangent = (samples[i + 1] - samples[i - 1]).normalized;
                Vector2 lateral = new Vector2(-tangent.y, tangent.x);
                Vector2 left = samples[i] + lateral * (width * 0.5f);
                Vector2 right = samples[i] - lateral * (width * 0.5f);
                vertices[i * 2] = new Vector3(left.x, HeightAt(left.x, left.y) + surfaceOffset, left.y);
                vertices[i * 2 + 1] = new Vector3(right.x, HeightAt(right.x, right.y) + surfaceOffset, right.y);
                if (i > 0) travelled += Vector2.Distance(samples[i - 1], samples[i]);
                float v = travelled / 6f;
                uv[i * 2] = new Vector2(0f, v);
                uv[i * 2 + 1] = new Vector2(1f, v);
            }
            int index = 0;
            for (int i = 0; i < samples.Count - 1; i++)
            {
                int left0 = i * 2, right0 = left0 + 1, left1 = left0 + 2, right1 = left0 + 3;
                triangles[index++] = left0;
                triangles[index++] = left1;
                triangles[index++] = right0;
                triangles[index++] = right0;
                triangles[index++] = left1;
                triangles[index++] = right1;
            }
            Mesh mesh = new Mesh { name = label + " - heightfield-conforming ribbon",
                indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject ribbon = new GameObject(label);
            ribbon.transform.SetParent(parent, false);
            ribbon.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            if (addCollider)
            {
                MeshCollider collider = ribbon.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = false;
            }
        }

        private static void BuildDrainageAndStream(Transform parent)
        {
            Mesh cube = GetPrimitiveMesh(PrimitiveType.Cube);
            List<CombineInstance> water = new List<CombineInstance>(100);
            Vector2[][] streams = DrainageNetwork;
            for (int p = 0; p < streams.Length; p++)
            {
                Vector2[] path = streams[p];
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Vector2 a = path[i];
                    Vector2 b = path[i + 1];
                    float length = Vector2.Distance(a, b);
                    int count = Mathf.Max(1, Mathf.CeilToInt(length / 5f));
                    for (int j = 0; j < count; j++)
                    {
                        Vector2 point = Vector2.Lerp(a, b, (j + 0.5f) / count);
                        Vector2 d = (b - a).normalized;
                        Vector3 dir = new Vector3(d.x, 0f, d.y);
                        AddCube(water, cube, parent,
                            new Vector3(point.x, HeightAt(point.x, point.y) + 0.07f, point.y),
                            new Vector3(2.8f, 0.12f, length / count + 0.25f),
                            Quaternion.LookRotation(dir, Vector3.up));
                    }
                }
            }
            CreateCombined(parent, "Seasonal drainage stream beds", water, waterMaterial, false, 0f);
        }

        private static void BuildDefensiveCompounds(Transform parent)
        {
            BuildHouseCompound(parent, new Vector3(-112f, HeightAt(-112f, -168f) + 0.14f, -168f),
                "01 - West defensive compound", 1.0f);
            BuildHouseCompound(parent, new Vector3(116f, HeightAt(116f, -174f) + 0.14f, -174f),
                "02 - East defensive compound", -1.0f);
        }

        private static void BuildVillageAndFarm(Transform parent)
        {
            BuildHouseCompound(parent, new Vector3(-190f, HeightAt(-190f, -54f) + 0.14f, -54f),
                "03 - West farmstead", 0.7f);
            BuildHouseCompound(parent, new Vector3(184f, HeightAt(184f, -70f) + 0.14f, -70f),
                "04 - East village house", -0.8f);
            BuildShed(parent, new Vector3(215f, HeightAt(215f, -38f) + 0.14f, -38f), "Village equipment shed");
            BuildShed(parent, new Vector3(-220f, HeightAt(-220f, -102f) + 0.14f, -102f), "Farm machine shed");
        }

        private static void BuildHouseCompound(Transform parent, Vector3 ground, string label, float yawSign)
        {
            Transform compound = new GameObject(label + " - enterable interior and courtyard").transform;
            compound.SetParent(parent, false);
            compound.position = ground;
            compound.rotation = Quaternion.Euler(0f, yawSign * 7f, 0f);

            BuildEnterableHouse(compound, "Masonry house", 0f, 0f, 17f, 13f, 4.1f);
            CreateBox(compound, new Vector3(0f, 0.42f, 14.2f), new Vector3(27f, 0.82f, 0.72f),
                masonryMaterial, Quaternion.identity, "Courtyard south wall - gate opening");
            CreateBox(compound, new Vector3(-13.2f, 0.42f, 1f), new Vector3(0.72f, 0.82f, 25f),
                masonryMaterial, Quaternion.identity, "Courtyard west wall");
            CreateBox(compound, new Vector3(13.2f, 0.42f, 1f), new Vector3(0.72f, 0.82f, 25f),
                masonryMaterial, Quaternion.identity, "Courtyard east wall");
            CreateBox(compound, new Vector3(0f, 0.42f, -11.8f), new Vector3(27f, 0.82f, 0.72f),
                masonryMaterial, Quaternion.identity, "Courtyard north wall");

            CreateBox(compound, new Vector3(-8.5f, 0.65f, 5.5f), new Vector3(2.2f, 1.3f, 1.5f),
                concreteMaterial, Quaternion.Euler(0f, 22f, 0f), "Courtyard concrete cover block");
            PlaceBattlefieldProp(parent, "old_military_crate",
                compound.TransformPoint(new Vector3(8.3f, 0.15f, 5.2f)),
                yawSign * 7f, 1.25f, 360f,
                label + " - authored courtyard military crate");
            CreateBox(compound, new Vector3(0f, 0.8f, -7.8f), new Vector3(2.0f, 1.6f, 1.8f),
                concreteMaterial, Quaternion.identity, "Interior room cover");
        }

        private static void BuildEnterableHouse(Transform parent, string label,
            float x, float z, float width, float depth, float height)
        {
            Transform house = new GameObject(label + " - open door and window apertures").transform;
            house.SetParent(parent, false);
            house.localPosition = new Vector3(x, 0f, z);

            float wall = 0.55f;
            float halfW = width * 0.5f;
            float halfD = depth * 0.5f;
            // Floor and four real wall facades. The front doorway is a true collider gap.
            CreateBox(house, new Vector3(0f, 0.12f, 0f), new Vector3(width, 0.24f, depth),
                concreteMaterial, Quaternion.identity, "House interior floor");
            CreateBox(house, new Vector3(0f, height * 0.5f, -halfD), new Vector3(width, height, wall),
                masonryMaterial, Quaternion.identity, "Rear wall");
            // Front/south wall: leave a 2.2m x 2.45m open doorway.
            float doorWidth = 2.2f;
            CreateBox(house, new Vector3(-(halfW + doorWidth) * 0.5f, height * 0.5f, halfD),
                new Vector3(halfW - doorWidth * 0.5f, height, wall), masonryMaterial,
                Quaternion.identity, "Front wall - left of doorway");
            CreateBox(house, new Vector3((halfW + doorWidth) * 0.5f, height * 0.5f, halfD),
                new Vector3(halfW - doorWidth * 0.5f, height, wall), masonryMaterial,
                Quaternion.identity, "Front wall - right of doorway");
            CreateBox(house, new Vector3(0f, height - 0.72f, halfD),
                new Vector3(doorWidth, 1.44f, wall), masonryMaterial, Quaternion.identity, "Door lintel");

            // Side walls contain genuine horizontal firing/vision openings.
            BuildWindowedSideWall(house, -halfW, depth, height, wall, "West");
            BuildWindowedSideWall(house, halfW, depth, height, wall, "East");

            // Roof uses two sloped planes with a ridge, leaving interior volume below.
            CreateBox(house, new Vector3(-width * 0.25f, height + 0.48f, 0f),
                new Vector3(width * 0.57f, 0.34f, depth + 1.4f), roofMaterial,
                Quaternion.Euler(0f, 0f, -24f), "West pitched roof plane");
            CreateBox(house, new Vector3(width * 0.25f, height + 0.48f, 0f),
                new Vector3(width * 0.57f, 0.34f, depth + 1.4f), roofMaterial,
                Quaternion.Euler(0f, 0f, 24f), "East pitched roof plane");

            CreateBox(house, new Vector3(-3.7f, 1.3f, -0.8f), new Vector3(0.30f, 2.5f, 6.1f),
                masonryMaterial, Quaternion.identity, "Interior partition - west");
            CreateBox(house, new Vector3(3.7f, 1.3f, -0.8f), new Vector3(0.30f, 2.5f, 6.1f),
                masonryMaterial, Quaternion.identity, "Interior partition - east");
            CreateBox(house, new Vector3(-5.0f, 0.72f, -3.3f), new Vector3(1.6f, 1.25f, 0.85f),
                concreteMaterial, Quaternion.Euler(0f, 8f, 0f), "Interior masonry debris");
            // Use the authored ammunition box instead of a generic cube prop.
            PlaceBattlefieldProp(parent, "ammo_box",
                house.TransformPoint(new Vector3(4.8f, 0.10f, 2.5f)),
                18f, 0.48f, 160f, label + " - interior ammunition box");
        }

        private static void BuildWindowedSideWall(Transform parent, float x, float depth,
            float height, float wall, string side)
        {
            float halfD = depth * 0.5f;
            float windowWidth = 1.65f;
            float windowBottom = 1.15f;
            float windowTop = 2.55f;
            float leftLength = (depth - windowWidth) * 0.5f;
            CreateBox(parent, new Vector3(x, height * 0.5f, -halfD + leftLength * 0.5f),
                new Vector3(wall, height, leftLength), masonryMaterial, Quaternion.identity, side + " wall - rear panel");
            CreateBox(parent, new Vector3(x, height * 0.5f, halfD - leftLength * 0.5f),
                new Vector3(wall, height, leftLength), masonryMaterial, Quaternion.identity, side + " wall - front panel");
            CreateBox(parent, new Vector3(x, windowBottom * 0.5f, 0f),
                new Vector3(wall, windowBottom, windowWidth), masonryMaterial, Quaternion.identity, side + " window sill wall");
            CreateBox(parent, new Vector3(x, (height + windowTop) * 0.5f, 0f),
                new Vector3(wall, height - windowTop, windowWidth), masonryMaterial, Quaternion.identity, side + " window header");
            CreateBox(parent, new Vector3(x, 1.9f, -0.93f), new Vector3(wall + 0.08f, 0.12f, 0.12f),
                roofMaterial, Quaternion.identity, side + " window lower frame");
            CreateBox(parent, new Vector3(x, 1.9f, 0.93f), new Vector3(wall + 0.08f, 0.12f, 0.12f),
                roofMaterial, Quaternion.identity, side + " window upper frame");
        }

        private static void BuildShed(Transform parent, Vector3 ground, string label)
        {
            Transform shed = new GameObject(label + " - damaged farm outbuilding").transform;
            shed.SetParent(parent, false);
            shed.position = ground;
            CreateBox(shed, new Vector3(0f, 1.7f, -3.8f), new Vector3(12f, 3.4f, 0.5f),
                masonryMaterial, Quaternion.identity, "Rear shed wall");
            CreateBox(shed, new Vector3(-5.8f, 1.7f, 0f), new Vector3(0.5f, 3.4f, 8f),
                masonryMaterial, Quaternion.identity, "West shed wall");
            CreateBox(shed, new Vector3(5.8f, 1.7f, 0f), new Vector3(0.5f, 3.4f, 8f),
                masonryMaterial, Quaternion.identity, "East shed wall");
            CreateBox(shed, new Vector3(-3.4f, 1.7f, 3.8f), new Vector3(5.2f, 3.4f, 0.5f),
                masonryMaterial, Quaternion.identity, "Front shed wall west");
            CreateBox(shed, new Vector3(3.4f, 1.7f, 3.8f), new Vector3(5.2f, 3.4f, 0.5f),
                masonryMaterial, Quaternion.identity, "Front shed wall east");
            CreateBox(shed, new Vector3(0f, 3.6f, 0f), new Vector3(12.8f, 0.45f, 8.8f),
                roofMaterial, Quaternion.Euler(0f, 0f, 3f), "Damaged sheet roof");
        }

        private static void BuildBattlefieldDressing(Transform parent)
        {
            // Concrete barriers sit on the outer shoulders of the southern staging lane.
            // The central corridor from the player spawn toward the valley is kept open.
            PlaceBattlefieldProp(parent, "concrete_road_barrier",
                new Vector3(-22f, HeightAt(-22f, 282f), 282f), 90f, 2.8f, 1800f,
                "South staging - west concrete road barrier");
            PlaceBattlefieldProp(parent, "concrete_road_barrier",
                new Vector3(22f, HeightAt(22f, 282f), 282f), 90f, 2.8f, 1800f,
                "South staging - east concrete road barrier");

            // Crew-served resupply dressing remains outside the operator's position
            // and traverse arc.
            PlaceBattlefieldProp(parent, "old_military_crate",
                new Vector3(-214f, HeightAt(-214f, -197f), -197f), 18f, 1.25f, 360f,
                "DShK emplacement - military ammunition crate");
            PlaceBattlefieldProp(parent, "ammo_box",
                new Vector3(-200f, HeightAt(-200f, -197f), -197f), -12f, 0.48f, 160f,
                "DShK emplacement - ammunition box");
            PlaceBattlefieldProp(parent, "metal_jerrycan_green",
                new Vector3(211f, HeightAt(211f, -196f), -196f), 24f, 0.72f, 120f,
                "M2 emplacement - fuel canister");
            PlaceBattlefieldProp(parent, "old_military_crate",
                new Vector3(218f, HeightAt(218f, -197f), -197f), -14f, 1.25f, 360f,
                "M2 emplacement - military ammunition crate");

            // Small resupply props flank, rather than block, the spawn approach.
            PlaceBattlefieldProp(parent, "ammo_box",
                new Vector3(-13f, HeightAt(-13f, 294f), 294f), 16f, 0.48f, 160f,
                "Attacker staging - ammunition box");
            PlaceBattlefieldProp(parent, "metal_jerrycan_green",
                new Vector3(13f, HeightAt(13f, 294f), 294f), -18f, 0.72f, 120f,
                "Attacker staging - fuel canister");
        }

        private static GameObject PlaceBattlefieldProp(Transform parent, string resourceName,
            Vector3 groundPosition, float yaw, float targetLargestDimension, float coverIntegrity,
            string label)
        {
            GameObject prefab = Resources.Load<GameObject>(
                "TacticalStrike/Imported/Props/" + resourceName);
            if (prefab == null)
            {
                Debug.LogError("Missing required battlefield prop resource: " + resourceName);
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, groundPosition,
                Quaternion.Euler(0f, yaw, 0f), parent);
            instance.name = label;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError("Battlefield prop has no renderable meshes: " + resourceName);
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            Bounds bounds = RecalculateWorldBounds(renderers);
            float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (largest < 0.001f)
            {
                Debug.LogError("Battlefield prop has invalid bounds: " + resourceName);
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            float scale = Mathf.Clamp(targetLargestDimension / largest, 0.2f, 5f);
            instance.transform.localScale *= scale;
            bounds = RecalculateWorldBounds(renderers);

            Vector3 correctedPosition = instance.transform.position;
            correctedPosition.y += groundPosition.y - bounds.min.y + 0.018f;
            instance.transform.position = correctedPosition;
            bounds = RecalculateWorldBounds(renderers);

            BoxCollider collider = instance.GetComponent<BoxCollider>();
            if (collider == null) collider = instance.AddComponent<BoxCollider>();
            Vector3 lossy = instance.transform.lossyScale;
            collider.center = instance.transform.InverseTransformPoint(bounds.center);
            collider.size = new Vector3(
                bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(lossy.x)),
                bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(lossy.y)),
                bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(lossy.z)));

            if (coverIntegrity > 0f)
                instance.AddComponent<BallisticCover>().Initialize(coverIntegrity);

            foreach (Renderer renderer in renderers)
            {
                renderer.receiveShadows = true;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                foreach (Material material in renderer.sharedMaterials)
                    if (material != null) material.enableInstancing = true;
            }

            Debug.Log("BATTLEFIELD PROP PLACED: " + label + " / scale=" +
                      scale.ToString("F3") + " / bounds=" + bounds.size);
            return instance;
        }

        private static Bounds RecalculateWorldBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void BuildMountedWeaponPositions(Transform parent)
        {
            BuildHmgPosition(parent, new Vector3(-207f, HeightAt(-207f, -201f) + 0.14f, -201f), "DShK 12.7mm");
            BuildHmgPosition(parent, new Vector3(207f, HeightAt(207f, -201f) + 0.14f, -201f), "M2 Browning .50");
        }

        private static void BuildHmgPosition(Transform parent, Vector3 ground, string label)
        {
            Transform nest = new GameObject(label + " - protected crew-served machine-gun emplacement").transform;
            nest.SetParent(parent, false);
            nest.position = ground;
            for (int i = 0; i < 7; i++)
            {
                float angle = -115f + i * 38f;
                Vector3 local = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 3.2f);
                CreateBox(nest, local + new Vector3(0f, 0.62f, 0f),
                    new Vector3(1.7f, 1.25f, 0.72f), sandbagMaterial,
                    Quaternion.Euler(0f, angle, 0f), "HMG sandbag protection");
            }

            CreateBox(nest, new Vector3(0f, 1.15f, 0f), new Vector3(0.38f, 1.45f, 0.38f),
                metalMaterial, Quaternion.identity, "Fixed HMG pedestal");
            MountedWeaponEmplacement emplacement = nest.gameObject.AddComponent<MountedWeaponEmplacement>();
            emplacement.Initialize(label);
        }

        private static void BuildWoodlandBelts(Transform parent)
        {
            GameObject[] trees =
            {
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-pinerounda"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-pineroundb"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-oak"),
                Resources.Load<GameObject>("TacticalStrike/Imported/Nature/tree-default")
            };
            GameObject bush = Resources.Load<GameObject>("TacticalStrike/Imported/Nature/plant-bushdetailed");
            bool anyTree = false;
            for (int i = 0; i < trees.Length; i++) if (trees[i] != null) anyTree = true;

            System.Random random = new System.Random(10112026);
            for (int i = 0; i < 210; i++)
            {
                int flank = i % 2 == 0 ? -1 : 1;
                float x = flank * (178f + (float)random.NextDouble() * 134f);
                float z = (float)random.NextDouble() * 560f - 280f;
                if (Mathf.Abs(z + 168f) < 38f && Mathf.Abs(x) < 155f) continue;
                if (Mathf.Abs(z + 54f) < 34f && Mathf.Abs(Mathf.Abs(x) - 190f) < 35f) continue;
                if (Mathf.Abs(z - 224f) < 16f || Mathf.Abs(z + 225f) < 16f) continue;

                if (anyTree)
                {
                    GameObject prefab = trees[random.Next(0, trees.Length)];
                    if (prefab == null)
                    {
                        for (int j = 0; j < trees.Length; j++)
                            if (trees[j] != null) { prefab = trees[j]; break; }
                    }
                    if (prefab != null)
                    {
                        float scale = 3.8f + (float)random.NextDouble() * 2.6f;
                        GameObject tree = UnityEngine.Object.Instantiate(prefab,
                            new Vector3(x, HeightAt(x, z) + 0.14f, z),
                            Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);
                        tree.name = "Northwind woodland - " + prefab.name;
                        tree.transform.localScale = Vector3.one * scale;
                        NormalizeModelHeight(tree, UnityEngine.Random.Range(8.0f, 16.0f));
                        AlignModelBottomToTerrain(tree, x, z, 0.025f);
                        foreach (Renderer r in tree.GetComponentsInChildren<Renderer>(true))
                        {
                            r.receiveShadows = true;
                            r.shadowCastingMode = Mathf.Abs(x) < 245f ? ShadowCastingMode.On : ShadowCastingMode.Off;
                            foreach (Material m in r.sharedMaterials)
                                if (m != null) m.enableInstancing = true;
                        }
                        if (tree.GetComponentInChildren<Collider>() == null)
                        {
                            CapsuleCollider trunk = tree.AddComponent<CapsuleCollider>();
                            trunk.center = new Vector3(0f, 0.55f, 0f);
                            trunk.height = 1.1f;
                            trunk.radius = 0.12f;
                        }
                    }
                }

                if (bush != null && i % 2 == 0)
                {
                    float bx = x + (float)random.NextDouble() * 11f - 5.5f;
                    float bz = z + (float)random.NextDouble() * 11f - 5.5f;
                    GameObject shrub = UnityEngine.Object.Instantiate(bush,
                        new Vector3(bx, HeightAt(bx, bz) + 0.14f, bz),
                        Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f), parent);
                    shrub.name = "Northwind woodland shrub";
                    shrub.transform.localScale = Vector3.one * (1.5f + (float)random.NextDouble());
                    NormalizeModelHeight(shrub, UnityEngine.Random.Range(1.3f, 3.0f));
                    AlignModelBottomToTerrain(shrub, bx, bz, 0.02f);
                }
            }
        }

        private static void NormalizeModelHeight(GameObject instance, float targetHeight)
        {
            if (instance == null) return;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y < 0.02f) return;
            float factor = Mathf.Clamp(targetHeight / bounds.size.y, 0.08f, 8f);
            instance.transform.localScale *= factor;
        }

        private static void AlignModelBottomToTerrain(GameObject instance,
            float worldX, float worldZ, float clearance)
        {
            if (instance == null) return;
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 position = instance.transform.position;
            position.y += HeightAt(worldX, worldZ) + clearance - bounds.min.y;
            instance.transform.position = position;
        }

        private static void BuildRidgeRocks(Transform parent)
        {
            Mesh sphere = GetPrimitiveMesh(PrimitiveType.Sphere);
            List<CombineInstance> rocks = new List<CombineInstance>(260);
            System.Random random = new System.Random(73112026);
            for (int i = 0; i < 250; i++)
            {
                float side = random.Next(0, 2) == 0 ? -1f : 1f;
                float x = side * (192f + (float)random.NextDouble() * 128f);
                float z = (float)random.NextDouble() * 650f - 325f;
                float scale = 0.8f + (float)random.NextDouble() * 3.0f;
                Vector3 position = new Vector3(x, HeightAt(x, z) + scale * 0.16f, z);
                Vector3 size = new Vector3(scale * 1.45f, scale * 0.55f, scale);
                Quaternion rotation = Quaternion.Euler(
                    (float)random.NextDouble() * 18f,
                    (float)random.NextDouble() * 360f,
                    (float)random.NextDouble() * 20f);
                AddCubeLike(rocks, sphere, parent, position, size, rotation);
            }
            CreateCombined(parent, "Broken ridge rock outcrops", rocks, concreteMaterial, true, 1500f);
        }

        private static void BuildFieldDetails(Transform parent)
        {
            System.Random random = new System.Random(11022026);
            for (int i = 0; i < 52; i++)
            {
                float x = (float)random.NextDouble() * 560f - 280f;
                float z = (float)random.NextDouble() * 560f - 280f;
                if (Mathf.Abs(x) < 65f && z > -145f && z < 170f) continue;
                float y = HeightAt(x, z);
                CreateBox(parent, new Vector3(x, y + 0.26f, z),
                    new Vector3(0.7f + (float)random.NextDouble() * 0.5f, 0.5f, 0.7f),
                    i % 3 == 0 ? roofMaterial : trenchSoilMaterial,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f),
                    "Field debris / broken masonry");
            }
            BuildFence(parent, new Vector3(-250f, HeightAt(-250f, 22f), 22f), 45f, 34f);
            BuildFence(parent, new Vector3(246f, HeightAt(246f, 66f), 66f), -36f, 38f);
        }

        private static void BuildFence(Transform parent, Vector3 ground, float yaw, float length)
        {
            Transform fence = new GameObject("Rural timber fence - damaged section").transform;
            fence.SetParent(parent, false);
            fence.position = ground;
            fence.rotation = Quaternion.Euler(0f, yaw, 0f);
            for (int i = 0; i < 8; i++)
            {
                float x = -length * 0.5f + i * length / 7f;
                CreateBox(fence, new Vector3(x, 0.72f, 0f), new Vector3(0.16f, 1.44f, 0.16f),
                    roofMaterial, Quaternion.identity, "Fence post");
            }
            CreateBox(fence, new Vector3(0f, 0.92f, 0f), new Vector3(length, 0.15f, 0.14f),
                roofMaterial, Quaternion.identity, "Fence upper rail");
            CreateBox(fence, new Vector3(-2f, 0.38f, 0f), new Vector3(length * 0.43f, 0.13f, 0.14f),
                roofMaterial, Quaternion.identity, "Broken lower rail");
        }

        private static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            GameObject source = GameObject.CreatePrimitive(type);
            Mesh mesh = source.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(source);
            return mesh;
        }

        private static void AddCube(List<CombineInstance> list, Mesh mesh, Transform parent,
            Vector3 worldPosition, Vector3 scale, Quaternion rotation)
        {
            list.Add(new CombineInstance
            {
                mesh = mesh,
                transform = Matrix4x4.TRS(parent.InverseTransformPoint(worldPosition), rotation, scale)
            });
        }

        private static void AddCubeLike(List<CombineInstance> list, Mesh mesh, Transform parent,
            Vector3 worldPosition, Vector3 scale, Quaternion rotation)
        {
            AddCube(list, mesh, parent, worldPosition, scale, rotation);
        }

        private static void CreateCombined(Transform parent, string label,
            List<CombineInstance> items, Material material, bool collider, float durability)
        {
            if (items == null || items.Count == 0) return;
            Mesh mesh = new Mesh { name = label + " - combined", indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(items.ToArray(), true, true, false);
            mesh.RecalculateBounds();

            GameObject combined = new GameObject(label);
            combined.transform.SetParent(parent, false);
            combined.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = combined.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            if (collider)
            {
                MeshCollider meshCollider = combined.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
                if (durability > 0f)
                    combined.AddComponent<BallisticCover>().Initialize(durability);
            }
        }

        private static void CreateBox(Transform parent, Vector3 localPosition, Vector3 size,
            Material material, Quaternion localRotation, string label)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = label;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = localRotation;
            box.transform.localScale = size;
            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }
    }
}
