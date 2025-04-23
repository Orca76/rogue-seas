using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HotbarUI : MonoBehaviour
{
    [SerializeField] private List<Image> slotImages; // �X���b�gUI��Image�i6�j
    [SerializeField] private Sprite emptySprite;     // �����Ȃ��Ƃ��̉摜

    private List<ItemData> hotbarItems = new();      // ���ۂ̏����A�C�e��
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
    }

    void UpdateSelectionHighlight()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    public void SetItem(int index, ItemData item)
    {
        if (index < 0 || index >= slotImages.Count) return;

        hotbarItems[index] = item;
        slotImages[index].sprite = item != null ? item.icon : emptySprite;
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

        Debug.Log("�z�b�g�o�[���t�I");
        return false;
    }
}
