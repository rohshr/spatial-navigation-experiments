using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeasurementTool
{
    /// <summary>
    /// Component that automatically displays object measurements in the Inspector
    /// Add this to any GameObject to see its dimensions and bounds information
    /// </summary>
    [AddComponentMenu("Measurement Tool/Object Measurement")]
    [System.Serializable]
    public class ObjectMeasurement : MonoBehaviour
    {
        [Header("Measurement Display")]
        public MeasurementUnit displayUnit = MeasurementUnit.Metric;
        public bool showBothUnits = true;
        public bool showInInspector = true;

        [Header("Measurement Results")]
        [SerializeField, TextArea(3, 5)] private string sizeDisplay = "";
        [SerializeField, TextArea(2, 3)] private string boundsDisplay = "";

        [Header("Distance Measurement")]
        public GameObject measureDistanceTo;
        [SerializeField] private string distanceDisplay = "";

        private void OnValidate()
        {
            if (showInInspector)
            {
                UpdateMeasurements();
            }
        }

        private void Start()
        {
            UpdateMeasurements();
        }

        private void UpdateMeasurements()
        {
            UpdateSizeDisplay();
            UpdateBoundsDisplay();
            UpdateDistanceDisplay();
        }

        private void UpdateSizeDisplay()
        {
            Vector3 size = MeasurementUtils.GetObjectSize(gameObject);

            if (size != Vector3.zero)
            {
                sizeDisplay = $"Width (X): {MeasurementUtils.FormatMeasurement(size.x, displayUnit, showBothUnits)}\n" +
                             $"Height (Y): {MeasurementUtils.FormatMeasurement(size.y, displayUnit, showBothUnits)}\n" +
                             $"Depth (Z): {MeasurementUtils.FormatMeasurement(size.z, displayUnit, showBothUnits)}\n" +
                             $"Diagonal: {MeasurementUtils.FormatMeasurement(size.magnitude, displayUnit, showBothUnits)}";
            }
            else
            {
                sizeDisplay = "No renderer or collider found on this GameObject.\nAdd a Renderer or Collider component to measure size.";
            }
        }

        private void UpdateBoundsDisplay()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                boundsDisplay = $"Center: {bounds.center}\n" +
                               $"Min: {bounds.min}\n" +
                               $"Max: {bounds.max}";
            }
            else
            {
                Collider collider = GetComponent<Collider>();
                if (collider != null)
                {
                    Bounds bounds = collider.bounds;
                    boundsDisplay = $"Center: {bounds.center}\n" +
                                   $"Min: {bounds.min}\n" +
                                   $"Max: {bounds.max}";
                }
                else
                {
                    boundsDisplay = "No bounds available";
                }
            }
        }

        private void UpdateDistanceDisplay()
        {
            if (measureDistanceTo != null)
            {
                float distance = MeasurementUtils.GetDistance(gameObject, measureDistanceTo);
                distanceDisplay = $"Distance to {measureDistanceTo.name}:\n{MeasurementUtils.FormatMeasurement(distance, displayUnit, showBothUnits)}";
            }
            else
            {
                distanceDisplay = "Assign a GameObject above to measure distance";
            }
        }

        /// <summary>
        /// Get formatted size information as a string
        /// </summary>
        public string GetSizeInfo()
        {
            UpdateSizeDisplay();
            return sizeDisplay;
        }

        /// <summary>
        /// Get distance to another object
        /// </summary>
        public string GetDistanceTo(GameObject other)
        {
            if (other == null) return "No target object specified";

            float distance = MeasurementUtils.GetDistance(gameObject, other);
            return MeasurementUtils.FormatMeasurement(distance, displayUnit, showBothUnits);
        }

        /// <summary>
        /// Get the volume of the object (if it has bounds)
        /// </summary>
        public string GetVolume()
        {
            Vector3 size = MeasurementUtils.GetObjectSize(gameObject);
            if (size != Vector3.zero)
            {
                float volume = size.x * size.y * size.z;

                if (displayUnit == MeasurementUnit.Metric)
                {
                    if (volume < 0.001f)
                    {
                        return $"{volume * 1000000000f:F2} mm³";
                    }
                    else if (volume < 1f)
                    {
                        return $"{volume * 1000000f:F2} cm³";
                    }
                    else
                    {
                        return $"{volume:F3} m³";
                    }
                }
                else
                {
                    float cubicFeet = volume * 35.3147f; // Convert m³ to ft³
                    if (cubicFeet < 1f)
                    {
                        float cubicInches = volume * 61023.7f; // Convert m³ to in³
                        return $"{cubicInches:F2} in³";
                    }
                    else
                    {
                        return $"{cubicFeet:F3} ft³";
                    }
                }
            }
            return "No volume available";
        }

        /// <summary>
        /// Get the surface area of the object (approximated as a box)
        /// </summary>
        public string GetSurfaceArea()
        {
            Vector3 size = MeasurementUtils.GetObjectSize(gameObject);
            if (size != Vector3.zero)
            {
                float surfaceArea = 2f * (size.x * size.y + size.y * size.z + size.z * size.x);

                if (displayUnit == MeasurementUnit.Metric)
                {
                    if (surfaceArea < 0.01f)
                    {
                        return $"{surfaceArea * 1000000f:F2} mm²";
                    }
                    else if (surfaceArea < 1f)
                    {
                        return $"{surfaceArea * 10000f:F2} cm²";
                    }
                    else
                    {
                        return $"{surfaceArea:F3} m²";
                    }
                }
                else
                {
                    float squareFeet = surfaceArea * 10.7639f; // Convert m² to ft²
                    if (squareFeet < 1f)
                    {
                        float squareInches = surfaceArea * 1550f; // Convert m² to in²
                        return $"{squareInches:F2} in²";
                    }
                    else
                    {
                        return $"{squareFeet:F3} ft²";
                    }
                }
            }
            return "No surface area available";
        }

        // Context menu options for easy access
        [ContextMenu("Update Measurements")]
        public void ForceUpdateMeasurements()
        {
            UpdateMeasurements();
        }

        [ContextMenu("Log All Measurements")]
        public void LogAllMeasurements()
        {
            Debug.Log($"=== Measurements for {gameObject.name} ===");
            Debug.Log($"Size: {GetSizeInfo()}");
            Debug.Log($"Volume: {GetVolume()}");
            Debug.Log($"Surface Area: {GetSurfaceArea()}");
            if (measureDistanceTo != null)
            {
                Debug.Log($"Distance: {GetDistanceTo(measureDistanceTo)}");
            }
        }
    }
}