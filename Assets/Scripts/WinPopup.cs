using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class WinPopup : MonoBehaviour
{
    [SerializeField] public TMP_Text Text;
    public Image background;
    public float displayTime = 5f;

    private System.Action onClose;

    public void Initialize(float duration, System.Action closeCallback)
    {
        background.enabled = true;
        background.raycastTarget = true;
        onClose = closeCallback;
        displayTime = duration;

        // Animation
        transform.localScale = Vector3.zero;
        background.color = new Color(0, 0, 0, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1, 0.5f).SetEase(Ease.OutBack));
        seq.Join(background.DOFade(0.7f, 0.5f));
        seq.OnComplete(() => {
            Invoke("ClosePopup", displayTime);
        });
    }

    public void ClosePopup()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0, 0.3f).SetEase(Ease.InBack));
        seq.Join(background.DOFade(0, 0.3f));
        seq.OnComplete(() => {
            if (onClose != null) onClose();
            Destroy(gameObject);
        });
    }
}