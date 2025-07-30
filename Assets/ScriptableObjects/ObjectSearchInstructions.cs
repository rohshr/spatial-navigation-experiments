using UnityEngine;

[CreateAssetMenu(fileName = "ObjectSearchInstructions", menuName = "Locomotion Research/Object Search Instructions")]
public class ObjectSearchInstructions : ScriptableObject
{
    [System.Serializable]
    public class ObjectSearchInstruction
    {
        [Tooltip("Unique identifier for the object to search")]
        public string objectId;
        [TextArea(3, 10)]
        public string instructionText;
    }

    public ObjectSearchInstruction[] instructions;

    // Helper method to find instruction by spawn point name
    public string GetInstructionForObjectSearch(string objectName)
    {
        var instruction = System.Array.Find(instructions, x => x.objectId == objectName);
        return instruction?.instructionText ?? "No instruction found for this object.";
    }
}
