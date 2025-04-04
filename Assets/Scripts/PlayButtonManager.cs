using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayButtonManager : MonoBehaviour
{
    public Button playButton;
    public TMP_Text playButtonText;

    public int currentLevel;
    private int maxLevel = 10;

    void Start()
    {
        // Load saved level progress, default to level 1 if no saved progress
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        UpdateButtonText();

        playButton.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        if (currentLevel > maxLevel)
        {
            return; // Do nothing if all levels are completed
        }

        SceneManager.LoadScene("LevelScene");
    }

    public void CompleteLevel()
    {
        UpdateButtonText();
    }

    public void UpdateButtonText()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        if (currentLevel > maxLevel)
        {
            playButtonText.text = "Finished";
        }
        else
        {
            playButtonText.text = "Level " + currentLevel;
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        currentLevel = 1;
        UpdateButtonText();
    }
}
