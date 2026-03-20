using System.Collections.Generic;
using UnityEngine;

// Stores all dialogue in one place as a dictionary of string arrays.
// Key = dialogue ID used by DialogueTrigger, Value = array of lines.
// Future improvement: move to ScriptableObjects for easier editing without code.
public class Dialogue : MonoBehaviour
{
    public Dictionary<string, string[]> dialogue = new Dictionary<string, string[]>();

    void Start()
    {
        // --- Arena entrance door ---
        dialogue.Add("LockedArenaA", new string[]
        {
            "The arena awaits...",
            "Are you sure you're ready to enter?"
        });
        dialogue.Add("LockedArenaB", new string[]
        {
            "Good luck in there."
        });

        // --- Hub upgrade merchant ---
        dialogue.Add("MerchantA", new string[]
        {
            "Welcome back, gladiator.",
            "I can make you stronger — for a price.",
            "What'll it be?"
        });
        dialogue.Add("MerchantAChoice1", new string[]
        {
            "",
            "",
            "Upgrade my health.",
        });
        dialogue.Add("MerchantAChoice2", new string[]
        {
            "",
            "",
            "Upgrade my damage."
        });
        dialogue.Add("MerchantB", new string[]
        {
            "Done. You're stronger now.",
            "Come back when you have more coin."
        });

        // --- Hub resting NPC ---
        dialogue.Add("RestingNPCA", new string[]
        {
            "I used to fight in the arena too.",
            "The enemies get stronger every round.",
            "Every third round... something worse shows up.",
            "Stay sharp."
        });

        dialogue.Add("LockedArenaAChoice1", new string[]
        {
            "",
            "Yes, I'm ready."
        });
        dialogue.Add("LockedArenaAChoice2", new string[]
        {
            "",
            "Not yet."
        });
    }
}