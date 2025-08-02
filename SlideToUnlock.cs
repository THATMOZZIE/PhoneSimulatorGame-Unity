using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class SlideToUnlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public RectTransform slideButton;
    public RectTransform slideTrack;
    public TextMeshProUGUI slideText;
    public PhoneManager phoneManager;

    [Header("Settings")]
    public float unlockThreshold = 0.8f; // 80% of track width to unlock
    public float snapBackDuration = 0.3f;

    private Vector2 startPosition;
    private float trackWidth;
    private bool isUnlocked = false;

    void Start()
    {
        startPosition = slideButton.anchoredPosition;
        trackWidth = slideTrack.rect.width - slideButton.rect.width;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isUnlocked) return;

        // Stop any ongoing animations
        slideButton.DOKill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isUnlocked) return;

        // Convert screen point to local point
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            slideTrack, eventData.position, eventData.pressEventCamera, out localPoint);

        // Constrain to track bounds
        float newX = Mathf.Clamp(localPoint.x, startPosition.x, trackWidth);
        slideButton.anchoredPosition = new Vector2(newX, startPosition.y);

        // Update text opacity based on slide progress
        float progress = (newX - startPosition.x) / trackWidth;
        slideText.alpha = 1f - progress;

        // Check if we've reached unlock threshold
        if (progress >= unlockThreshold)
        {
            UnlockPhone();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isUnlocked) return;

        // Snap back to start position
        slideButton.DOAnchorPos(startPosition, snapBackDuration).SetEase(Ease.OutBack);
        slideText.DOFade(1f, snapBackDuration);
    }

    void UnlockPhone()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        // Animate button to end
        slideButton.DOAnchorPos(new Vector2(trackWidth, startPosition.y), 0.2f)
            .OnComplete(() =>
            {
                // Fade out the entire lock screen and show home screen
                GetComponentInParent<CanvasGroup>().DOFade(0f, 0.5f)
                    .OnComplete(() =>
                    {
                        phoneManager.ShowHomeScreen();
                        // Reset for next time
                        ResetSlider();
                    });
            });
    }

    void ResetSlider()
    {
        isUnlocked = false;
        if (slideButton != null) slideButton.anchoredPosition = startPosition;
        if (slideText != null) slideText.alpha = 1f;

        CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }
}