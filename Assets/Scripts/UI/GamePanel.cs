using System.Collections;
using TMPro;
using UnityEngine;

public class GamePanel : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI comboText;

    float[] comboValues = { 1f, 1.5f, 2f, 2.5f };
    string[] comboNames = { "Perfect", "Great", "Good", "Missing" };
    private int comboTime = 1;
    private string lastCombo;
    private string currentCombo;

    public void ShowScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void ShowMultiplier(int multiplier)
    {
        multiplierText.text = "x" + multiplier;
    }

    public void ShowTimer(string value)
    {
        timeText.text = value;
    }

    public void ShowComboEffect(float value)
    {
        StopCoroutine("HideCombo");
        StartCoroutine("HideCombo");

        comboText.gameObject.SetActive(true);
        for (int i = 0; i < comboValues.Length; i++)
        {
            if (value < comboValues[i])
            {
                currentCombo = comboNames[i];
                Debug.Log(currentCombo);
                if (currentCombo == lastCombo)
                {
                    comboTime++;
                }
                else
                {
                    comboTime = 1;
                }

                lastCombo = currentCombo;
                var combo = "";
                if (i < comboValues.Length - 1)
                {
                    combo = comboTime > 1 ? " X " + comboTime : "";
                }
                comboText.text = comboNames[i] + combo;
                break;
            }
        }
    }

    IEnumerator HideCombo()
    {
        yield return new WaitForSeconds(2);
        comboText.gameObject.SetActive(false);
    }
}