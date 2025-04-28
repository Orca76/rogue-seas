using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;


public class HotbarUI : MonoBehaviour
{
    [SerializeField] private List<Image> slotImages; // スロットUIのImage（6個）
    [SerializeField] private Sprite emptySprite;     // 何もないときの画像
    public List<Button> slots;

    public List<ItemData> hotbarItems = new();      // 実際の所持アイテム
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

        // 右クリックで使用
        if (Input.GetMouseButtonDown(1)) // 右クリック
        {
            ItemData selected = GetSelectedItem();
            if (selected != null)
            {
                selected.Use();

                if (selected.isConsumable)
                {
                    RemoveItemAt(selectedIndex); // 消費タイプのみ削除
                }
               
            }
        }
    }
    public ItemData GetSelectedItem()
    {
        if (selectedIndex >= 0 && selectedIndex < hotbarItems.Count)
            return hotbarItems[selectedIndex];
        return null;
    }
    void UpdateSelectionHighlight()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].color = (i == selectedIndex) ? Color.white : Color.grey;
        }
    }

    public void SetItem(int index, ItemData item)
    {
        if (index < 0 || index >= slotImages.Count) return;

        hotbarItems[index] = item;
        slotImages[index].sprite = item != null ? item.icon : emptySprite;
    }
    void RemoveItemAt(int index)
    {
        hotbarItems[index] = null;
        slotImages[index].sprite = emptySprite;
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

        //Debug.Log("ホットバー満杯！");
        return false;
    }

    public int GetSlotIndex(Button button)
    {
        return slots.IndexOf(button); // ボタンのリスト中のインデックスを返す
    }

    public bool HasItemAt(int index)
    {
        return hotbarItems[index] != null;
    }

    public ItemData GetItemDataAt(int index)
    {
        return hotbarItems[index];
    }

    public void SetItemAt(int index, ItemData itemData)
    {
        hotbarItems[index] = itemData;
        UpdateSlotVisual(index);
    }

    public void ClearItemAt(int index)
    {
        hotbarItems[index] = null;
        UpdateSlotVisual(index);
    }

    private void UpdateSlotVisual(int index)
    {
        Image iconImage = slots[index].GetComponent<Image>();

        if (hotbarItems[index] != null)
        {
            iconImage.sprite = hotbarItems[index].icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // 完全透明にする
        }
    }

}
