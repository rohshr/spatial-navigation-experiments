using UnityEngine;
using UnityEditor;

public class GridDuplicator : EditorWindow
{
    [Header("Grid Settings")]
    public Vector3Int gridSize = new Vector3Int(3, 1, 3);
    public Vector3 spacing = new Vector3(2f, 2f, 2f);
    
    [Header("Organization")]
    public bool createParentGroup = true;
    public string parentName = "Grid_Group";
    
    [Header("Naming")]
    public bool useCoordinateNaming = true;
    public string namePrefix = "";
    
    private GameObject selectedObject;
    private Vector2 scrollPos;
    
    [MenuItem("Tools/Grid Duplicator")]
    static void ShowWindow()
    {
        GridDuplicator window = GetWindow<GridDuplicator>("Grid Duplicator");
        window.minSize = new Vector2(300, 400);
    }
    
    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Label("Grid Duplicator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Current Selection Display
        EditorGUILayout.LabelField("Current Selection:", EditorStyles.boldLabel);
        selectedObject = Selection.activeGameObject;
        
        if (selectedObject != null)
        {
            EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);
        }
        else
        {
            EditorGUILayout.HelpBox("Please select a GameObject in the scene to duplicate.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Grid Settings
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.Vector3IntField("Grid Size (X,Y,Z)", gridSize);
        
        // Ensure minimum values
        gridSize.x = Mathf.Max(1, gridSize.x);
        gridSize.y = Mathf.Max(1, gridSize.y);
        gridSize.z = Mathf.Max(1, gridSize.z);
        
        spacing = EditorGUILayout.Vector3Field("Spacing", spacing);
        
        // Calculate total objects
        int totalObjects = gridSize.x * gridSize.y * gridSize.z;
        EditorGUILayout.LabelField($"Total Objects: {totalObjects}", EditorStyles.miniLabel);
        
        if (totalObjects > 100)
        {
            EditorGUILayout.HelpBox($"Warning: Creating {totalObjects} objects. This might impact performance.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Organization Settings
        EditorGUILayout.LabelField("Organization", EditorStyles.boldLabel);
        createParentGroup = EditorGUILayout.Toggle("Create Parent Group", createParentGroup);
        
        if (createParentGroup)
        {
            parentName = EditorGUILayout.TextField("Parent Name", parentName);
        }
        
        EditorGUILayout.Space();
        
        // Naming Settings
        EditorGUILayout.LabelField("Naming", EditorStyles.boldLabel);
        useCoordinateNaming = EditorGUILayout.Toggle("Use Coordinate Naming", useCoordinateNaming);
        namePrefix = EditorGUILayout.TextField("Name Prefix", namePrefix);
        
        if (useCoordinateNaming)
        {
            EditorGUILayout.HelpBox("Objects will be named: [Prefix]OriginalName_X_Y_Z", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Preview Section
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        if (selectedObject != null)
        {
            Vector3 totalSize = new Vector3(
                (gridSize.x - 1) * spacing.x,
                (gridSize.y - 1) * spacing.y,
                (gridSize.z - 1) * spacing.z
            );
            EditorGUILayout.LabelField($"Grid Dimensions: {totalSize.x:F1} x {totalSize.y:F1} x {totalSize.z:F1}");
        }
        
        EditorGUILayout.Space();
        
        // Action Buttons
        GUI.enabled = selectedObject != null;
        
        if (GUILayout.Button("Create Grid", GUILayout.Height(30)))
        {
            CreateGrid();
        }
        
        GUI.enabled = true;
        
        if (GUILayout.Button("Reset to Defaults"))
        {
            ResetToDefaults();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void CreateGrid()
    {
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "No object selected!", "OK");
            return;
        }
        
        // Record undo state
        string undoName = $"Create Grid ({gridSize.x}x{gridSize.y}x{gridSize.z})";
        int undoGroupIndex = Undo.GetCurrentGroup();
        
        GameObject parentGroup = null;
        
        // Create parent group if requested
        if (createParentGroup)
        {
            parentGroup = new GameObject(parentName);
            parentGroup.transform.position = selectedObject.transform.position;
            Undo.RegisterCreatedObjectUndo(parentGroup, undoName);
        }
        
        Vector3 startPosition = selectedObject.transform.position;
        
        // Create grid
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    // Skip the original object position if it matches
                    if (x == 0 && y == 0 && z == 0)
                        continue;
                    
                    // Calculate position
                    Vector3 position = startPosition + new Vector3(
                        x * spacing.x,
                        y * spacing.y,
                        z * spacing.z
                    );
                    
                    // Create duplicate
                    GameObject duplicate = Instantiate(selectedObject);
                    duplicate.transform.position = position;
                    duplicate.transform.rotation = selectedObject.transform.rotation;
                    duplicate.transform.localScale = selectedObject.transform.localScale;
                    
                    // Set name
                    if (useCoordinateNaming)
                    {
                        duplicate.name = $"{namePrefix}{selectedObject.name}_{x}_{y}_{z}";
                    }
                    else
                    {
                        duplicate.name = $"{namePrefix}{selectedObject.name}";
                    }
                    
                    // Parent to group if created
                    if (parentGroup != null)
                    {
                        duplicate.transform.SetParent(parentGroup.transform);
                    }
                    
                    // Register for undo
                    Undo.RegisterCreatedObjectUndo(duplicate, undoName);
                }
            }
        }
        
        // Also parent the original object if creating a parent group
        if (createParentGroup && parentGroup != null)
        {
            Undo.SetTransformParent(selectedObject.transform, parentGroup.transform, undoName);
            
            // Rename original object to match coordinate system
            if (useCoordinateNaming)
            {
                Undo.RecordObject(selectedObject, undoName);
                selectedObject.name = $"{namePrefix}{selectedObject.name}_0_0_0";
            }
        }
        
        // Group undo operations
        Undo.CollapseUndoOperations(undoGroupIndex);
        
        // Select the parent group or keep original selection
        if (parentGroup != null)
        {
            Selection.activeGameObject = parentGroup;
        }
        
        Debug.Log($"Created grid: {gridSize.x}x{gridSize.y}x{gridSize.z} = {gridSize.x * gridSize.y * gridSize.z} objects");
    }
    
    void ResetToDefaults()
    {
        gridSize = new Vector3Int(3, 1, 3);
        spacing = new Vector3(2f, 2f, 2f);
        createParentGroup = true;
        parentName = "Grid_Group";
        useCoordinateNaming = true;
        namePrefix = "";
    }
    
    void OnSelectionChange()
    {
        // Refresh the window when selection changes
        Repaint();
    }
}