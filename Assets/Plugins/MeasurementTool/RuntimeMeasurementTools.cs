using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MeasurementTool
{
    [AddComponentMenu("Measurement Tool/Runtime Measurement Tool")]
    public class RuntimeMeasurementTool : MonoBehaviour
    {
        [Header("Measurement Settings")]
        public MeasurementUnit primaryUnit = MeasurementUnit.Metric;
        public bool showBothUnits = true;
        public Color measurementColor = Color.yellow;
        public float lineWidth = 0.02f;
        public KeyCode measurementKey = KeyCode.M;
        public bool showUI = true;
        
        [Header("Visual Settings")]
        public Material lineMaterial;
        public GameObject pointPrefab;
        
        private List<Vector3> measurementPoints = new List<Vector3>();
        private List<GameObject> pointObjects = new List<GameObject>();
        private List<LineRenderer> lineRenderers = new List<LineRenderer>();
        private Camera playerCamera;
        private bool isMeasuring = false;
        private Canvas uiCanvas;
        private UnityEngine.UI.Text measurementText;
        
        private void Start()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindObjectOfType<Camera>();
                
            CreateUI();
            SetupMaterials();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(measurementKey))
            {
                ToggleMeasurement();
            }
            
            if (isMeasuring && Input.GetMouseButtonDown(0))
            {
                PlaceMeasurementPoint();
            }
            
            if (isMeasuring && Input.GetMouseButtonDown(1))
            {
                StopMeasurement();
            }
            
            if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            {
                ClearAllMeasurements();
            }
            
            UpdateUI();
        }
        
        private void CreateUI()
        {
            if (!showUI) return;
            
            // Create UI Canvas
            GameObject canvasObj = new GameObject("MeasurementUI");
            canvasObj.transform.SetParent(transform);
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 100;
            
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create text display
            GameObject textObj = new GameObject("MeasurementText");
            textObj.transform.SetParent(canvasObj.transform);
            
            measurementText = textObj.AddComponent<UnityEngine.UI.Text>();
            measurementText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            measurementText.fontSize = 16;
            measurementText.color = Color.white;
            measurementText.alignment = TextAnchor.UpperLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = new Vector2(10, -10);
            textRect.sizeDelta = new Vector2(400, 200);
            
            // Add background
            UnityEngine.UI.Image background = textObj.AddComponent<UnityEngine.UI.Image>();
            background.color = new Color(0, 0, 0, 0.5f);
        }
        
        private void SetupMaterials()
        {
            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
                lineMaterial.color = measurementColor;
            }
        }
        
        private void ToggleMeasurement()
        {
            isMeasuring = !isMeasuring;
            
            if (isMeasuring)
            {
                ClearAllMeasurements();
                Debug.Log("Measurement mode ON - Left click to place points, Right click to finish");
            }
            else
            {
                Debug.Log("Measurement mode OFF");
            }
        }
        
        private void PlaceMeasurementPoint()
        {
            Vector3 worldPos = GetWorldPositionFromMouse();
            if (worldPos != Vector3.zero)
            {
                measurementPoints.Add(worldPos);
                CreatePointVisual(worldPos);
                
                if (measurementPoints.Count > 1)
                {
                    CreateLineVisual(measurementPoints[measurementPoints.Count - 2], worldPos);
                }
            }
        }
        
        private void StopMeasurement()
        {
            isMeasuring = false;
            Debug.Log("Measurement completed");
        }
        
        private Vector3 GetWorldPositionFromMouse()
        {
            if (playerCamera == null) return Vector3.zero;
            
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            
            // Try to hit colliders first
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
            
            // If no hit, project to a plane at a reasonable distance
            return ray.GetPoint(10f);
        }
        
        private void CreatePointVisual(Vector3 position)
        {
            GameObject point;
            
            if (pointPrefab != null)
            {
                point = Instantiate(pointPrefab, position, Quaternion.identity);
            }
            else
            {
                point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.position = position;
                point.transform.localScale = Vector3.one * 0.1f;
                
                Renderer renderer = point.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = lineMaterial;
                }
            }
            
            point.transform.SetParent(transform);
            pointObjects.Add(point);
        }
        
        private void CreateLineVisual(Vector3 start, Vector3 end)
        {
            GameObject lineObj = new GameObject("MeasurementLine");
            lineObj.transform.SetParent(transform);
            
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.positionCount = 2;
            line.SetPositions(new Vector3[] { start, end });
            line.useWorldSpace = true;
            
            lineRenderers.Add(line);
        }
        
        private void ClearAllMeasurements()
        {
            measurementPoints.Clear();
            
            foreach (GameObject point in pointObjects)
            {
                if (point != null)
                    DestroyImmediate(point);
            }
            pointObjects.Clear();
            
            foreach (LineRenderer line in lineRenderers)
            {
                if (line != null)
                    DestroyImmediate(line.gameObject);
            }
            lineRenderers.Clear();
        }
        
        private void UpdateUI()
        {
            if (measurementText == null) return;
            
            string uiText = $"Measurement Tool - Press {measurementKey} to toggle\n";
            uiText += isMeasuring ? "MEASURING - Left click: place point, Right click: finish\n" : "Press M to start measuring\n";
            uiText += "Ctrl+C: Clear all measurements\n\n";
            
            if (measurementPoints.Count > 1)
            {
                uiText += "Measurements:\n";
                
                for (int i = 1; i < measurementPoints.Count; i++)
                {
                    float distance = Vector3.Distance(measurementPoints[i - 1], measurementPoints[i]);
                    uiText += $"Segment {i}: {FormatMeasurement(distance)}\n";
                }
                
                if (measurementPoints.Count > 2)
                {
                    float totalDistance = 0f;
                    for (int i = 1; i < measurementPoints.Count; i++)
                    {
                        totalDistance += Vector3.Distance(measurementPoints[i - 1], measurementPoints[i]);
                    }
                    uiText += $"Total: {FormatMeasurement(totalDistance)}\n";
                }
            }
            
            measurementText.text = uiText;
        }
        
        private string FormatMeasurement(float unityUnits)
        {
            if (primaryUnit == MeasurementUnit.Metric)
            {
                string metricText = FormatMetric(unityUnits);
                if (showBothUnits)
                {
                    string imperialText = FormatImperial(unityUnits);
                    return $"{metricText} ({imperialText})";
                }
                return metricText;
            }
            else
            {
                string imperialText = FormatImperial(unityUnits);
                if (showBothUnits)
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
        
        private void OnDestroy()
        {
            ClearAllMeasurements();
        }
    }
}