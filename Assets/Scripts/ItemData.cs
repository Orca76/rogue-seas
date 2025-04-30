using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public bool isConsumable = false; // 
    public bool isStackable = true; // スタック可能かどうか（デフォルトtrue）


    public enum ItemType
    {
        Potion,
        Tool,
        Block
    }

    public ItemType type; // Enum: Potion / Tool / Block など
    public int value;     // ポーションなら回復量など

    public void Use()
    {
        switch (type)
        {
            case ItemType.Potion:
                Debug.Log($"ポーションを飲んだ！ HP +{value}");
                break;
            case ItemType.Tool:
                Debug.Log("つるはしを振った！");
                break;
            case ItemType.Block:
                Debug.Log("ブロックを置こうとした！");
                break;
        }
    }
}

