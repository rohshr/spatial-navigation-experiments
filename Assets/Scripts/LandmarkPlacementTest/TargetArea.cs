using UnityEngine;

namespace LandmarkPlacementTest
{
    public class TargetArea : MonoBehaviour
    {
        public string targetName;
        private SpriteRenderer spriteRenderer;
    
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.tag = "Target";
        
            // Add a collider for detection
            if (!GetComponent<Collider2D>())
            {
                gameObject.AddComponent<CircleCollider2D>();
                GetComponent<Collider2D>().isTrigger = true;
            }
        }
    
        // Visual feedback when objects hover over
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<DraggableObject>())
            {
                spriteRenderer.color = Color.yellow;
            }
        }
    
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<DraggableObject>())
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}
