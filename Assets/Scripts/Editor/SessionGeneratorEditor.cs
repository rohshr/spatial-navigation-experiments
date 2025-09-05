// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;
//
// [CustomEditor(typeof(SessionGenerator))]
// public class SessionGeneratorEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
//
//         SessionGenerator generator = (SessionGenerator)target;
//         
//         GUILayout.Space(10);
//         GUILayout.Label("Quick Block Creation", EditorStyles.boldLabel);
//         
//         GUILayout.BeginHorizontal();
//         if (GUILayout.Button("Create Generic Block"))
//         {
//             CreateBlock<LocomotionExperimentBlock>("GenericBlock");
//         }
//         
//         if (GUILayout.Button("Create Object Search Block"))
//         {
//             CreateBlock<ObjectSearchBlock>("ObjectSearchBlock");
//         }
//         
//         if (GUILayout.Button("Create Exploration Block"))
//         {
//             CreateBlock<TimedExplorationBlock>("ExplorationBlock");
//         }
//         GUILayout.EndHorizontal();
//     }
//
//     private void CreateBlock<T>(string baseName) where T : LocomotionExperimentBlock
//     {
//         T block = ScriptableObject.CreateInstance<T>();
//         block.blockName = baseName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
//         
//         string path = EditorUtility.SaveFilePanelInProject(
//             "Save Block Asset",
//             block.blockName,
//             "asset",
//             "Choose where to save the block asset"
//         );
//         
//         if (!string.IsNullOrEmpty(path))
//         {
//             AssetDatabase.CreateAsset(block, path);
//             AssetDatabase.SaveAssets();
//             
//             // Optionally add it to the list automatically
//             SessionGenerator generator = (SessionGenerator)target;
//             // Note: You'll need to make experimentBlocks public or add a method to add blocks
//             generator.AddBlock(block);
//             EditorUtility.SetDirty(generator);
//         }
//     }
// }
// #endif