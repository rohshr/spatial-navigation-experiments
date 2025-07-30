using System;
using UnityEngine;

public class VRNoPeeking : MonoBehaviour
{
    [SerializeField] public LayerMask collisionLayer;
    [SerializeField] public float fadeSpeed;
    [SerializeField] public float sphereCheckSize = 0.15f;
    
    private Material cameraFadeMaterial;
    private bool isCameraFadeOut = false;

    private void Awake()
    {
        cameraFadeMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.CheckSphere(transform.position, sphereCheckSize, collisionLayer, QueryTriggerInteraction.Ignore))
        {
            CameraFade(1f);
            isCameraFadeOut = true;
        }
        else
        {
            if(!isCameraFadeOut) return;
            
            CameraFade(0f);
        }
    }
    
    public void CameraFade(float targetAlpha)
    {
        var fadeValue = Mathf.MoveTowards(cameraFadeMaterial.GetFloat("_AlphaValue"), targetAlpha, Time.deltaTime * fadeSpeed);
        cameraFadeMaterial.SetFloat("_AlphaValue", fadeValue);
        
        if(fadeValue <= 0.01f)
        {
            isCameraFadeOut = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.5f, 0.7f, 0.75f);
        Gizmos.DrawSphere(transform.position, sphereCheckSize);
    }
}
