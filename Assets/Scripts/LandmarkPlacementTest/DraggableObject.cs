using UnityEngine;

namespace LandmarkPlacementTest
{
    public class DraggableObject : MonoBehaviour
    {
        private bool isDragging = false;
        private Vector3 dragOffset;
        private Camera cam;
    
        public string objectName; // For identification
    
        void Start()
        {
            cam = Camera.main;
        }
    
        void OnMouseDown()
        {
            isDragging = true;
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dragOffset = transform.position - mousePos;
        }
    
        void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                transform.position = mousePos + dragOffset;
            }
        }
    
        void OnMouseUp()
        {
            isDragging = false;
        
            // Check if we're over a target
            CheckForTarget();
        }
    
        void CheckForTarget()
        {
            // Simple collision check
            Collider2D hit = Physics2D.OverlapPoint(transform.position);
            if (hit != null && hit.CompareTag("Target"))
            {
                // Snap to target center
                transform.position = hit.transform.position;
            
                // Tell the game manager
                LandmarkPlacement.Instance.ObjectPlaced(objectName, hit.name);
            }
        }
    }
}
