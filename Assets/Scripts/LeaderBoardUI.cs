using UnityEngine;
using TMPro; // Required for TextMeshPro

public class LeaderboardUI : MonoBehaviour
{
    public TMP_Text leaderboardText; // Reference to TextMeshPro component
    public LeaderboardManager leaderboardManager; // Reference to the LeaderboardManager script

    void Update()
    {
        if (leaderboardManager != null && leaderboardText != null)
        {
            // Clear the text and rebuild the leaderboard display
            leaderboardText.text = "Leaderboard:\n";
            for (int i = 0; i < leaderboardManager.bestTimes.Count; i++)
            {
                leaderboardText.text += $"{i + 1}. {leaderboardManager.bestTimes[i]:F2} seconds\n";
            }
        }
    }
}
