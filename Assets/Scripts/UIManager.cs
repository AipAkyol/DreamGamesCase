using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text goalText;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateMovesText(int moves)
    {
        if (movesText != null)
        {
            movesText.text = $"{moves}";
        }
    }
    public void UpdateGoalText(int goal)
    {
        if (goalText != null)
        {
            goalText.text = $"{goal}";
        }
    }

    
}