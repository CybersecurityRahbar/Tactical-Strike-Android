using UnityEngine;

namespace TacticalStrike
{
    public static class ImpactFx
    {
        public static void Spawn(Vector3 position, Vector3 normal, bool heavy)
        {
            GameObject cloud = new GameObject(heavy ? "Heavy impact dust" : "Rifle impact dust");
            cloud.transform.position = position + normal * 0.04f;
            ParticleSystem particles = cloud.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.72f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.30f, heavy ? 1.05f : 0.72f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(heavy ? 3.2f : 1.8f, heavy ? 8.0f : 4.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(heavy ? 0.14f : 0.055f, heavy ? 0.54f : 0.23f);
            main.startColor = new Color(0.52f, 0.42f, 0.30f, 0.62f);
            main.gravityModifier = -0.12f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = heavy ? 72 : 32;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(heavy ? 48 : 20)) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = heavy ? 0.35f : 0.15f;
            shape.rotation = new Vector3(90f, 0f, 0f);

            ParticleSystemRenderer renderer = cloud.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader != null)
            {
                Material material = new Material(shader);
                material.color = new Color(0.56f, 0.46f, 0.34f, 0.7f);
                renderer.material = material;
            }
            particles.Play();
            Object.Destroy(cloud, 1.8f);
        }
    }
}
