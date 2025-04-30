using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;


public class HotbarUI : MonoBehaviour,IInventoryUI
{
    [SerializeField] private List<Image> slotImages; // スロットUIのImage（6個）
    [SerializeField] private Sprite emptySprite;     // 何もないときの画像
    public List<Button> slots;

   public List<ItemData> hotbarItemData = new();      // 実際の所持アイテム
    public List<ItemStack> hotbarItems = new();
    public int selectedIndex = 0;

    void Start()
    {
        //   hotbarItems = new List<ItemStack>();
        Debug.Log($"slotImages.Count = {slotImages.Count}");

        Debug.Log($"hotbarItems.Count = {hotbarItems.Count}");
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            var s = hotbarItems[i];
            Debug.Log($"slot[{i}] = {(s == null ? "null" : (s.itemData == null ? "itemData null" : s.itemData.name + " x" + s.count))}");
        }
        // 初期化：全部空にする
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            hotbarItems.Add(null);
        }

        UpdateSelectionHighlight();

        for (int i = 0; i < hotbarItems.Count; i++)
        {
            var stack = hotbarItems[i];
            if (stack == null)
            {
                Debug.Log($"slot[{i}] = null");
            }
            else if (stack.itemData == null)
            {
                Debug.Log($"slot[{i}] = stackありだが itemDataがnull");
            }
            else
            {
                Debug.Log($"slot[{i}] = {stack.itemData.name}");
            }
        }

        Debug.Log($"hotbarItems.Count = {hotbarItems.Count}");
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            Debug.Log($"slot[{i}] = {(hotbarItems[i] == null ? "null" : hotbarItems[i].itemData.name)}");
        }

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
            ItemData selected = GetItemDataAt(selectedIndex);
            if (selected != null)
            {
                selected.Use();

                if (selected.isConsumable)
                {
                    ClearItemAt(selectedIndex); // 消費タイプのみ削除
                }

            }
        }
    }

    public int GetSlotCount()
    {
        return hotbarItems.Count;
    }

    void UpdateSelectionHighlight()//選択ちゅうスロットの色を更新
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].color = (i == selectedIndex) ? Color.white : Color.grey;
        }
    }


    public bool TryAddItem(ItemData newItem)
    {
        if (newItem == null)
        {
            Debug.LogError("TryAddItem: 渡された ItemData が null です");
            return false;
        }

        //  合体試行はスタック可能なアイテムのみ
        if (newItem.isStackable)
        {
            for (int i = 0; i < hotbarItems.Count; i++)
            {
                var stack = hotbarItems[i];
                if (stack != null && stack.itemData != null)
                {
                    if (stack.itemData == newItem)
                    {
                        stack.count++;
                        UpdateSlotVisual(i);
                        return true;
                    }
                }
            }
        }

        //  スタックできなかった or スタック不可 → 空きスロットに追加
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            if (hotbarItems[i] == null)
            {
                hotbarItems[i] = new ItemStack(newItem, 1);
                UpdateSlotVisual(i);
                return true;
            }
        }

        Debug.Log("ホットバー満杯！実際は空きなし or 合体失敗");
        return false;
    }




    public int GetSlotIndex(Button button)//ボタンがリストの何番目？
    {
        return slots.IndexOf(button); // ボタンのリスト中のインデックスを返す
    }

    public ItemData GetItemDataAt(int index)//指定スロットのアイテムデータを取得
    {
        if (hotbarItems[index] != null)
            return hotbarItems[index].itemData;
        return null;
    }

    public void SetItemAt(int index, ItemData itemData)//アイテムをセット　ドラッグの時
    {
        hotbarItems[index] = new ItemStack(itemData, 1);
        UpdateSlotVisual(index);
    }

    public void ClearItemAt(int index)//スロットをリセット
    {
        hotbarItems[index] = null;
        slotImages[index].sprite = emptySprite;
        UpdateSlotVisual(index);
    }

    private void UpdateSlotVisual(int index)
    {
        //Image iconImage = slots[index].GetComponent<Image>();

        //if (hotbarItems[index] != null)
        //{
        //    iconImage.sprite = hotbarItems[index].itemData.icon;

        //    iconImage.color = Color.white;
        //}
        //else
        //{
        //    iconImage.sprite = null;
        //    iconImage.color = new Color(1, 1, 1, 0); // 完全透明にする
        //}




        var stack = hotbarItems[index];
        Image iconImage = slots[index].GetComponent<Image>();
        TextMeshProUGUI countText = slots[index].transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        Debug.Log(countText);
        if (stack != null && stack.itemData != null)
        {
            iconImage.sprite = stack.itemData.icon;
            iconImage.color = Color.white;

            // スタック数が2以上のときだけ表示
            countText.text = stack.count > 1 ? $"x{stack.count}" : "";
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
            countText.text = "";
        }
    }

}
