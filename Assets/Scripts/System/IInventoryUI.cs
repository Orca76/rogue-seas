using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryUI
{
    int GetSlotIndex(Button button);
    ItemData GetItemDataAt(int index);
    void SetItemAt(int index, ItemStack itemData);
    void ClearItemAt(int index);
    int GetSlotCount();
    void UpdateSlotVisual(int index);
    ItemStack GetItemStackAt(int index);
}
