using UnityEngine;

namespace Sandbox
{
    public class SimulationConfiguration : MonoBehaviour
    {
        [Header("Simulation Parameters")]
        public int totalNumberOfCycles = 10;
        public int tradesPerCycle = 10;
        public float simulationSpeed = 1.0f; // Seconds between cycles
    }
}
