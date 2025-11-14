// using UnityEngine;
//
// public class RegisterableObject : MonoBehaviour
// {
//     [SerializeField] private string objectId;
//     
//     private void Awake()
//     {
//         if (string.IsNullOrEmpty(objectId))
//             objectId = gameObject.name;
//             
//         SceneObjectRegistry.Instance?.RegisterObject(objectId, gameObject);
//     }
//     
//     private void OnDestroy()
//     {
//         SceneObjectRegistry.Instance?.UnregisterObject(objectId);
//     }
// }
