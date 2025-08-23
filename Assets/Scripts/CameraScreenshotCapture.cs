using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraScreenshotCapture : MonoBehaviour
{
    [Header("Screenshot Settings")]
    public int screenshotWidth = 1920;
    public int screenshotHeight = 1080;
    public string screenshotPath = "Screenshots";

    [Header("Camera Reference")]
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            Debug.LogError("CameraScreenshotCapture: No Camera component found!");
        }
    }

    [ContextMenu("Take Screenshot")]
    public void TakeScreenshot()
    {
        if (targetCamera == null)
        {
            Debug.LogError("No camera found!");
            return;
        }

        Debug.Log("Taking screenshot...");
        
        #if UNITY_EDITOR
        // In editor, call directly without coroutine
        CaptureScreenshotDirect();
        #else
        // In play mode, use coroutine
        if (Application.isPlaying)
        {
            StartCoroutine(CaptureScreenshot());
        }
        else
        {
            CaptureScreenshotDirect();
        }
        #endif
    }

    private void CaptureScreenshotDirect()
    {
        // Ensure screenshots directory exists
        string fullPath = Path.Combine(Application.dataPath, screenshotPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Debug.Log($"Created directory: {fullPath}");
        }

        // Create filename with timestamp
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"{gameObject.name}_screenshot_{timestamp}.png";
        string filePath = Path.Combine(fullPath, filename);

        // Store original camera settings
        RenderTexture originalTarget = targetCamera.targetTexture;
        RenderTexture originalActive = RenderTexture.active;

        // Create temporary render texture
        RenderTexture tempRT = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        tempRT.Create();

        try
        {
            // Set camera to render to our texture
            targetCamera.targetTexture = tempRT;
            RenderTexture.active = tempRT;

            // Force render
            targetCamera.Render();

            // Create texture to read pixels into
            Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            screenshot.Apply();

            // Convert to PNG and save
            byte[] pngData = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);

            Debug.Log($"Screenshot saved successfully: {filePath}");

            // Cleanup screenshot texture
            #if UNITY_EDITOR
            DestroyImmediate(screenshot);
            #else
            Destroy(screenshot);
            #endif

            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to take screenshot: {e.Message}");
        }
        finally
        {
            // Always restore original settings
            targetCamera.targetTexture = originalTarget;
            RenderTexture.active = originalActive;

            // Cleanup render texture
            if (tempRT != null)
            {
                tempRT.Release();
                #if UNITY_EDITOR
                DestroyImmediate(tempRT);
                #else
                Destroy(tempRT);
                #endif
            }
        }
    }

    private System.Collections.IEnumerator CaptureScreenshot()
    {
        yield return new WaitForEndOfFrame();
        CaptureScreenshotDirect();
    }

    public void TakeScreenshotWithCustomName(string customName)
    {
        if (targetCamera == null) return;
        
        #if UNITY_EDITOR
        CaptureScreenshotWithNameDirect(customName);
        #else
        if (Application.isPlaying)
        {
            StartCoroutine(CaptureScreenshotWithName(customName));
        }
        else
        {
            CaptureScreenshotWithNameDirect(customName);
        }
        #endif
    }

    private void CaptureScreenshotWithNameDirect(string customName)
    {
        string fullPath = Path.Combine(Application.dataPath, screenshotPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"{customName}_{timestamp}.png";
        string filePath = Path.Combine(fullPath, filename);

        RenderTexture originalTarget = targetCamera.targetTexture;
        RenderTexture originalActive = RenderTexture.active;

        RenderTexture tempRT = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        tempRT.Create();

        try
        {
            targetCamera.targetTexture = tempRT;
            RenderTexture.active = tempRT;
            targetCamera.Render();

            Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
            screenshot.Apply();

            byte[] pngData = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);

            Debug.Log($"Screenshot saved: {filePath}");

            #if UNITY_EDITOR
            DestroyImmediate(screenshot);
            AssetDatabase.Refresh();
            #else
            Destroy(screenshot);
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to take screenshot: {e.Message}");
        }
        finally
        {
            targetCamera.targetTexture = originalTarget;
            RenderTexture.active = originalActive;

            if (tempRT != null)
            {
                tempRT.Release();
                #if UNITY_EDITOR
                DestroyImmediate(tempRT);
                #else
                Destroy(tempRT);
                #endif
            }
        }
    }

    private System.Collections.IEnumerator CaptureScreenshotWithName(string customName)
    {
        yield return new WaitForEndOfFrame();
        CaptureScreenshotWithNameDirect(customName);
    }
}