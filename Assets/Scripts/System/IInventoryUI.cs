using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryUI
{
    int GetSlotIndex(Button button);
    ItemData GetItemDataAt(int index);
    void SetItemAt(int index, ItemData itemData);
    void ClearItemAt(int index);
    int GetSlotCount();
}
