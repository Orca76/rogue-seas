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
    public bool isStackable = true; // �X�^�b�N�\���ǂ����i�f�t�H���gtrue�j


    public enum ItemType
    {
        Potion,
        Tool,
        Block
    }

    public ItemType type; // Enum: Potion / Tool / Block �Ȃ�
    public int value;     // �|�[�V�����Ȃ�񕜗ʂȂ�

    public void Use()
    {
        switch (type)
        {
            case ItemType.Potion:
                Debug.Log($"�|�[�V���������񂾁I HP +{value}");
                break;
            case ItemType.Tool:
                Debug.Log("��͂���U�����I");
                break;
            case ItemType.Block:
                Debug.Log("�u���b�N��u�����Ƃ����I");
                break;
        }
    }
}

