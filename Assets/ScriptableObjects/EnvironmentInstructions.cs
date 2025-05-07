using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentInstructions", menuName = "Locomotion Research/Environment Instructions")]
public class EnvironmentInstructions : ScriptableObject
{
    [System.Serializable]
    public class EnvironmentInstruction
    {
        [Tooltip("Unique identifier for the spawn point")]
        public string spawnPointId;
        [TextArea(3, 10)]
        public string instructionText;
    }

    public EnvironmentInstruction[] instructions;

    // Helper method to find instruction by spawn point name
    public string GetInstructionForSpawnPoint(string spawnPointName)
    {
        var instruction = System.Array.Find(instructions, x => x.spawnPointId == spawnPointName);
        return instruction?.instructionText ?? "No instruction found for this spawn point.";
    }
}
