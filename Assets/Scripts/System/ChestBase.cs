using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;


public class ChestBase : MonoBehaviour, IInventoryUI
{
    // Start is called before the first frame update
    public GameObject chestUI; // チェストのUI（開閉制御用）
    private bool isOpen = false;

    [SerializeField] private List<Image> slotImages; // スロットUIのImage（6個）
    [SerializeField] private Sprite emptySprite;     // 何もないときの画像
    public List<Button> slots;

    public List<ItemStack> chestItems = new();      // 実際の所持アイテム
    private int selectedIndex = 0;

    void Start()
    {
        if (chestUI != null)
        {
            chestUI.SetActive(false); // 最初は閉じとく
        }

        // 初期化：全部空にする
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            chestItems.Add(null);
        }
    }

    void Update()
    {
        if (IsPlayerInRange() && Input.GetMouseButtonDown(1)) // 右クリック
        {
            if (!isOpen)
            {
                OpenChest();
            }
            else
            {
                CloseChest();
            }
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        chestUI.SetActive(true);
        //   ChestUIManager.Instance.OpenChest(this); // グローバルで今開いてるチェスト管理するなら
    }

    private void CloseChest()
    {
        isOpen = false;
        chestUI.SetActive(false);
        //   ChestUIManager.Instance.CloseChest();
    }

    private bool IsPlayerInRange()
    {
        // 仮実装：常にtrueにしといてもいい
        // 将来、距離制限や範囲チェック入れたくなったらここ
        return true;
    }
    //個々からチェストのUI処理----------------------------------------------------------------------------------------------------------
    public bool TryAddItem(ItemData item)//空いているスロットに新しいアイテムを入れる　拾った時の処理
    {
       // for (int i = 0; i < chestItems.Count; i++)
        {
            //// 空スロットなら格納
            //if (chestItems[i] == null)
            //{
            //    chestItems[i] = item;
            //    slotImages[i].sprite = item.icon;
            //    return true;
            //}
            // 同じアイテムならスタック（拡張予定）

            //  合体試行はスタック可能なアイテムのみ
            if (item.isStackable)
            {
                for (int i = 0; i < chestItems.Count; i++)
                {
                    var stack = chestItems[i];

                    if (stack != null && stack.itemData != null)
                    {
                        if (stack.itemData == item)
                        {

                            stack.count++;
                            UpdateSlotVisual(i);
                            return true;
                        }
                    }
                }
            }

            //  スタックできなかった or スタック不可 → 空きスロットに追加
            for (int i = 0; i < chestItems.Count; i++)
            {
                Debug.Log(chestItems.Count);
                if (chestItems[i].itemData == null)
                {
                    chestItems[i] = new ItemStack(item, 1);

                    UpdateSlotVisual(i);
                    return true;
                }
            }
        }

        //Debug.Log("ホットバー満杯！");
        return false;
    }

    public int GetSlotIndex(Button button)//ボタンがリストの何番目？
    {
        return slots.IndexOf(button); // ボタンのリスト中のインデックスを返す
    }
    public int GetSlotCount()
    {
        return chestItems.Count;
    }
    public ItemData GetItemDataAt(int index)//指定スロットのアイテムデータを取得
    {
        if (chestItems[index] != null)
            return chestItems[index].itemData;
        return null;
    }

    public void SetItemAt(int index, ItemStack itemStack)//アイテムをセット　ドラッグの時
    {
        chestItems[index] = itemStack;
        UpdateSlotVisual(index);
    }

    public void ClearItemAt(int index)//スロットをリセット
    {
        chestItems[index] = null;
        slotImages[index].sprite = emptySprite;
        UpdateSlotVisual(index);
    }

    public ItemStack GetItemStackAt(int index)
    {
        if (index >= 0 && index < chestItems.Count)
            return chestItems[index];
        return null;
    }

    public void UpdateSlotVisual(int index)
    {
        Image iconImage = slots[index].GetComponent<Image>();

        if (chestItems[index] != null)
        {
            iconImage.sprite = chestItems[index].itemData.icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0); // 完全透明にする
        }
    }
}
