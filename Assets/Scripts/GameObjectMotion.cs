using UnityEngine;

public class GameObjectMotion : MonoBehaviour
{
    public float bobbingSpeed = 2f; // Speed of the bobbing motion
    public float bobbingHeight = 0.5f; // Height of the bobbing motion
    public float rotationSpeed = 50f; // Speed of rotation along the Y-axis

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        // Store the initial position of the GameObject
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Bobbing motion
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Rotation along the Y-axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
