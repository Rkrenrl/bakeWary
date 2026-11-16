using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    [Header("Reference")]
    public WeaponSwitcher weaponSwitcher;

    [Header("Slots")]
    public GameObject[] selectedFrames; // 각 슬롯의 SelectedFrame
    public Image[] slotIcons;           // 각 슬롯의 Icon
    public RectTransform[] slotRects;   // 선택 시 살짝 커지는 연출용

    [Header("Throwable Icons (레벨 순서)")]
    public Sprite[] throwableIcons;     // 0번=뒤집개, 1번=밀대, 2번=휘핑기

    [Header("Selected Effect")]
    public float selectedScale = 1.15f;
    public float scaleLerpSpeed = 15f;

    void Update()
    {
        if (weaponSwitcher == null) return;

        int selected = weaponSwitcher.selectedWeapon;

        for (int i = 0; i < selectedFrames.Length; i++)
        {
            bool isSelected = (i == selected);

            selectedFrames[i].SetActive(isSelected);

            // 선택된 칸은 살짝 커지게 (부드럽게 보간)
            if (slotRects != null && i < slotRects.Length)
            {
                float target = isSelected ? selectedScale : 1f;
                slotRects[i].localScale = Vector3.Lerp(
                    slotRects[i].localScale,
                    Vector3.one * target,
                    scaleLerpSpeed * Time.deltaTime);
            }
        }

        // 0번 슬롯(원거리 무기)은 업그레이드 레벨에 맞춰 아이콘 교체
        if (throwableIcons.Length > 0 && slotIcons.Length > 0)
        {
            int idx = Mathf.Clamp(RoomManager.instance.upgradingLevel - 1, 0, throwableIcons.Length - 1);
            slotIcons[0].sprite = throwableIcons[idx];
        }
    }
}