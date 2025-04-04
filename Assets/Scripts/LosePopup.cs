using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class LosePopup : MonoBehaviour
{
    [SerializeField] public TMP_Text messageText;
    public Image background;
    public Button retryButton;
    public Button mainMenuButton;

    private System.Action onRetry;
    private System.Action onMainMenu;

    public void Initialize(System.Action onRetry, System.Action onMainMenu)
    {
        background.enabled = true;
        background.raycastTarget = true;
        this.onRetry = onRetry;
        this.onMainMenu = onMainMenu;

        // Animation
        transform.localScale = Vector3.zero;
        background.color = new Color(0, 0, 0, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1, 0.5f).SetEase(Ease.OutBack));
        seq.Join(background.DOFade(0.7f, 0.5f));

        // Button listeners
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    private void OnRetry()
    {
        if (onRetry != null) onRetry();
        Destroy(gameObject);
    }

    private void OnMainMenu()
    {
        if (onMainMenu != null) onMainMenu();
        Destroy(gameObject);
    }

    
}