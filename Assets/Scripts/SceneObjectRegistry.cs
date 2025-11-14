// using UnityEngine;
// using System.Collections.Generic;
//
// public class SceneObjectRegistry : MonoBehaviour
// {
//     private static SceneObjectRegistry _instance;
//     public static SceneObjectRegistry Instance
//     {
//         get
//         {
//             if (_instance == null)
//                 _instance = FindFirstObjectByType<SceneObjectRegistry>();
//             return _instance;
//         }
//     }
//     
//     private Dictionary<string, GameObject> registeredObjects = new Dictionary<string, GameObject>();
//     
//     public void RegisterObject(string id, GameObject obj)
//     {
//         if (registeredObjects.ContainsKey(id))
//         {
//             Debug.LogWarning($"Object with ID {id} already registered. Overwriting.");
//         }
//         registeredObjects[id] = obj;
//     }
//     
//     public void UnregisterObject(string id)
//     {
//         registeredObjects.Remove(id);
//     }
//     
//     public GameObject GetObject(string id)
//     {
//         registeredObjects.TryGetValue(id, out GameObject obj);
//         return obj;
//     }
//     
//     public T GetComponent<T>(string id) where T : Component
//     {
//         var obj = GetObject(id);
//         return obj?.GetComponent<T>();
//     }
// }
