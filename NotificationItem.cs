using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class NotificationItem : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("UI References")]
    public Image characterPortrait;
    public TextMeshProUGUI senderNameText;
    public TextMeshProUGUI messagePreviewText;
    public TextMeshProUGUI timeText;
    public Image backgroundImage;

    [Header("Colors")]
    public Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color pressedBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

    private NotificationData notificationData;
    private NotificationManager notificationManager;


    public void SetupNotification(NotificationData data, NotificationManager manager)
    {
        notificationData = data;
        notificationManager = manager;

        // Set UI elements
        senderNameText.text = data.senderName;
        messagePreviewText.text = "New message";

        // Only set time text if it exists
        if (timeText != null)
        {
            timeText.text = "now";
        }

        // Set character portrait
        if (data.characterPortrait != null)
        {
            characterPortrait.sprite = data.characterPortrait;
        }

        // Set background
        backgroundImage.color = normalBackgroundColor;

        // Make portrait circular
        SetupCircularPortrait();
    }

    void SetupCircularPortrait()
    {
        // Add mask component to make portrait circular
        if (characterPortrait.GetComponent<Mask>() == null)
        {
            characterPortrait.gameObject.AddComponent<Mask>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click - remove notification
            OnRightClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click - handle notification click
            OnLeftClick();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Visual feedback on press
        backgroundImage.DOColor(pressedBackgroundColor, 0.1f);
    }

    void OnLeftClick()
    {
        // Reset background color
        backgroundImage.DOColor(normalBackgroundColor, 0.1f);

        // Tell notification manager about the click
        notificationManager.OnNotificationClicked(notificationData);
    }

    void OnRightClick()
    {
        // Remove this notification
        notificationManager.RemoveNotification(notificationData);
    }
}