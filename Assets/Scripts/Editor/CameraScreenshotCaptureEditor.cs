using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraScreenshotCapture))]
public class CameraScreenshotCaptureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Get reference to the target component
        CameraScreenshotCapture captureComponent = (CameraScreenshotCapture)target;

        // Add a button in the inspector
        if (GUILayout.Button("Take Screenshot", GUILayout.Height(30)))
        {
            captureComponent.TakeScreenshot();
        }

        EditorGUILayout.Space();

        // Optional: Add a button for custom name screenshots
        EditorGUILayout.LabelField("Custom Screenshot", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Take Screenshot with Timestamp", GUILayout.Height(25)))
        {
            string customName = $"Custom_{System.DateTime.Now:HHmmss}";
            captureComponent.TakeScreenshotWithCustomName(customName);
        }
    }
}