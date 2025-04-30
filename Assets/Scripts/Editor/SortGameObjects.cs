using UnityEngine;
using UnityEditor;

public class SortGameObjects : MonoBehaviour
{
    [MenuItem("Tools/Sort Child GameObjects by X and Z Position")]
    public static void SortChildrenByPosition()
    {
        // Get the selected GameObject in the hierarchy
        GameObject selectedGameObject = Selection.activeGameObject;

        if (selectedGameObject == null)
        {
            Debug.LogWarning("No GameObject selected. Please select a GameObject in the hierarchy.");
            return;
        }

        // Get all direct child objects
        Transform[] children = new Transform[selectedGameObject.transform.childCount];
        for (int i = 0; i < selectedGameObject.transform.childCount; i++)
        {
            children[i] = selectedGameObject.transform.GetChild(i);
        }

        // Sort the children based on their x and z positions
        System.Array.Sort(children, (a, b) =>
        {
            if (a.position.x != b.position.x)
                return a.position.x.CompareTo(b.position.x);
            else
                return a.position.z.CompareTo(b.position.z);
        });

        // Reorder the children in the hierarchy
        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetSiblingIndex(i);
        }

        Debug.Log($"Sorted {children.Length} child GameObjects of '{selectedGameObject.name}' by X and Z position.");
    }
}
