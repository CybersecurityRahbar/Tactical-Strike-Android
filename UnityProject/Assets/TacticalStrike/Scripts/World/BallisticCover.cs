using UnityEngine;

namespace TacticalStrike
{
    public sealed class BallisticCover : MonoBehaviour
    {
        [SerializeField] private float integrity = 100f;
        private Renderer targetRenderer;
        private Material runtimeMaterial;

        public void Initialize(float startingIntegrity)
        {
            integrity = Mathf.Max(1f, startingIntegrity);
        }

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
        }

        public void ApplyBallisticDamage(float damage, float penetration, string calibre)
        {
            float effectiveDamage = Mathf.Max(1f, damage * Mathf.Clamp01(0.25f + penetration));
            integrity -= effectiveDamage;
            if (targetRenderer != null)
            {
                if (runtimeMaterial == null)
                {
                    runtimeMaterial = new Material(targetRenderer.sharedMaterial);
                    targetRenderer.material = runtimeMaterial;
                }
                float wear = Mathf.Clamp01(1f - integrity / 140f);
                runtimeMaterial.color = Color.Lerp(Color.white, new Color(0.48f, 0.39f, 0.28f), wear * 0.55f);
            }

            if (integrity <= 0f)
            {
                ImpactFx.Spawn(transform.position, Vector3.up, true);
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);
        }
    }
}
