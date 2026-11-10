using UnityEngine;

namespace TacticalStrike
{
    /// <summary>Calibre-specific parameters. DragFactor is game-tuned quadratic drag, not G1/G7 BC.</summary>
    public sealed class BallisticProfile
    {
        public readonly string AmmunitionName;
        public readonly float ProjectileMassGrams, DiameterMillimeters, MuzzleVelocity;
        public readonly float DragFactor, Damage, Penetration, MaximumRange, TracerDiameterMeters;

        public BallisticProfile(string name, float massGrams, float diameterMm, float muzzleVelocity,
            float damage, float penetration, float range)
        {
            AmmunitionName = name;
            ProjectileMassGrams = massGrams;
            DiameterMillimeters = diameterMm;
            MuzzleVelocity = muzzleVelocity;
            float areaRatio = Mathf.Pow(diameterMm / 5.45f, 2f);
            DragFactor = 0.000190f * (3.42f / Mathf.Max(0.01f, massGrams)) * areaRatio;
            Damage = damage;
            Penetration = penetration;
            MaximumRange = range;
            TracerDiameterMeters = diameterMm * 0.00165f;
        }
    }

    public static class BallisticProfiles
    {
        public static readonly BallisticProfile AK74M_7N6 =
            new BallisticProfile("5.45x39mm 7N6 PS", 3.42f, 5.45f, 890f, 38f, 0.34f, 650f);
        public static readonly BallisticProfile AK74M_7T3_Tracer =
            new BallisticProfile("5.45x39mm 7T3 tracer", 3.20f, 5.45f, 883f, 37f, 0.31f, 650f);
        public static readonly BallisticProfile AK74M_7N22_AP =
            new BallisticProfile("5.45x39mm 7N22 AP", 3.69f, 5.45f, 890f, 42f, 0.48f, 650f);
        public static readonly BallisticProfile DShK_12_7x108 =
            new BallisticProfile("12.7x108mm DShK ball", 48.3f, 12.7f, 850f, 128f, 0.94f, 1250f);
        public static readonly BallisticProfile M2_12_7x99_M33 =
            new BallisticProfile("12.7x99mm M33 ball", 42.87f, 12.7f, 887f, 132f, 0.95f, 1500f);
    }
}
