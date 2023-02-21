

using System.Linq;
using nickmaltbie.OpenKCC.Character;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    public class MoleParticles : MonoBehaviour
    {
        /// <summary>
        /// Default color for particles.
        /// </summary>
        public static Color DefaultParticleColor = new Color(153 / 255.0f, 102 / 255.0f, 0 / 255.0f);

        public static Color dirtColor = DefaultParticleColor;

        [SerializeField]
        public ParticleSystem diggingTrailParticlePrefab;

        [SerializeField]
        public ParticleSystem burrowParticlePrefab;

        public bool DrawParticles { get; set; }

        public Color GroundParticleColor(GameObject parent)
        {
            if (parent?.GetComponent<DiggingParticleColor>() is DiggingParticleColor dig)
            {
                return dig.diggingColor;
            }
            else if (parent?.GetComponentInParent<Renderer>() is Renderer renderer)
            {
                if (renderer.material.HasColor("_BaseColor"))
                {
                    return renderer.material.GetColor("_BaseColor");
                }
            }

            return DefaultParticleColor;
        }

        [SerializeField]
        public int maxDiggingTrails = 10;

        [SerializeField]
        public Vector3 particleOffset = new Vector3(0, -0.5f, 0);

        /// <summary>
        /// Previous parent for spawning burrowing particles.
        /// </summary>
        private Transform previousParent;

        private ParticleSystem burrowParticles;
        private ParticleSystem[] diggingTrails;
        private int currentTrail;

        private ParticleSystem CurrentTrail => diggingTrails[currentTrail];
        private ParticleSystem NextTrail
        {
            get
            {
                currentTrail = (currentTrail + 1) % diggingTrails.Length;

                if (CurrentTrail.isPlaying)
                {
                    CurrentTrail.Stop();
                }

                CurrentTrail.Clear();
                return CurrentTrail;
            }
        }

        public void Awake()
        {
            burrowParticles = Instantiate(burrowParticlePrefab, transform);
            diggingTrails = Enumerable.Range(0, maxDiggingTrails)
                .Select(_ => Instantiate(diggingTrailParticlePrefab, transform))
                .ToArray();

            burrowParticles.transform.localPosition = particleOffset;
            burrowParticles.Stop();
            foreach (ParticleSystem trail in diggingTrails)
            {
                trail.transform.localPosition = particleOffset;
                trail.Stop();
            }
        }

        public void Update()
        {
            if (DrawParticles)
            {
                UpdateParticles();
            }
            else
            {
                ClearParticles();
            }
        }

        public void ClearParticles()
        {
            previousParent = null;
            CurrentTrail.Stop();
            burrowParticles.Stop();
            burrowParticles.Clear();
        }

        public void UpdateParticles()
        {
            KCCMovementEngine movementEngine = GetComponent<KCCMovementEngine>();
            Transform currentParent = movementEngine.GroundedState.Floor?.transform;

            if (currentParent == null)
            {
                CurrentTrail.Stop();
                ParticleSystem.MainModule trailParticles = NextTrail.main;
                trailParticles.simulationSpace = ParticleSystemSimulationSpace.World;
                trailParticles.customSimulationSpace = null;
                var emissionSettings = CurrentTrail.emission;
                emissionSettings.rateOverDistanceMultiplier = 10;
            }
            else if (movementEngine.GroundedState.StandingOnGround && previousParent != currentParent)
            {
                CurrentTrail.Stop();
                ParticleSystem.MainModule trailParticles = NextTrail.main;
                trailParticles.simulationSpace = ParticleSystemSimulationSpace.Custom;
                trailParticles.customSimulationSpace = currentParent;
                var emissionSettings = CurrentTrail.emission;
                emissionSettings.rateOverDistanceMultiplier = 10 * Mathf.Max(
                    currentParent.lossyScale.x,
                    currentParent.lossyScale.y,
                    currentParent.lossyScale.z
                );
            }

            if (!CurrentTrail.isPlaying)
            {
                CurrentTrail.Play();
                ParticleSystem.MainModule trailParticles = CurrentTrail.main;
                trailParticles.startColor = new ParticleSystem.MinMaxGradient(dirtColor, GroundParticleColor(currentParent?.gameObject));
            }

            if (!burrowParticles.isPlaying || currentParent != previousParent)
            {
                burrowParticles.Play();
                ParticleSystem.MainModule burrowSettings = burrowParticles.main;
                burrowSettings.simulationSpace = ParticleSystemSimulationSpace.Local;
                burrowSettings.startColor = new ParticleSystem.MinMaxGradient(dirtColor, GroundParticleColor(currentParent.gameObject));
                burrowParticles.transform.localPosition = particleOffset;
            }

            previousParent = currentParent;
        }
    }
}
