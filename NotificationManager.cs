using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


// testing remove block
#if UNITY_EDITOR
using UnityEditor;
#endif
// testing remove block

[System.Serializable]
public class NotificationData
{
    public string senderName;
    public string messagePreview;
    public Sprite characterPortrait;
    public string contactId;
    public bool isUnknown;

    public NotificationData(string sender, string message, string id, bool unknown = false)
    {
        senderName = sender;
        messagePreview = message;
        contactId = id;
        isUnknown = unknown;
        characterPortrait = null; // Will be set by NotificationManager
    }
}

public class NotificationManager : MonoBehaviour
{


    [Header("UI References")]
    public Transform notificationContainer;
    public GameObject notificationPrefab;
    public Sprite defaultUnknownPortrait;

    [Header("Settings")]
    public int maxStackedNotifications = 3;
    public int maxListNotifications = 5;
    public float stackSpacing = 10f;
    public float notificationHeight = 80f;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public float stackedOpacity1 = 1f;      // Top notification
    public float stackedOpacity2 = 0.7f;    // Second notification  
    public float stackedOpacity3 = 0.4f;    // Third notification

    private List<NotificationData> notifications = new List<NotificationData>();
    private List<GameObject> notificationObjects = new List<GameObject>();
    private bool isInListMode = false;
    private PhoneManager phoneManager;

    void Start()
    {
        phoneManager = FindObjectOfType<PhoneManager>();

        // Test notification after a delay
        Invoke(nameof(SendTestNotification), 3f);
    }

    void SendTestNotification()
    {
        Debug.Log($"SendTestNotification called. Current state: {phoneManager.currentState}");

        // Only send if we're on home screen
        if (phoneManager.currentState == PhoneManager.PhoneState.Home)
        {
            Debug.Log("Adding test notification...");
            AddNotification("???", "Hey there! How's it going?", "unknown_1", true);
        }
        else
        {
            Debug.Log("Not on home screen, skipping notification");
        }
    }

    public void AddNotification(string senderName, string messagePreview, string contactId, bool isUnknown = false)
    {
        Debug.Log($"AddNotification called: {senderName} - {messagePreview}");

        NotificationData newNotification = new NotificationData(senderName, messagePreview, contactId, isUnknown);
        newNotification.characterPortrait = isUnknown ? defaultUnknownPortrait : GetCharacterPortrait(contactId);

        notifications.Insert(0, newNotification); // Add to front (most recent)

        CreateNotificationUI(newNotification);
        RefreshNotificationDisplay();

        Debug.Log($"Notification added. Total notifications: {notifications.Count}");
    }

