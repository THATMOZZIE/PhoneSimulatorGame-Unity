using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AppIcon : MonoBehaviour
{
    [Header("References")]
    public Image iconImage;
    public TextMeshProUGUI appNameText;
    public Button appButton;

    [Header("App Data")]
    public string appName;
    public AppType appType;

    [Header("Animation")]
    public float bounceScale = 1.1f;
    public float bounceDuration = 0.1f;

    public enum AppType
    {
        Messages,
        Pictures,
        Settings,
        Notes,
        About,
        Other
    }

    private PhoneManager phoneManager;

    void Start()
    {
        phoneManager = FindObjectOfType<PhoneManager>();
        SetupAppIcon();

        // Add button click listener
        appButton.onClick.AddListener(OnAppClicked);
    }

    void SetupAppIcon()
    {
        appNameText.text = appName;

        // Set icon color based on app type (temporary until we add real icons)
        switch (appType)
        {
            case AppType.Messages:
                iconImage.color = new Color(0.0f, 0.8f, 0.2f); // Green
                break;
            case AppType.Pictures:
                iconImage.color = new Color(1.0f, 0.6f, 0.0f); // Orange
                break;
            case AppType.Settings:
                iconImage.color = new Color(0.5f, 0.5f, 0.5f); // Gray
                break;
            default:
                iconImage.color = new Color(0.2f, 0.6f, 1.0f); // Blue
                break;
        }
    }

    void OnAppClicked()
    {
        // Bounce animation
        transform.DOScale(bounceScale, bounceDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(1f, bounceDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        OpenApp();
                    });
            });
    }

    void OpenApp()
    {
        switch (appType)
        {
            case AppType.Messages:
                phoneManager.ShowInstaProfileScreen(); // We'll add this method
                break;
            case AppType.Pictures:
                Debug.Log("Pictures app clicked - not implemented yet");
                break;
            case AppType.Settings:
                Debug.Log("Settings app clicked - not implemented yet");
                break;
            default:
                Debug.Log($"{appName} app clicked - not implemented yet");
                break;
        }
    }
}