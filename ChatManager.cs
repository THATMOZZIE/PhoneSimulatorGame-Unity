using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class ChatMessage
{
    public string messageText;
    public bool isFromPlayer;
    public string senderId;
    public System.DateTime timestamp;

    public ChatMessage(string text, bool fromPlayer, string sender)
    {
        messageText = text;
        isFromPlayer = fromPlayer;
        senderId = sender;
        timestamp = System.DateTime.Now;
    }
}

[System.Serializable]
public class ChatConversation
{
    public string contactId;
    public string contactName;
    public List<ChatMessage> messages = new List<ChatMessage>();
    public bool isContactKnown = false;
}

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform chatContent;
    public GameObject aiMessageBubblePrefab;
    public GameObject playerMessageBubblePrefab;
    public Transform responseArea;
    public GameObject responseButtonPrefab;
    public ScrollRect chatScrollRect;

    [Header("Header References")]
    public Image characterPortraitHeader;
    public TextMeshProUGUI characterNameHeader;
    public TextMeshProUGUI statusText;
    public Button chatBackButton;

    [Header("Current Chat")]
    public string currentContactId;
    public ChatConversation currentConversation;

    private PhoneManager phoneManager;
    private NotificationManager notificationManager;
    private Dictionary<string, ChatConversation> allConversations = new Dictionary<string, ChatConversation>();

    void Start()
    {
        phoneManager = FindObjectOfType<PhoneManager>();
        notificationManager = FindObjectOfType<NotificationManager>();

        // Setup back button
        chatBackButton.onClick.AddListener(() => phoneManager.GoBack());
    }

    public void OpenChat(string contactId, string contactName = "???", bool isKnown = false)
    {
        currentContactId = contactId;

        // Get or create conversation
        if (!allConversations.ContainsKey(contactId))
        {
            allConversations[contactId] = new ChatConversation
            {
                contactId = contactId,
                contactName = contactName,
                isContactKnown = isKnown
            };
        }

        currentConversation = allConversations[contactId];

        // Update header
        UpdateChatHeader();

        // Load existing messages
        LoadChatHistory();

        // Start with a test message if it's a new conversation
        if (currentConversation.messages.Count == 0)
        {
            StartCoroutine(SendInitialMessage());
        }
    }

    void UpdateChatHeader()
    {
        characterNameHeader.text = currentConversation.contactName;
        statusText.text = currentConversation.isContactKnown ? "Online" : "Unknown Contact";

        // Set portrait (placeholder for now)
        // TODO: Set actual character portrait based on contactId
    }

    void LoadChatHistory()
    {
        // Clear existing UI messages
        foreach (Transform child in chatContent)
        {
            Destroy(child.gameObject);
        }

        // Create UI for each message in history
        foreach (ChatMessage message in currentConversation.messages)
        {
            CreateMessageBubble(message.messageText, message.isFromPlayer);
        }

        // Scroll to bottom
        StartCoroutine(ScrollToBottomDelayed());
    }

    IEnumerator SendInitialMessage()
    {
        yield return new WaitForSeconds(1f);

        // Send a test message from the AI
        string initialMessage = currentConversation.isContactKnown ?
            "Hey! How's it going?" :
            "Hello there! Do I know you?";

        ReceiveMessage(initialMessage);
    }

    public void ReceiveMessage(string messageText)
    {
        // Add to conversation history
        ChatMessage newMessage = new ChatMessage(messageText, false, currentContactId);
        currentConversation.messages.Add(newMessage);

        // Create UI bubble
        CreateMessageBubble(messageText, false);

        // Show response options after a delay
        StartCoroutine(ShowResponseOptionsDelayed());

        // Scroll to bottom
        StartCoroutine(ScrollToBottomDelayed());
    }

    public void SendPlayerMessage(string messageText)
    {
        // Add to conversation history
        ChatMessage newMessage = new ChatMessage(messageText, true, "player");
        currentConversation.messages.Add(newMessage);

        // Create UI bubble
        CreateMessageBubble(messageText, true);

        // Clear response buttons
        ClearResponseButtons();

        // AI responds after a delay
        StartCoroutine(AIResponseDelayed(messageText));

        // Scroll to bottom
        StartCoroutine(ScrollToBottomDelayed());
    }

    void CreateMessageBubble(string message, bool isFromPlayer)
    {
        GameObject bubblePrefab = isFromPlayer ? playerMessageBubblePrefab : aiMessageBubblePrefab;
        GameObject messageObj = Instantiate(bubblePrefab, chatContent);

        // Find the text component and set message
        TextMeshProUGUI messageText = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = message;
        }

        // Animate the message appearing
        messageObj.transform.localScale = Vector3.zero;
        messageObj.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    IEnumerator ShowResponseOptionsDelayed()
    {
        yield return new WaitForSeconds(1f);

        // Show some test response options
        ShowResponseOptions(new string[] {
            "That's interesting!",
            "Tell me more...",
            "I have to go now."
        });
    }

    void ShowResponseOptions(string[] options)
    {
        ClearResponseButtons();

        foreach (string option in options)
        {
            CreateResponseButton(option);
        }
    }

    void CreateResponseButton(string responseText)
    {
        GameObject buttonObj = Instantiate(responseButtonPrefab, responseArea);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = responseText;
        }

        button.onClick.AddListener(() => {
            SendPlayerMessage(responseText);
        });

        // Animate button appearing
        buttonObj.transform.localScale = Vector3.zero;
        buttonObj.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
    }

    void ClearResponseButtons()
    {
        foreach (Transform child in responseArea)
        {
            Destroy(child.gameObject);
        }
    }

    IEnumerator AIResponseDelayed(string playerMessage)
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));

        // Simple AI responses based on player input
        string[] responses = {
            "That's a good point!",
            "I see what you mean...",
            "Interesting perspective!",
            "What do you think about this?",
            "Let me tell you something..."
        };

        string aiResponse = responses[Random.Range(0, responses.Length)];
        ReceiveMessage(aiResponse);
    }

    IEnumerator ScrollToBottomDelayed()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}