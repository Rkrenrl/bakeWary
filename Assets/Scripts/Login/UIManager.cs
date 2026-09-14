using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Screens")]
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject lobbyUI;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    void ClearAllScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        lobbyUI.SetActive(false);
    }

    // Update is called once per frame
    public void LoginScreen()
    {
        ClearAllScreen();
        loginUI.SetActive(true);
    }
    public void RegisterScreen()
    {   
        ClearAllScreen();
        registerUI.SetActive(true);
    }

    public void LobbyScreen()
    {
        ClearAllScreen();
        lobbyUI.SetActive(true);
    }
}
