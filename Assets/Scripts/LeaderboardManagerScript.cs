using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public int maxEntries = 10; // Maximum leaderboard entries
    private const string LeaderboardKey = "Leaderboard"; // Base key for PlayerPrefs

    public List<float> bestTimes = new List<float>(); // List to store times during runtime

    void Start()
    {
        LoadLeaderboard();
    }

    // Add a new time to the leaderboard
    public void AddTime(float time)
    {
        // Add the new time to the list
        bestTimes.Add(time);

        // Sort the times in ascending order (smallest first)
        bestTimes.Sort();

        // Keep only the top 'maxEntries' times
        if (bestTimes.Count > maxEntries)
        {
            bestTimes.RemoveAt(bestTimes.Count - 1);
        }

        // Save the leaderboard
        SaveLeaderboard();
    }

    // Save the leaderboard to PlayerPrefs
    private void SaveLeaderboard()
    {
        PlayerPrefs.SetInt($"{LeaderboardKey}_Count", bestTimes.Count);

        for (int i = 0; i < bestTimes.Count; i++)
        {
            PlayerPrefs.SetFloat($"{LeaderboardKey}_{i}", bestTimes[i]);
        }

        PlayerPrefs.Save(); // Save changes to disk
        Debug.Log("Leaderboard saved!");
    }

    // Load the leaderboard from PlayerPrefs
    private void LoadLeaderboard()
    {
        bestTimes.Clear(); // Clear the list before loading

        int count = PlayerPrefs.GetInt($"{LeaderboardKey}_Count", 0);

        for (int i = 0; i < count; i++)
        {
            float time = PlayerPrefs.GetFloat($"{LeaderboardKey}_{i}", 0);
            bestTimes.Add(time);
        }

        Debug.Log("Leaderboard loaded!");
    }

    // Debug function to display the leaderboard
    public void DisplayLeaderboard()
    {
        Debug.Log("Leaderboard:");
        for (int i = 0; i < bestTimes.Count; i++)
        {
            Debug.Log($"{i + 1}. {bestTimes[i]:F2} seconds");
        }
    }
}
