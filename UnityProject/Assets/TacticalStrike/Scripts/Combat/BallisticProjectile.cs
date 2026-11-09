using UnityEngine;

namespace TacticalStrike
{
    public sealed class BallisticProjectile : MonoBehaviour
    {
        private const float Gravity = 9.81f;
        private const float SimulationStep = 0.008f;
        private Vector3 velocity;
        private Transform shooter;
        private float damage;
        private float penetration;
        private float drag;
        private float maxRange;
        private float travelled;
        private float age;
        private int ricochets;
        private LineRenderer line;
        private Material tracerMaterial;

        public void Initialize(Vector3 start, Vector3 initialVelocity, Transform owner,
            bool showTracer, float projectileDamage, float projectilePenetration,
            float dragCoefficient, float range)
        {
            transform.position = start;
            velocity = initialVelocity;
            shooter = owner;
            damage = projectileDamage;
            penetration = projectilePenetration;
            drag = dragCoefficient;
            maxRange = range;
            if (showTracer) CreateTracer();
        }

        private void CreateTracer()
        {
            line = gameObject.AddComponent<LineRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) return;

            tracerMaterial = new Material(shader);
            Color hot = new Color(1f, 0.92f, 0.38f, 1f);
            if (tracerMaterial.HasProperty("_BaseColor"))
                tracerMaterial.SetColor("_BaseColor", hot);
            if (tracerMaterial.HasProperty("_Color"))
                tracerMaterial.SetColor("_Color", hot);
            if (tracerMaterial.HasProperty("_EmissionColor"))
            {
                tracerMaterial.EnableKeyword("_EMISSION");
                tracerMaterial.SetColor("_EmissionColor", new Color(3.2f, 1.15f, 0.16f, 1f));
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(hot, 0f),
                    new GradientColorKey(new Color(1f, 0.30f, 0.035f, 1f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.88f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            line.sharedMaterial = tracerMaterial;
            line.colorGradient = gradient;
            line.positionCount = 2;
            line.startWidth = 0.012f;
            line.endWidth = 0.0035f;
            line.useWorldSpace = true;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.numCornerVertices = 2;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.SetPosition(0, transform.position - velocity.normalized * 4.8f);
            line.SetPosition(1, transform.position);
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
                        cover.ApplyBallisticDamage(damage, penetration, "5.45x39mm");

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

            if (line != null)
            {
                Vector3 direction = velocity.sqrMagnitude > 0.01f
                    ? velocity.normalized : transform.forward;
                // A 900 m/s round advances about 15 m per rendered frame at 60 FPS.
                // Size the tail to the distance travelled this frame so the tracer does
                // not appear as isolated tiny dashes with empty gaps between frames.
                float visibleTrailLength = Mathf.Clamp(velocity.magnitude * frameDelta * 1.45f,
                    8f, 55f);
                line.SetPosition(0, transform.position - direction * visibleTrailLength);
                line.SetPosition(1, transform.position);
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
            if (tracerMaterial != null) Destroy(tracerMaterial);
        }
    }
}