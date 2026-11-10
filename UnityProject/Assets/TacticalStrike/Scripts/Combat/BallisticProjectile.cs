using UnityEngine;

namespace TacticalStrike
{
    public sealed class BallisticProjectile : MonoBehaviour
    {
        private const float Gravity = 9.81f;
        private const float SimulationStep = 0.008f;
        private const int TracerSides = 10;
        private static readonly float[] TracerRingT = { 0f, 0.13f, 0.48f, 1f };
        private static readonly float[] TracerRingRadius = { 0.72f, 0.58f, 0.28f, 0.025f };
        private Vector3 velocity;
        private Transform shooter;
        private float damage, penetration, drag, maxRange, travelled, age, tracerDiameter;
        private string ammunitionName = "unknown ammunition";
        private int ricochets;
        private Mesh tracerMesh;
        private MeshFilter tracerMeshFilter;
        private Material tracerMaterial, tracerHeadMaterial;
        private Vector3[] tracerVertices, tracerNormals;
        private Vector2[] tracerUv;
        private int[] tracerTriangles;
        private static readonly Collider[] SuppressionOverlapBuffer = new Collider[64];

        public void Initialize(Vector3 start, Vector3 initialVelocity, Transform owner,
            bool showTracer, BallisticProfile profile)
        {
            if (profile == null) throw new System.ArgumentNullException(nameof(profile));
            transform.position = start;
            velocity = initialVelocity;
            if (velocity.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            shooter = owner;
            damage = profile.Damage;
            penetration = profile.Penetration;
            drag = profile.DragFactor;
            maxRange = profile.MaximumRange;
            ammunitionName = profile.AmmunitionName;
            if (showTracer) CreateTracer(profile.TracerDiameterMeters);
        }

        private static Material CreateEmissiveMaterial(Shader shader, string label,
            Color baseColor, Color emissionColor)
        {
            Material material = new Material(shader) { name = label };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Color")) material.SetColor("_Color", baseColor);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
            }
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.22f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            return material;
        }

        private void CreateTracer(float projectileDiameter)
        {
            // Calibre-scaled bright core; don't enlarge the tracer into a laser tube.
            tracerDiameter = Mathf.Clamp(projectileDiameter * 1.55f, 0.008f, 0.025f);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogError("Tracer visual could not find a supported lit shader."); return; }

            tracerMaterial = CreateEmissiveMaterial(shader, "Tracer - incandescent amber body",
                new Color(1f, 0.43f, 0.055f, 1f), new Color(3.5f, 1.05f, 0.12f, 1f));
            tracerHeadMaterial = CreateEmissiveMaterial(shader, "Tracer - hot yellow-white core",
                new Color(1f, 0.91f, 0.57f, 1f), new Color(5f, 2.6f, 0.62f, 1f));

            GameObject tail = new GameObject("Tracer - round tapered incandescent tail");
            tail.transform.SetParent(transform, false);
            tracerMeshFilter = tail.AddComponent<MeshFilter>();
            MeshRenderer tailRenderer = tail.AddComponent<MeshRenderer>();
            tracerMesh = new Mesh { name = "Tracer - 3D circular tapered mesh" };
            tracerMesh.MarkDynamic();
            tracerMeshFilter.sharedMesh = tracerMesh;
            tailRenderer.sharedMaterial = tracerMaterial;
            tailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tailRenderer.receiveShadows = false;

