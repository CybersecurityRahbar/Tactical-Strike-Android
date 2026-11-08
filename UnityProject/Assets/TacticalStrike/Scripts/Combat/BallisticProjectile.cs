using UnityEngine;

namespace TacticalStrike
{
    public sealed class BallisticProjectile : MonoBehaviour
    {
        private Vector3 velocity;
        private Transform shooter;
        private bool tracer;
        private float damage;
        private float penetration;
        private float drag;
        private float maxRange;
        private float travelled;
        private float age;
        private LineRenderer line;
        private Vector3 lineTail;
        private Material tracerMaterial;

        public void Initialize(Vector3 start, Vector3 initialVelocity, Transform owner,
            bool showTracer, float projectileDamage, float projectilePenetration,
            float dragCoefficient, float range)
        {
            transform.position = start;
            velocity = initialVelocity;
            shooter = owner;
            tracer = showTracer;
            damage = projectileDamage;
            penetration = projectilePenetration;
            drag = dragCoefficient;
            maxRange = range;
            lineTail = start;
            if (tracer) CreateTracer();
        }

        private void CreateTracer()
        {
            line = gameObject.AddComponent<LineRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            tracerMaterial = new Material(shader);
            tracerMaterial.color = new Color(1f, 0.12f, 0.025f);
            line.material = tracerMaterial;
            line.positionCount = 2;
            line.startWidth = 0.012f;
            line.endWidth = 0.006f;
            line.useWorldSpace = true;
            line.numCapVertices = 2;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position);
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.04f);
            age += dt;
            if (age > 7f || travelled >= maxRange || velocity.sqrMagnitude < 25f)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 start = transform.position;
            float speed = velocity.magnitude;
            velocity -= velocity.normalized * speed * speed * drag * dt;
            velocity.y -= 9.81f * dt;
            Vector3 end = start + velocity * dt;
            Vector3 segment = end - start;
            float segmentLength = segment.magnitude;

            if (Physics.Raycast(start, segment.normalized, out RaycastHit hit, segmentLength,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                BallisticCover cover = hit.collider.GetComponentInParent<BallisticCover>();
                if (cover != null)
                    cover.ApplyBallisticDamage(damage, penetration, "5.45x39mm");
                ImpactFx.Spawn(hit.point, hit.normal, damage >= 75f);
                Destroy(gameObject);
                return;
            }

            transform.position = end;
            travelled += segmentLength;
            if (line != null)
            {
                lineTail = transform.position - velocity.normalized * 1.65f;
                line.SetPosition(0, lineTail);
                line.SetPosition(1, transform.position);
            }
        }

        private void OnDestroy()
        {
            if (tracerMaterial != null) Destroy(tracerMaterial);
        }
    }
}
