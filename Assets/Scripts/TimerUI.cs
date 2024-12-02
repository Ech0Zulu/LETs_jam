using UnityEngine;
using TMPro; // Nécessaire pour TextMeshPro

public class TimerUI : MonoBehaviour
{
    public TMP_Text timerText; // Référence au texte TimerText
    public FinishAreaScript finishArea;   // Référence à l'EndArea pour accéder au timer

    void Update()
    {
        if (finishArea != null && timerText != null)
        {
            // Met à jour le texte avec le timer formaté (2 décimales)
            timerText.text = $"{finishArea.timer:F2}s";
        }
    }
}
