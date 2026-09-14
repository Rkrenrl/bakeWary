using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat instance;

    [Header("UI")]
    public TextMeshProUGUI playerNicknameText;
    public TextMeshProUGUI upgradingLevelText;
    public Image upgradingLevelBar;

    RoomManager roomManager;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        roomManager = RoomManager.instance;
        playerNicknameText.text = roomManager.nickname;
    }

    void Update()
    {
        upgradingLevelText.text = roomManager.upgradingLevel.ToString();
        upgradingLevelBar.fillAmount = roomManager.upgradingXP / roomManager.upgradeRequireXP;
    }
}