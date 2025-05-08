using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UXF;
using NUnit.Framework.Constraints;

public class SessionGenerator : MonoBehaviour
{
    // Session Start
    public void GenerateExperiment(Session session)
    {
        // String testString = session.settings.GetString("test_string");
        // Debug.Log("Test string: " + testString);
        String locomotionMethod = session.settings.GetString("locomotion_method");
        String locomotionMethodFromUI = session.participantDetails["locomotion_method"].ToString();
        String preferredHandFromUI = session.participantDetails["preferred_hand"].ToString();
        // Debug.Log("Locomotion method from settings: " + locomotionMethod);
        Debug.Log("Locomotion method from UI: " + locomotionMethodFromUI);
        Debug.Log("Preferred hand from UI: " + preferredHandFromUI);
        session.settings.SetValue("locomotion_method", locomotionMethodFromUI);
        session.settings.SetValue("preferred_hand", preferredHandFromUI);

        LocomotionMethod.UpdateFloors(locomotionMethodFromUI);
        InputHandler.UpdateLocomotionControls(locomotionMethodFromUI);

        if (locomotionMethodFromUI.ToLower() == "continuous")
        {
            // InputHandler.UpdateHandPreference(preferredHandFromUI, true); // Smooth motion for continuous locomotion
            session.settings.SetValue("locomotion_method_instruction", "continuous_locomotion_instruction");
        }
        else
        {
            // InputHandler.UpdateHandPreference(preferredHandFromUI, false); // Snap motion for teleport and node-based locomotion
            session.settings.SetValue("locomotion_method_instruction", locomotionMethodFromUI.ToLower() == "teleport" ? "teleport_locomotion_instruction" : "node_locomotion_instruction");
        }

        // Curved corridor practice block
        Block curvedPracticeBlock = session.CreateBlock(1);
        curvedPracticeBlock.settings.SetValue("environment", "curved_corridor_practice");
        // curvedPracticeBlock.settings.SetValue("next_instruction_set", new List<string> { "angled_corridor_briefing" });
        // InstructionsController.UpdateInstructionSet(new List<string> { "curved_corridor_briefing" });
        
        // Angle corridor practice block
        Block angledPracticeBlock = session.CreateBlock(1);
        angledPracticeBlock.settings.SetValue("environment", "angled_corridor_practice");
        // angledPracticeBlock.settings.SetValue("next_instruction_set", new List<string> { "open_space_briefing" });
        // InstructionsController.UpdateInstructionSet(new List<string> { "angled_corridor_briefing" });
        
        // Open space practice block
        Block openSpacePracticeBlock = session.CreateBlock(4);
        openSpacePracticeBlock.settings.SetValue("environment", "open_space_practice");
        // openSpacePracticeBlock.settings.SetValue("next_instruction_set", new List<string> { "find_cube", "find_sphere", "find_statue", "find_star" });
        // InstructionsController.UpdateInstructionSet(new List<string> { "open_space_briefing" });

        // Block routeFindingBlock = session.CreateBlock(nRouteFindingTrials);
        // routeFindingBlock.settings.SetValue("condition", "route-finding");
    }
}
