#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LocomotionExperimentBlock), true)]
public class LocomotionExperimentBlockPropertyDrawer : PropertyDrawer
{
    private const float TopPadding = 5f;
    private const float BottomPadding = 5f;
    private const float LineSpacing = 2f;
    private const float SectionSpacing = 5f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Only apply custom drawing for SerializeReference fields in lists
        if (property.propertyType == SerializedPropertyType.ManagedReference && IsInTrialBlocksList(property))
        {
            // Create default instance if null
            if (property.managedReferenceValue == null)
            {
                property.managedReferenceValue = new LocomotionExperimentBlock();
                property.serializedObject.ApplyModifiedProperties();
            }
            
            var blockNameProperty = property.FindPropertyRelative("blockName");
            // Set default block name if empty
            if (blockNameProperty != null && string.IsNullOrEmpty(blockNameProperty.stringValue))
            {
                var blockIndex = GetBlockIndex(property) + 1;
                blockNameProperty.stringValue = $"Block{blockIndex}";
            }
            
            // Draw block name label (line 0)
            var blockNameLabelRect = new Rect(position.x, GetYPosition(position.y, 0), position.width, LineHeight);
            var boldStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            EditorGUI.LabelField(blockNameLabelRect, blockNameProperty?.stringValue ?? "", boldStyle);
            
            // Draw editable block name field (line 1)
            var blockNameFieldRect = new Rect(position.x, GetYPosition(position.y, 1), position.width, LineHeight);
            EditorGUI.PropertyField(blockNameFieldRect, blockNameProperty, new GUIContent("Block Name"));
            
            // Draw type selector dropdown (line 2)
            var dropdownRect = new Rect(position.x, GetYPosition(position.y, 2, SectionSpacing), position.width, LineHeight);
            var types = GetDerivedTypes(typeof(LocomotionExperimentBlock));
            var typeNames = types.Select(GetTypeName).ToArray();
            var currentTypeName = GetCurrentTypeName(property);
            var currentIndex = Array.IndexOf(typeNames, currentTypeName);
            if (currentIndex == -1) currentIndex = 0;
        
            var newIndex = EditorGUI.Popup(dropdownRect, "Block Type", currentIndex, typeNames);

            if (newIndex != currentIndex && newIndex >= 0 && newIndex < types.Length)
            {
                property.managedReferenceValue = Activator.CreateInstance(types[newIndex]);
                property.serializedObject.ApplyModifiedProperties();
            }

            // Draw block properties
            if (property.managedReferenceValue != null)
            {
                var headerHeight = GetTotalHeaderHeight();
                var availableHeight = position.height - headerHeight - BottomPadding;
                var propertyRect = new Rect(position.x, position.y + headerHeight,
                    position.width, availableHeight);
                EditorGUI.PropertyField(propertyRect, property, new GUIContent("Block Settings"), true);
            }
        }
        else
        {
            // Use default drawing for non-SerializeReference or nested properties
            EditorGUI.PropertyField(position, property, label, true);
        }

        EditorGUI.EndProperty();
    }
    
    private float LineHeight => EditorGUIUtility.singleLineHeight;
    
    private float GetYPosition(float baseY, int lineIndex, float extraSpacing = 0f)
    {
        return baseY + TopPadding + (lineIndex * (LineHeight + LineSpacing)) + extraSpacing;
    }
    
    private float GetTotalHeaderHeight()
    {
        return TopPadding + (3 * LineHeight) + (2 * LineSpacing) + SectionSpacing;
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.ManagedReference &&
            property.managedReferenceValue != null &&
            IsInTrialBlocksList(property))
        {
            float headerHeight = GetTotalHeaderHeight();
            float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
            return headerHeight + propertyHeight + BottomPadding;
        }
        return EditorGUI.GetPropertyHeight(property, true);
    }

    private bool IsInTrialBlocksList(SerializedProperty property)
    {
        var path = property.propertyPath;
        // Check if this property is within the trialBlocks array
        return path.Contains("newTrialBlocks.Array.data[") && 
               path.Count(c => c == '.') <= 2; // Allow for trialBlocks.Array.data[X] pattern
    }
    
    private int GetBlockIndex(SerializedProperty property)
    {
        var path = property.propertyPath;
        var startIndex = path.IndexOf("data[") + 5;
        var endIndex = path.IndexOf("]", startIndex);
        if (startIndex > 4 && endIndex > startIndex)
        {
            if (int.TryParse(path.Substring(startIndex, endIndex - startIndex), out int index))
                return index;
        }
        return 0;
    }

    private Type[] GetDerivedTypes(Type baseType)
    {
        var assembly = baseType.Assembly;
        return assembly.GetTypes()
            .Where(t => (t.IsSubclassOf(baseType) || t == baseType) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(LocomotionExperimentBlock))
            return "Generic Block";
        if (type == typeof(ObjectSearchBlock))
            return "Object Search Block";
        if (type == typeof(ExplorationBlock))
            return "Exploration Block";
        return type.Name;
    }

    private string GetCurrentTypeName(SerializedProperty property)
    {
        if (property.managedReferenceValue == null) return "Generic Block";

        var type = property.managedReferenceValue.GetType();
        return GetTypeName(type);
    }
}
#endif