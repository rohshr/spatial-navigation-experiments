using UnityEngine;

[System.Serializable]
public class DialogTriggerConfig
{
    public string dialogKey;
    public bool showImmediately = true;
    public bool destroyTriggerAfterUse = true;
    public bool pauseCurrentFlow = true;
}

public class VRDialogTrigger : MonoBehaviour
{
    [Header("Trigger Configuration")]
    [SerializeField] private DialogTriggerConfig triggerConfig;
    
    [Header("Collision Settings")]
    [SerializeField] private bool requiresPlayerTag = true;
    [SerializeField] private string playerTag = "Player";
    
    private bool hasTriggered = false;
    private VRDialogFlowManager dialogManager;

    private void Start()
    {
        dialogManager = FindFirstObjectByType<VRDialogFlowManager>();
        if (dialogManager == null)
        {
            Debug.LogError("VRDialogTrigger: No VRDialogFlowManager found in scene!");
        }

        // Ensure this object has a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("VRDialogTrigger: No collider found, adding BoxCollider as trigger");
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("VRDialogTrigger: Collider is not set as trigger, enabling isTrigger");
            col.isTrigger = true;
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (hasTriggered || dialogManager == null) return;
    //
    //     // Check if it's the player
    //     if (requiresPlayerTag && !other.CompareTag(playerTag)) return;
    //
    //     // Trigger the dialog
    //     TriggerDialog();
    // }

    // private void TriggerDialog()
    // {
    //     hasTriggered = true;
    //     
    //     Debug.Log($"VRDialogTrigger: Triggering dialog '{triggerConfig.dialogKey}'");
    //     
    //     if (triggerConfig.pauseCurrentFlow)
    //     {
    //         dialogManager.PauseDialogFlow();
    //     }
    //
    //     dialogManager.ShowTriggeredDialog(triggerConfig.dialogKey, triggerConfig.showImmediately);
    //
    //     if (triggerConfig.destroyTriggerAfterUse)
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    // Public method to manually trigger
    // public void ManualTrigger()
    // {
    //     if (!hasTriggered)
    //     {
    //         TriggerDialog();
    //     }
    // }
}