using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ObjectOrganizer2D : EditorWindow
{
    [MenuItem("Tools/2D Object Organizer")]
    public static void ShowWindow()
    {
        GetWindow<ObjectOrganizer2D>("2D Object Organizer");
    }

    // Alignment settings
    private float alignmentGap = 1f;
    
    // Distribution settings
    private float distributionGap = 1f;
    
    // Grid settings
    private float gridSpacingX = 2f;
    private float gridSpacingY = 2f;
    private int gridColumns = 3;
    private bool autoCalculateColumns = true;
    
    // UI Settings
    private Vector2 scrollPosition;
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("2D Object Organizer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Show selected objects count
        Transform[] selectedObjects = GetSelectedTransforms();
        EditorGUILayout.LabelField($"Selected Objects: {selectedObjects.Length}");
        
        if (selectedObjects.Length < 2)
        {
            EditorGUILayout.HelpBox("Select 2 or more objects to organize them.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }
        
        GUILayout.Space(10);
        
        // ALIGNMENT SECTION
        DrawSectionHeader("Alignment");
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Align Left"))
            AlignObjects(selectedObjects, AlignmentType.Left);
        if (GUILayout.Button("Align Center X"))
            AlignObjects(selectedObjects, AlignmentType.CenterX);
        if (GUILayout.Button("Align Right"))
            AlignObjects(selectedObjects, AlignmentType.Right);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Align Top"))
            AlignObjects(selectedObjects, AlignmentType.Top);
        if (GUILayout.Button("Align Center Y"))
            AlignObjects(selectedObjects, AlignmentType.CenterY);
        if (GUILayout.Button("Align Bottom"))
            AlignObjects(selectedObjects, AlignmentType.Bottom);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        
        // DISTRIBUTION SECTION
        DrawSectionHeader("Distribution");
        
        distributionGap = EditorGUILayout.FloatField("Gap Distance", distributionGap);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Distribute Horizontally"))
            DistributeObjects(selectedObjects, DistributionType.Horizontal, distributionGap);
        if (GUILayout.Button("Distribute Vertically"))
            DistributeObjects(selectedObjects, DistributionType.Vertical, distributionGap);
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        
        // GRID ARRANGEMENT SECTION
        DrawSectionHeader("Grid Arrangement");
        
        gridSpacingX = EditorGUILayout.FloatField("Spacing X", gridSpacingX);
        gridSpacingY = EditorGUILayout.FloatField("Spacing Y", gridSpacingY);
        
        autoCalculateColumns = EditorGUILayout.Toggle("Auto Calculate Columns", autoCalculateColumns);
        
        if (!autoCalculateColumns)
        {
            gridColumns = EditorGUILayout.IntField("Columns", Mathf.Max(1, gridColumns));
        }
        else
        {
            int autoColumns = Mathf.CeilToInt(Mathf.Sqrt(selectedObjects.Length));
            EditorGUILayout.LabelField($"Auto Columns: {autoColumns}");
            gridColumns = autoColumns;
        }
        
        if (GUILayout.Button("Arrange in Grid"))
            ArrangeInGrid(selectedObjects, gridColumns, gridSpacingX, gridSpacingY);
        
        GUILayout.Space(15);
        
        // UTILITY SECTION
        DrawSectionHeader("Utilities");
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by Name"))
            SortObjectsByName(selectedObjects);
        if (GUILayout.Button("Randomize Order"))
            RandomizeObjects(selectedObjects);
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Reset to Original Positions"))
            ResetToOriginalPositions();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawSectionHeader(string title)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);
        GUILayout.Space(5);
    }
    
    private Transform[] GetSelectedTransforms()
    {
        return Selection.transforms;
    }
    
    // ALIGNMENT METHODS
    public enum AlignmentType
    {
        Left, Right, Top, Bottom, CenterX, CenterY
    }
    
    private void AlignObjects(Transform[] objects, AlignmentType alignmentType)
    {
        if (objects.Length < 2) return;
        
        Undo.RecordObjects(objects, $"Align Objects {alignmentType}");
        
        Vector3 referencePosition = objects[0].position;
        
        // Calculate reference point based on all objects
        switch (alignmentType)
        {
            case AlignmentType.Left:
                referencePosition.x = objects.Min(t => t.position.x);
                break;
            case AlignmentType.Right:
                referencePosition.x = objects.Max(t => t.position.x);
                break;
            case AlignmentType.Top:
                referencePosition.y = objects.Max(t => t.position.y);
                break;
            case AlignmentType.Bottom:
                referencePosition.y = objects.Min(t => t.position.y);
                break;
            case AlignmentType.CenterX:
                referencePosition.x = objects.Average(t => t.position.x);
                break;
            case AlignmentType.CenterY:
                referencePosition.y = objects.Average(t => t.position.y);
                break;
        }
        
        // Apply alignment
        foreach (Transform obj in objects)
        {
            Vector3 newPosition = obj.position;
            
            switch (alignmentType)
            {
                case AlignmentType.Left:
                case AlignmentType.Right:
                case AlignmentType.CenterX:
                    newPosition.x = referencePosition.x;
                    break;
                case AlignmentType.Top:
                case AlignmentType.Bottom:
                case AlignmentType.CenterY:
                    newPosition.y = referencePosition.y;
                    break;
            }
            
            obj.position = newPosition;
        }
        
        EditorUtility.SetDirty(objects[0]);
    }
    
    // DISTRIBUTION METHODS
    public enum DistributionType
    {
        Horizontal, Vertical
    }
    
    private void DistributeObjects(Transform[] objects, DistributionType distributionType, float gap)
    {
        if (objects.Length < 2) return;
        
        Undo.RecordObjects(objects, $"Distribute Objects {distributionType}");
        
        // Sort objects by position
        Transform[] sortedObjects;
        
        if (distributionType == DistributionType.Horizontal)
        {
            sortedObjects = objects.OrderBy(t => t.position.x).ToArray();
        }
        else
        {
            sortedObjects = objects.OrderBy(t => t.position.y).ToArray();
        }
        
        // Start from the first object's position
        Vector3 currentPosition = sortedObjects[0].position;
        
        for (int i = 1; i < sortedObjects.Length; i++)
        {
            if (distributionType == DistributionType.Horizontal)
            {
                currentPosition.x += gap;
                Vector3 newPosition = sortedObjects[i].position;
                newPosition.x = currentPosition.x;
                sortedObjects[i].position = newPosition;
            }
            else
            {
                currentPosition.y += gap;
                Vector3 newPosition = sortedObjects[i].position;
                newPosition.y = currentPosition.y;
                sortedObjects[i].position = newPosition;
            }
            
            currentPosition = sortedObjects[i].position;
        }
        
        EditorUtility.SetDirty(objects[0]);
    }
    
    // GRID ARRANGEMENT METHODS
    private void ArrangeInGrid(Transform[] objects, int columns, float spacingX, float spacingY)
    {
        if (objects.Length == 0) return;
        
        Undo.RecordObjects(objects, "Arrange in Grid");
        
        // Use the first selected object as the starting position
        Vector3 startPosition = objects[0].position;
        
        for (int i = 0; i < objects.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            Vector3 newPosition = startPosition;
            newPosition.x = startPosition.x + (col * spacingX);
            newPosition.y = startPosition.y - (row * spacingY); // Negative Y for top-to-bottom
            
            objects[i].position = newPosition;
        }
        
        EditorUtility.SetDirty(objects[0]);
    }
    
    // UTILITY METHODS
    private void SortObjectsByName(Transform[] objects)
    {
        System.Array.Sort(objects, (a, b) => string.Compare(a.name, b.name));
        Selection.objects = objects.Cast<Object>().ToArray();
    }
    
    private void RandomizeObjects(Transform[] objects)
    {
        System.Random random = new System.Random();
        for (int i = 0; i < objects.Length; i++)
        {
            int randomIndex = random.Next(i, objects.Length);
            (objects[i], objects[randomIndex]) = (objects[randomIndex], objects[i]);
        }
        Selection.objects = objects.Cast<Object>().ToArray();
    }
    
    // Store original positions for reset functionality
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    
    void OnSelectionChange()
    {
        // Store original positions when selection changes
        originalPositions.Clear();
        foreach (Transform t in Selection.transforms)
        {
            originalPositions[t] = t.position;
        }
        Repaint();
    }
    
    private void ResetToOriginalPositions()
    {
        Transform[] objects = GetSelectedTransforms();
        if (objects.Length == 0) return;
        
        Undo.RecordObjects(objects, "Reset to Original Positions");
        
        foreach (Transform obj in objects)
        {
            if (originalPositions.ContainsKey(obj))
            {
                obj.position = originalPositions[obj];
            }
        }
        
        EditorUtility.SetDirty(objects[0]);
    }
}