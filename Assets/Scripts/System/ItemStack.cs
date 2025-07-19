using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ItemStack
{
    public string itemName;
    public Sprite icon;
    public int count = 1;
    public bool isStackable = true;

    public ItemStack(string name, Sprite icon, int count = 1, bool isStackable = true)
    {
        this.itemName = name;
        this.icon = icon;
        this.count = count;
        this.isStackable = isStackable;
    }
}