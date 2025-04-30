using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ItemStack
{
    public ItemData itemData;
    public int count;

    public ItemStack(ItemData data, int count)
    {
        this.itemData = data;
        this.count = count;
    }
}