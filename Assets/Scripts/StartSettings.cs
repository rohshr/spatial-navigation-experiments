using UnityEngine;

public class StartSettings : MonoBehaviour
{
    public GameObject xrOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xrOrigin.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