    void CreateNotificationUI(NotificationData data)
    {
        GameObject notificationObj = Instantiate(notificationPrefab, notificationContainer);
        NotificationItem notificationItem = notificationObj.GetComponent<NotificationItem>();

        notificationItem.SetupNotification(data, this);
        notificationObjects.Insert(0, notificationObj);

        // Start the notification off-screen and small
        RectTransform rectTransform = notificationObj.GetComponent<RectTransform>();
        Vector3 finalPosition = rectTransform.anchoredPosition;

        // Set initial state (small and below screen)
        rectTransform.anchoredPosition = new Vector2(finalPosition.x, finalPosition.y - 150f);
        rectTransform.localScale = Vector3.one * 0.3f;

        // Add slight delay for stacking effect if multiple notifications
        float delay = notificationObjects.Count > 1 ? (notificationObjects.Count - 1) * 0.1f : 0f;

        // Animate to final position with scale
        Sequence slideSequence = DOTween.Sequence();
        slideSequence.SetDelay(delay);
        slideSequence.Append(rectTransform.DOAnchorPos(finalPosition, 0.5f).SetEase(Ease.OutBack));
        slideSequence.Join(rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

        // Optional: Add a subtle bounce at the end
        slideSequence.AppendCallback(() => {
            rectTransform.DOPunchScale(Vector3.one * 0.05f, 0.2f, 2, 0.5f);
        });
    }

    public void RemoveNotification(NotificationData data)
    {
        int index = notifications.IndexOf(data);
        if (index >= 0)
        {
            notifications.RemoveAt(index);

            if (index < notificationObjects.Count)
            {
                GameObject objToRemove = notificationObjects[index];
                notificationObjects.RemoveAt(index);

                // Animate removal
                objToRemove.transform.DOScale(0f, animationDuration)
                    .OnComplete(() => {
                        Destroy(objToRemove);
                        RefreshNotificationDisplay();
                    });
            }
        }
    }

    public void ToggleListMode()
    {
        isInListMode = !isInListMode;
        RefreshNotificationDisplay();
    }

    void RefreshNotificationDisplay()
    {
        // Small delay to let entry animations finish
        DOVirtual.DelayedCall(0.6f, () => {
            if (isInListMode)
            {
                DisplayAsList();
            }
            else
            {
                DisplayAsStack();
            }
        });
    }

    void DisplayAsStack()
    {
        int visibleCount = Mathf.Min(notifications.Count, maxStackedNotifications);

        for (int i = 0; i < notificationObjects.Count; i++)
        {
            if (i < visibleCount)
            {
                notificationObjects[i].SetActive(true);

                // Position
                float yOffset = -i * stackSpacing;
                notificationObjects[i].transform.DOLocalMoveY(yOffset, animationDuration);

                // Opacity
                CanvasGroup canvasGroup = notificationObjects[i].GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = notificationObjects[i].AddComponent<CanvasGroup>();

                float targetOpacity = GetStackOpacity(i);
                canvasGroup.DOFade(targetOpacity, animationDuration);
            }
            else
            {
                notificationObjects[i].SetActive(false);
            }
        }
    }

    void DisplayAsList()
    {
        int visibleCount = Mathf.Min(notifications.Count, maxListNotifications);

        for (int i = 0; i < notificationObjects.Count; i++)
        {
            if (i < visibleCount)
            {
                notificationObjects[i].SetActive(true);

                // Position in list
                float yOffset = -i * notificationHeight;
                notificationObjects[i].transform.DOLocalMoveY(yOffset, animationDuration);

                // Full opacity in list mode
                CanvasGroup canvasGroup = notificationObjects[i].GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = notificationObjects[i].AddComponent<CanvasGroup>();
                canvasGroup.DOFade(1f, animationDuration);
            }
            else
            {
                notificationObjects[i].SetActive(false);
            }
        }
    }

    float GetStackOpacity(int index)
    {
        switch (index)
        {
            case 0: return stackedOpacity1;
            case 1: return stackedOpacity2;
            case 2: return stackedOpacity3;
            default: return 0f;
        }
    }

    Sprite GetCharacterPortrait(string contactId)
    {
        // TODO: Implement character portrait lookup
        return defaultUnknownPortrait;
    }

    public void OnNotificationClicked(NotificationData data)
    {
        if (isInListMode)
        {
            // Open specific chat
            Debug.Log($"Opening chat with {data.senderName}");
            phoneManager.ShowChatScreen();
            // TODO: phoneManager.ShowChatScreen(data.contactId);

            // Tell ChatManager to open this specific conversation
            ChatManager chatManager = FindObjectOfType<ChatManager>();
            if (chatManager != null)
            {
                chatManager.OpenChat(data.contactId, data.senderName, !data.isUnknown);
            }
        }

        else
        {
            // Switch to list mode
            ToggleListMode();
        }
    }

    // Called by PhoneManager when switching screens
    public void OnScreenChanged(PhoneManager.PhoneState newState)
    {
        // Hide notifications when not on home screen
        bool shouldShow = (newState == PhoneManager.PhoneState.Home);
        notificationContainer.gameObject.SetActive(shouldShow);
    }


    // Method that can be called from inspector or other scripts
    public void SendRandomTestNotification()
    {
        string[] testNames = { "???", "Claire", "Nathan", "Ashley", "Tara" };
        string[] testMessages = { "Hey there!", "How's it going?", "What's up?", "Miss you!", "Are you free tonight?" };

        int randomIndex = Random.Range(0, testNames.Length);
        AddNotification(testNames[randomIndex], testMessages[randomIndex], "test_" + Random.Range(0, 1000), testNames[randomIndex] == "???");
    }


    // Test method - remove this later
    [ContextMenu("Send Test Notification")]
    public void SendAnotherTestNotification()
    {
        string[] testNames = { "???", "Claire", "Nathan", "Ashley" };
        string[] testMessages = { "Hey there!", "How's it going?", "What's up?", "Miss you!" };

        int randomIndex = Random.Range(0, testNames.Length);
        AddNotification(testNames[randomIndex], testMessages[randomIndex], "test_" + randomIndex, testNames[randomIndex] == "???");
    }


}

#if UNITY_EDITOR
[CustomEditor(typeof(NotificationManager))]
public class NotificationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        NotificationManager manager = (NotificationManager)target;
        
        if (Application.isPlaying)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Send Random Test Notification"))
            {
                manager.SendRandomTestNotification();
            }
        }
    }
}
#endif