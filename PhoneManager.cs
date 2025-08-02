using UnityEngine;
using DG.Tweening;



public class PhoneManager : MonoBehaviour
{
    [Header("Screen References")]
    public GameObject lockScreen;
    public GameObject homeScreen;
    public GameObject messagesScreen;
    public GameObject chatScreen;
    public GameObject instaProfileScreen;

    [Header("Current State")]
    public PhoneState currentState = PhoneState.Locked;

    private NotificationManager notificationManager;

    public enum PhoneState
    {
        Locked,
        Home,
        Messages,
        Chat,
        InstaProfile
    }

    void Start()
    {
        // Initialize - show only lock screen
        ShowLockScreen();

        notificationManager = FindObjectOfType<NotificationManager>();

    }

    public void ShowLockScreen()
    {
        SetActiveScreen(lockScreen);
        currentState = PhoneState.Locked;
    }

    public void ShowHomeScreen()
    {
        SetActiveScreen(homeScreen);
        currentState = PhoneState.Home;
    }

    public void ShowMessagesScreen()
    {
        SetActiveScreen(messagesScreen);
        currentState = PhoneState.Messages;
    }

    public void ShowChatScreen()
    {
        SetActiveScreen(chatScreen);
        currentState = PhoneState.Chat;
    }

    public void ShowInstaProfileScreen()
    {
        SetActiveScreen(instaProfileScreen);
        currentState = PhoneState.InstaProfile;
    }


    void SetActiveScreen(GameObject activeScreen)
    {
        // Hide all screens first
        if (lockScreen != null) lockScreen.SetActive(false);
        if (homeScreen != null) homeScreen.SetActive(false);
        if (messagesScreen != null) messagesScreen.SetActive(false);
        if (chatScreen != null) chatScreen.SetActive(false);
        if (instaProfileScreen != null) instaProfileScreen.SetActive(false);

        // Show the active screen
        if (activeScreen != null)
        {
            activeScreen.SetActive(true);
        }

        // Update the current state based on which screen is active
        if (activeScreen == lockScreen) currentState = PhoneState.Locked;
        else if (activeScreen == homeScreen) currentState = PhoneState.Home;
        else if (activeScreen == messagesScreen) currentState = PhoneState.Messages;
        else if (activeScreen == chatScreen) currentState = PhoneState.Chat;
        else if (activeScreen == instaProfileScreen) currentState = PhoneState.InstaProfile;

        // Notify notification manager about screen change
        if (notificationManager != null)
        {
            notificationManager.OnScreenChanged(currentState);
        }
    }
    // Method to go back (like pressing back button)
    public void GoBack()
    {
        switch (currentState)
        {
            case PhoneState.Chat:
                ShowMessagesScreen();
                break;
            case PhoneState.Messages:
                ShowHomeScreen();
                break;
            case PhoneState.Home:
                ShowLockScreen();
                break;
            case PhoneState.InstaProfile:
                ShowHomeScreen();
                break;
        }
    }
}