using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;


public class HotbarUI : MonoBehaviour
{
    [SerializeField] private List<Image> slotImages; // �X���b�gUI��Image�i6�j
    [SerializeField] private Sprite emptySprite;     // �����Ȃ��Ƃ��̉摜
    public List<Button> slots;

    public List<ItemData> hotbarItems = new();      // ���ۂ̏����A�C�e��
    private int selectedIndex = 0;

    void Start()
    {
        // �������F�S����ɂ���
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            hotbarItems.Add(null);
        }

        UpdateSelectionHighlight();
    }

    void Update()
    {
        // �����L�[���́i1�`6�j
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                selectedIndex = i;
                UpdateSelectionHighlight();
            }
        }

        // �E�N���b�N�Ŏg�p
        if (Input.GetMouseButtonDown(1)) // �E�N���b�N
        {
            ItemData selected = GetSelectedItem();
            if (selected != null)
            {
                selected.Use();

                if (selected.isConsumable)
                {
                    RemoveItemAt(selectedIndex); // ����^�C�v�̂ݍ폜
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
            // ��X���b�g�Ȃ�i�[
            if (hotbarItems[i] == null)
            {
                hotbarItems[i] = item;
                slotImages[i].sprite = item.icon;
                return true;
            }
            // �����A�C�e���Ȃ�X�^�b�N�i�g���\��j
        }

        //Debug.Log("�z�b�g�o�[���t�I");
        return false;
    }

    public int GetSlotIndex(Button button)
    {
        return slots.IndexOf(button); // �{�^���̃��X�g���̃C���f�b�N�X��Ԃ�
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
            iconImage.color = new Color(1, 1, 1, 0); // ���S�����ɂ���
        }
    }

}
