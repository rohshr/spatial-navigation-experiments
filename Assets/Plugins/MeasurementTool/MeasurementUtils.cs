using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeasurementTool
{
    /// <summary>
    /// Utility class for unit conversions and measurement formatting
    /// </summary>
    public static class MeasurementUtils
    {
        // Conversion constants
        private const float METERS_TO_FEET = 3.28084f;
        private const float METERS_TO_INCHES = 39.3701f;
        private const float FEET_TO_METERS = 0.3048f;
        private const float INCHES_TO_METERS = 0.0254f;
        
        /// <summary>
        /// Convert meters to feet
        /// </summary>
        public static float MetersToFeet(float meters)
        {
            return meters * METERS_TO_FEET;
        }
        
        /// <summary>
        /// Convert meters to inches
        /// </summary>
        public static float MetersToInches(float meters)
        {
            return meters * METERS_TO_INCHES;
        }
        
        /// <summary>
        /// Convert feet to meters
        /// </summary>
        public static float FeetToMeters(float feet)
        {
            return feet * FEET_TO_METERS;
        }
        
        /// <summary>
        /// Convert inches to meters
        /// </summary>
        public static float InchesToMeters(float inches)
        {
            return inches * INCHES_TO_METERS;
        }
        
        /// <summary>
        /// Get the distance between two transforms
        /// </summary>
        public static float GetDistance(Transform a, Transform b)
        {
            return Vector3.Distance(a.position, b.position);
        }
        
        /// <summary>
        /// Get the distance between two GameObjects
        /// </summary>
        public static float GetDistance(GameObject a, GameObject b)
        {
            return Vector3.Distance(a.transform.position, b.transform.position);
        }
        
        /// <summary>
        /// Get the size of a GameObject's bounds
        /// </summary>
        public static Vector3 GetObjectSize(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size;
            }
            
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.size;
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Format a measurement value with appropriate units
        /// </summary>
        public static string FormatMeasurement(float unityUnits, MeasurementUnit unit, bool showBothUnits = false)
        {
            if (unit == MeasurementUnit.Metric)
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
        
        private static string FormatMetric(float meters)
        {
            if (meters < 0.001f)
            {
                return $"{meters * 1000000f:F0} μm";
            }
            else if (meters < 0.01f)
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
        
        private static string FormatImperial(float meters)
        {
            float inches = meters * METERS_TO_INCHES;
            
            if (inches < 1f)
            {
                return $"{inches:F2}\"";
            }
            else if (inches < 12f)
            {
                return $"{inches:F1}\"";
            }
            else if (inches < 36f)
            {
                float feet = inches / 12f;
                return $"{feet:F1} ft";
            }
            else if (inches < 63360f) // Less than 1 mile
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
            else
            {
                float miles = inches / 63360f;
                return $"{miles:F2} mi";
            }
        }
    }
    

}

/*
SETUP GUIDE FOR UNITY MEASUREMENT TOOL PLUGIN

1. INSTALLATION:
   - Create a new folder in your project: Assets/Plugins/MeasurementTool/
   - Create an "Editor" subfolder: Assets/Plugins/MeasurementTool/Editor/
   - Place the MeasurementTool.cs script in the Editor folder
   - Place the RuntimeMeasurementTool.cs, MeasurementUtils.cs, and ObjectMeasurement.cs in the main folder

2. EDITOR TOOL USAGE:
   - Go to Tools > Measurement Tool to open the editor window
   - Click "Start Measuring" to begin placing measurement points
   - Click in the Scene view to place points
   - Right-click to finish current measurement
   - Use "Clear All" to remove all measurements
   - Configure settings in the foldout panel

3. RUNTIME TOOL USAGE:
   - Add the RuntimeMeasurementTool component to any GameObject in your scene
   - Configure the settings in the inspector
   - In play mode, press M to toggle measurement mode
   - Left-click to place measurement points
   - Right-click to finish measuring
   - Ctrl+C to clear all measurements

4. OBJECT MEASUREMENT COMPONENT:
   - Add the ObjectMeasurement component to any GameObject
   - It will automatically display the object's dimensions in the inspector
   - Use GetSizeInfo() and GetDistanceTo() methods in scripts

5. UTILITY FUNCTIONS:
   - Use MeasurementUtils.FormatMeasurement() for custom formatting
   - Use conversion functions like MetersToFeet(), MetersToInches(), etc.
   - Use GetDistance() and GetObjectSize() for measurements in code

6. CUSTOMIZATION:
   - Modify colors, line widths, and display options in the settings
   - Create custom point prefabs for the runtime tool
   - Extend the formatting functions for additional units

7. FEATURES:
   - Supports both metric and imperial units
   - Real-time measurement display
   - Snap to grid option
   - Persistent settings
   - Runtime and editor modes
   - Automatic unit formatting (mm, cm, m, km / inches, feet, miles)

8. REQUIREMENTS:
   - Unity 2019.4 or later
   - No additional dependencies required
   - Works with any render pipeline

9. TROUBLESHOOTING:
   - If measurements don't appear, check that you have colliders in your scene
   - For runtime tool, ensure you have a Camera tagged as "MainCamera"
   - Make sure the scripts are in the correct folders (Editor scripts in Editor folder)

10. PERFORMANCE NOTES:
    - The tool uses LineRenderer components for runtime visualization
    - Large numbers of measurement points may affect performance
    - Clear measurements when not needed to maintain good performance
*/