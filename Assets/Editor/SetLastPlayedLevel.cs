using UnityEditor;
using UnityEngine;

public class LastPlayedLevelEditor : EditorWindow
{
    private string levelInput = ""; // Stores user input

    [MenuItem("Game Settings/Set Last Played Level")]
    private static void ShowWindow()
    {
        GetWindow<LastPlayedLevelEditor>("Set Last Played Level");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter the last played level number:", EditorStyles.boldLabel);
        levelInput = EditorGUILayout.TextField("Level Number:", levelInput);

        if (GUILayout.Button("Save"))
        {
            if (int.TryParse(levelInput, out int level))
            {
                if (level < 1 || level > 10)
                {
                    Debug.LogWarning("Level must be between 1 and 10!");
                    EditorUtility.DisplayDialog("Warning", "Level must be between 1 and 10!", "OK");
                    return;
                }

                PlayerPrefs.SetInt("CurrentLevel", level);
                PlayerPrefs.Save();

                PlayButtonManager playButtonManager = FindFirstObjectByType<PlayButtonManager>();
                if (playButtonManager != null)
                {
                    playButtonManager.currentLevel = level;
                    playButtonManager.UpdateButtonText();
                }

                Debug.Log($"Last played level set to: {level}");
                Close();
            }
            else
            {
                Debug.LogWarning("Invalid input! Please enter a number.");
                EditorUtility.DisplayDialog("Error", "Please enter a valid number!", "OK");
            }
        }
    }
}
