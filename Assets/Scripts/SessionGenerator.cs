using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;
using NUnit.Framework.Constraints;
using UnityEditor.EditorTools;

public class SessionGenerator : MonoBehaviour
{
    [System.Serializable]
    public class UXFBlock
    {
        [Tooltip("Unique name for UXF block")]
        public string blockName;
        public int trialCount;
        public enum EnvironmentType { Curved, Angled, Open_Space }
        public EnvironmentType environment; // Dropdown in the Unity Editor
    }

    [Tooltip("Enable this to run the experiment in non-VR mode. Useful for testing without VR headset.")]
    public bool NonVRMode = false;

    [Header("Session Settings")]
    [Tooltip("Specify block sequence and number of trials for each block")]
    [SerializeField] private UXFBlock[] blocks;
    // Session Start
    public void GenerateExperiment(Session session)
    {
        String locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString();
        String preferredHandFromUI = session.participantDetails["preferred_hand"].ToString();

        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
        session.settings.SetValue("preferred_hand", preferredHandFromUI);

        LocomotionMethod.UpdateFloors(locomotionMethodFromUI);

        if (!NonVRMode)
        {
            InputHandler.UpdateLocomotionControls(locomotionMethodFromUI);
        } else
        {
            Debug.Log("Dev mode is enabled. Skipping InputHandler.UpdateLocomotionControls()");
        }

        if (locomotionMethodFromUI.ToLower() == "continuous")
        {
            session.settings.SetValue("locomotion_method_instruction", "continuous_locomotion_instruction");
        }
        else
        {
            session.settings.SetValue("locomotion_method_instruction", locomotionMethodFromUI.ToLower() == "teleport" ? "teleport_locomotion_instruction" : "node_locomotion_instruction");
        }

        foreach (UXFBlock block in blocks)
        {
            // Create a block for each entry in the blocks array
            Block newBlock = session.CreateBlock(block.trialCount);
            newBlock.settings.SetValue("environment", block.environment.ToString().ToLower());
        }
    }

    public void EndExperiment()
    {
        // Wait for 5 seconds before ending the session
        StartCoroutine(EndSessionAfterDelay(5f));
    }

    private IEnumerator EndSessionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Session ended.");
        Session.instance.End();
    }
}