            int ringCount = TracerRingT.Length;
            int vertexCount = ringCount * TracerSides;
            tracerVertices = new Vector3[vertexCount];
            tracerNormals = new Vector3[vertexCount];
            tracerUv = new Vector2[vertexCount];
            tracerTriangles = new int[(ringCount - 1) * TracerSides * 6];
            int triangle = 0;
            for (int ring = 0; ring < ringCount; ring++)
            for (int side = 0; side < TracerSides; side++)
            {
                int index = ring * TracerSides + side;
                float angle = (Mathf.PI * 2f * side) / TracerSides;
                float x = Mathf.Cos(angle), y = Mathf.Sin(angle);
                tracerNormals[index] = new Vector3(x, y, 0f);
                tracerUv[index] = new Vector2(side / (float)TracerSides, TracerRingT[ring]);
            }
            for (int ring = 0; ring < ringCount - 1; ring++)
            for (int side = 0; side < TracerSides; side++)
            {
                int next = (side + 1) % TracerSides;
                int a = ring * TracerSides + side, b = ring * TracerSides + next;
                int c = (ring + 1) * TracerSides + side, d = (ring + 1) * TracerSides + next;
                tracerTriangles[triangle++] = a; tracerTriangles[triangle++] = c; tracerTriangles[triangle++] = b;
                tracerTriangles[triangle++] = b; tracerTriangles[triangle++] = c; tracerTriangles[triangle++] = d;
            }
            tracerMesh.vertices = tracerVertices;
            tracerMesh.normals = tracerNormals;
            tracerMesh.uv = tracerUv;
            tracerMesh.triangles = tracerTriangles;
            tracerMesh.RecalculateBounds();

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Tracer - rounded incandescent projectile head";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = Vector3.zero;
            head.transform.localScale = Vector3.one * tracerDiameter * 1.45f;
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null) Destroy(headCollider);
            MeshRenderer headRenderer = head.GetComponent<MeshRenderer>();
            headRenderer.sharedMaterial = tracerHeadMaterial;
            headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            headRenderer.receiveShadows = false;
            UpdateTracerGeometry(0.016f);
        }

        private void UpdateTracerGeometry(float frameDelta)
        {
            if (tracerMesh == null || velocity.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            // The old formula yielded 6-8m trails at rifle speed. Tracer glow should
            // be a short incandescent wake attached to the moving projectile.
            float tailLength = Mathf.Clamp(velocity.magnitude * frameDelta * 0.015f, 0.09f, 0.22f);
            float rootRadius = tracerDiameter * 0.82f;
            for (int ring = 0; ring < TracerRingT.Length; ring++)
            {
                float z = -tailLength * TracerRingT[ring];
                float radius = rootRadius * TracerRingRadius[ring];
                for (int side = 0; side < TracerSides; side++)
                {
                    int index = ring * TracerSides + side;
                    float angle = (Mathf.PI * 2f * side) / TracerSides;
                    tracerVertices[index] = new Vector3(Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius, z);
                }
            }
            tracerMesh.vertices = tracerVertices;
            tracerMesh.RecalculateBounds();
        }

        private void Update()
        {
            float frameDelta = Mathf.Min(Time.deltaTime, 0.05f);
            age += frameDelta;
            if (age > 7f || travelled >= maxRange || velocity.sqrMagnitude < 25f)
            {
                Destroy(gameObject);
                return;
            }

            int steps = Mathf.Max(1, Mathf.CeilToInt(frameDelta / SimulationStep));
            float step = frameDelta / steps;
            for (int i = 0; i < steps; i++)
            {
                Vector3 start = transform.position;
                velocity += Vector3.down * Gravity * step;
                float speed = velocity.magnitude;
                if (speed > 0.001f)
                    velocity -= velocity.normalized * speed * speed * drag * step;

                Vector3 delta = velocity * step;
                float segmentLength = delta.magnitude;
                if (segmentLength <= 0.0001f) continue;

                ApplyNearMissSuppression(start, start + delta);

                RaycastHit[] hits = Physics.RaycastAll(start, delta / segmentLength,
                    segmentLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                RaycastHit nearest = default;
                bool found = false;
                float nearestDistance = float.MaxValue;
                foreach (RaycastHit hit in hits)
                {
                    if (shooter != null &&
                        (hit.transform == shooter || hit.transform.IsChildOf(shooter)))
                        continue;
                    if (hit.distance < nearestDistance)
                    {
                        nearest = hit;
                        nearestDistance = hit.distance;
                        found = true;
                    }
                }

                if (found)
                {
                    string surface = SurfaceName(nearest.collider);
                    Vector3 incoming = velocity.normalized;
                    float glancingAngle = Vector3.Angle(-incoming, nearest.normal);
                    bool hardSurface = surface.Contains("metal") || surface.Contains("stone") ||
                                       surface.Contains("brick") || surface.Contains("concrete");
                    bool canRicochet = ricochets == 0 && hardSurface &&
                                       glancingAngle >= 73f && penetration < 0.58f;

                    ImpactFx.Spawn(nearest.point, nearest.normal, damage >= 75f, surface);

                    BallisticCover cover = nearest.collider.GetComponentInParent<BallisticCover>();
                    if (canRicochet)
                    {
                        velocity = Vector3.Reflect(velocity, nearest.normal) * 0.34f;
                        damage *= 0.38f;
                        penetration *= 0.48f;
                        travelled += nearest.distance;
                        transform.position = nearest.point + nearest.normal * 0.035f;
                        ricochets++;
                        continue;
                    }

                    if (cover != null)
                        cover.ApplyBallisticDamage(damage, penetration, ammunitionName);

                    transform.position = nearest.point;
                    Destroy(gameObject);
                    return;
                }

                transform.position = start + delta;
                travelled += segmentLength;
                if (travelled >= maxRange)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (tracerMesh != null) UpdateTracerGeometry(frameDelta);
        }

        private void ApplyNearMissSuppression(Vector3 start, Vector3 end)
        {
            const float suppressionRadius = 4.6f;
            int count = Physics.OverlapCapsuleNonAlloc(start, end, suppressionRadius,
                SuppressionOverlapBuffer, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                Collider nearby = SuppressionOverlapBuffer[i];
                if (nearby == null) continue;
                FpsPlayerController target = nearby.GetComponentInParent<FpsPlayerController>();
                if (target == null) continue;

                if (shooter != null &&
                    (target.transform == shooter || target.transform.IsChildOf(shooter) ||
                     shooter.IsChildOf(target.transform)))
                    continue;

                Vector3 playerCenter = target.transform.position + Vector3.up * 0.92f;
                Vector3 segment = end - start;
                float lengthSq = segment.sqrMagnitude;
                float t = lengthSq > 0.0001f
                    ? Mathf.Clamp01(Vector3.Dot(playerCenter - start, segment) / lengthSq) : 0f;
                Vector3 closestPoint = start + segment * t;
                Vector3 toPlayer = playerCenter - closestPoint;
                float distance = toPlayer.magnitude;
                if (distance >= suppressionRadius || distance < 0.05f) continue;

                // A solid wall or terrain ridge between the round and the infantry
                // should break near-miss suppression instead of applying it through cover.
                if (Physics.Raycast(closestPoint, toPlayer / distance, out RaycastHit occlusion,
                    distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    FpsPlayerController visiblePlayer =
                        occlusion.collider.GetComponentInParent<FpsPlayerController>();
                    if (visiblePlayer != target) continue;
                }

                float proximity = 1f - distance / suppressionRadius;
                float weaponPressure = damage >= 100f ? 0.30f : (damage >= 60f ? 0.18f : 0.11f);
                float amount = Mathf.Lerp(0.025f, weaponPressure, proximity);
                target.ReceiveSuppression(amount);
            }
        }

        private static string SurfaceName(Collider collider)
        {
            string value = collider.name.ToLowerInvariant();
            Renderer renderer = collider.GetComponent<Renderer>();
            if (renderer == null) renderer = collider.GetComponentInParent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
                value += " " + renderer.sharedMaterial.name.ToLowerInvariant();
            return value;
        }

        private void OnDestroy()
        {
            if (tracerMesh != null)
            {
                if (tracerMeshFilter != null) tracerMeshFilter.sharedMesh = null;
                Destroy(tracerMesh);
            }
            if (tracerMaterial != null) Destroy(tracerMaterial);
            if (tracerHeadMaterial != null) Destroy(tracerHeadMaterial);
        }
    }
}