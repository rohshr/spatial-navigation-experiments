using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeasurementTool
{
    [System.Serializable]
    public class MeasurementSettings
    {
        public MeasurementUnit primaryUnit = MeasurementUnit.Metric;
        public bool showBothUnits = true;
        public Color measurementColor = Color.yellow;
        public float lineWidth = 2f;
        public bool snapToGrid = false;
        public float snapDistance = 0.1f;
    }

    public class MeasurementTool : EditorWindow
    {
        private static MeasurementTool window;
        private MeasurementSettings settings = new MeasurementSettings();
        private List<Vector3> measurementPoints = new List<Vector3>();
        private bool isPlacingPoints = false;
        private Vector3 currentMousePosition;
        private bool showSettings = false;

        [MenuItem("Tools/Measurement Tool")]
        public static void OpenWindow()
        {
            window = GetWindow<MeasurementTool>("Measurement Tool");
            window.minSize = new Vector2(300, 400);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            LoadSettings();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SaveSettings();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            
            // Title
            EditorGUILayout.LabelField("Unity Measurement Tool", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Measurement controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(isPlacingPoints ? "Stop Measuring" : "Start Measuring"))
            {
                isPlacingPoints = !isPlacingPoints;
                if (isPlacingPoints)
                {
                    measurementPoints.Clear();
                }
            }
            
            if (GUILayout.Button("Clear All"))
            {
                measurementPoints.Clear();
                isPlacingPoints = false;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Settings toggle
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings");
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                settings.primaryUnit = (MeasurementUnit)EditorGUILayout.EnumPopup("Primary Unit", settings.primaryUnit);
                settings.showBothUnits = EditorGUILayout.Toggle("Show Both Units", settings.showBothUnits);
                settings.measurementColor = EditorGUILayout.ColorField("Line Color", settings.measurementColor);
                settings.lineWidth = EditorGUILayout.FloatField("Line Width", settings.lineWidth);
                settings.snapToGrid = EditorGUILayout.Toggle("Snap to Grid", settings.snapToGrid);
                if (settings.snapToGrid)
                {
                    settings.snapDistance = EditorGUILayout.FloatField("Snap Distance", settings.snapDistance);
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            // Instructions
            if (isPlacingPoints)
            {
                EditorGUILayout.HelpBox("Click in the Scene view to place measurement points. Right-click to finish current measurement.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Click 'Start Measuring' to begin placing measurement points in the scene.", MessageType.Info);
            }

            GUILayout.Space(10);

            // Display measurements
            if (measurementPoints.Count > 1)
            {
                EditorGUILayout.LabelField("Measurements:", EditorStyles.boldLabel);
                
                for (int i = 1; i < measurementPoints.Count; i++)
                {
                    float distance = Vector3.Distance(measurementPoints[i - 1], measurementPoints[i]);
                    string measurementText = FormatMeasurement(distance);
                    EditorGUILayout.LabelField($"Point {i}: {measurementText}");
                }

                if (measurementPoints.Count > 2)
                {
                    float totalDistance = 0f;
                    for (int i = 1; i < measurementPoints.Count; i++)
                    {
                        totalDistance += Vector3.Distance(measurementPoints[i - 1], measurementPoints[i]);
                    }
                    EditorGUILayout.LabelField($"Total: {FormatMeasurement(totalDistance)}", EditorStyles.boldLabel);
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!isPlacingPoints) return;

            Event e = Event.current;
            
            // Handle mouse input
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0) // Left click
                {
                    Vector3 worldPos = GetWorldPositionFromMouse(e.mousePosition);
                    if (settings.snapToGrid)
                    {
                        worldPos = SnapToGrid(worldPos);
                    }
                    measurementPoints.Add(worldPos);
                    e.Use();
                }
                else if (e.button == 1) // Right click
                {
                    isPlacingPoints = false;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseMove)
            {
                currentMousePosition = GetWorldPositionFromMouse(e.mousePosition);
                if (settings.snapToGrid)
                {
                    currentMousePosition = SnapToGrid(currentMousePosition);
                }
                sceneView.Repaint();
            }

            // Draw measurements
            DrawMeasurements();
        }

        private Vector3 GetWorldPositionFromMouse(Vector2 mousePosition)
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            
            // Raycast to find intersection point
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
            else
            {
                // If no collision, project to a plane at origin
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float distance))
                {
                    return ray.GetPoint(distance);
                }
            }
            
            return Vector3.zero;
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float snap = settings.snapDistance;
            return new Vector3(
                Mathf.Round(position.x / snap) * snap,
                Mathf.Round(position.y / snap) * snap,
                Mathf.Round(position.z / snap) * snap
            );
        }

        private void DrawMeasurements()
        {
            Handles.color = settings.measurementColor;
            
            // Draw existing measurement lines
            for (int i = 1; i < measurementPoints.Count; i++)
            {
                Vector3 start = measurementPoints[i - 1];
                Vector3 end = measurementPoints[i];
                
                // Draw line
                Handles.DrawLine(start, end, settings.lineWidth);
                
                // Draw points
                Handles.DrawSolidDisc(start, SceneView.lastActiveSceneView.camera.transform.forward, 0.05f);
                Handles.DrawSolidDisc(end, SceneView.lastActiveSceneView.camera.transform.forward, 0.05f);
                
                // Draw distance label
                Vector3 midPoint = (start + end) / 2f;
                float distance = Vector3.Distance(start, end);
                string label = FormatMeasurement(distance);
                
                Handles.Label(midPoint, label, EditorStyles.whiteBoldLabel);
            }
            
            // Draw current line (from last point to mouse)
            if (measurementPoints.Count > 0 && isPlacingPoints)
            {
                Vector3 lastPoint = measurementPoints[measurementPoints.Count - 1];
                Handles.color = new Color(settings.measurementColor.r, settings.measurementColor.g, settings.measurementColor.b, 0.5f);
                Handles.DrawLine(lastPoint, currentMousePosition, settings.lineWidth);
                
                // Show distance to current mouse position
                float distance = Vector3.Distance(lastPoint, currentMousePosition);
                string label = FormatMeasurement(distance);
                Vector3 midPoint = (lastPoint + currentMousePosition) / 2f;
                Handles.Label(midPoint, label, EditorStyles.whiteBoldLabel);
            }
        }

        private string FormatMeasurement(float unityUnits)
        {
            // Unity units are typically in meters
            if (settings.primaryUnit == MeasurementUnit.Metric)
            {
                string metricText = FormatMetric(unityUnits);
                if (settings.showBothUnits)
                {
                    string imperialText = FormatImperial(unityUnits);
                    return $"{metricText} ({imperialText})";
                }
                return metricText;
            }
            else
            {
                string imperialText = FormatImperial(unityUnits);
                if (settings.showBothUnits)
                {
                    string metricText = FormatMetric(unityUnits);
                    return $"{imperialText} ({metricText})";
                }
                return imperialText;
            }
        }

        private string FormatMetric(float meters)
        {
            if (meters < 0.01f)
            {
                return $"{meters * 1000f:F1} mm";
            }
            else if (meters < 1f)
            {
                return $"{meters * 100f:F1} cm";
            }
            else if (meters < 1000f)
            {
                return $"{meters:F2} m";
            }
            else
            {
                return $"{meters / 1000f:F2} km";
            }
        }

        private string FormatImperial(float meters)
        {
            float inches = meters * 39.3701f;
            
            if (inches < 12f)
            {
                return $"{inches:F1}\"";
            }
            else if (inches < 36f)
            {
                float feet = inches / 12f;
                return $"{feet:F1} ft";
            }
            else
            {
                float feet = inches / 12f;
                int wholeFeet = (int)feet;
                float remainingInches = inches - (wholeFeet * 12f);
                if (remainingInches < 0.1f)
                {
                    return $"{wholeFeet} ft";
                }
                else
                {
                    return $"{wholeFeet}' {remainingInches:F1}\"";
                }
            }
        }

        private void SaveSettings()
        {
            string json = JsonUtility.ToJson(settings);
            EditorPrefs.SetString("MeasurementTool_Settings", json);
        }

        private void LoadSettings()
        {
            if (EditorPrefs.HasKey("MeasurementTool_Settings"))
            {
                string json = EditorPrefs.GetString("MeasurementTool_Settings");
                JsonUtility.FromJsonOverwrite(json, settings);
            }
        }
    }
}