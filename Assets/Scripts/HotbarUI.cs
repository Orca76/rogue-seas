using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HotbarUI : MonoBehaviour
{
    [SerializeField] private List<Image> slotImages; // スロットUIのImage（6個）
    [SerializeField] private Sprite emptySprite;     // 何もないときの画像

    private List<ItemData> hotbarItems = new();      // 実際の所持アイテム
    private int selectedIndex = 0;

    void Start()
    {
        // 初期化：全部空にする
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            hotbarItems.Add(null);
        }

        UpdateSelectionHighlight();
    }

    void Update()
    {
        // 数字キー入力（1～6）
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                selectedIndex = i;
                UpdateSelectionHighlight();
            }
        }
    }

    void UpdateSelectionHighlight()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    public void SetItem(int index, ItemData item)
    {
        if (index < 0 || index >= slotImages.Count) return;

        hotbarItems[index] = item;
        slotImages[index].sprite = item != null ? item.icon : emptySprite;
    }

    public bool TryAddItem(ItemData item)
    {
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            // 空スロットなら格納
            if (hotbarItems[i] == null)
            {
                hotbarItems[i] = item;
                slotImages[i].sprite = item.icon;
                return true;
            }
            // 同じアイテムならスタック（拡張予定）
        }

        Debug.Log("ホットバー満杯！");
        return false;
    }
}
