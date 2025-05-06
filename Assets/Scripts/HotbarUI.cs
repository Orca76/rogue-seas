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
    [SerializeField] private List<Image> slotImages; // �X���b�gUI��Image�i6�j
    [SerializeField] private Sprite emptySprite;     // �����Ȃ��Ƃ��̉摜
    public List<Button> slots;

 //  public List<ItemData> hotbarItemData = new();      // ���ۂ̏����A�C�e��
    public List<ItemStack> hotbarItems = new();//�A�C�e���ƌ��������Ă�
    public int selectedIndex = 0;

    void Start()
    {

        // �������F�S����ɂ���
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            hotbarItems.Add(new ItemStack(null, 0)); 
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
            ItemStack selectedStack = GetItemStackAt(selectedIndex); // �� �����AItemStack�Ŏ��I
            if (selectedStack != null && selectedStack.itemData != null)
            {
                selectedStack.itemData.Use();

                if (selectedStack.itemData.isConsumable)
                {
                    selectedStack.count--;

                    if (selectedStack.count <= 0)
                    {
                        ClearItemAt(selectedIndex); // �X�^�b�N���[���Ȃ�X���b�g����ɂ���
                    }
                    else
                    {
                        UpdateSlotVisual(selectedIndex); // �X�^�b�N���������炷
                    }
                }
            }
        }
    }

    public int GetSlotCount()
    {
        return hotbarItems.Count;
    }

    void UpdateSelectionHighlight()//�I�����イ�X���b�g�̐F���X�V
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
            Debug.LogError("TryAddItem: �n���ꂽ ItemData �� null �ł�");
            return false;
        }

        //  ���̎��s�̓X�^�b�N�\�ȃA�C�e���̂�
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

        //  �X�^�b�N�ł��Ȃ����� or �X�^�b�N�s�� �� �󂫃X���b�g�ɒǉ�
        for (int i = 0; i < hotbarItems.Count; i++)
        {
            Debug.Log(hotbarItems.Count);
            if (hotbarItems[i].itemData == null)
            {
                hotbarItems[i] = new ItemStack(newItem, 1);
                
                UpdateSlotVisual(i);
                return true;
            }
        }

        Debug.Log("�z�b�g�o�[���t�I���ۂ͋󂫂Ȃ� or ���̎��s");
        return false;
    }




    public int GetSlotIndex(Button button)//�{�^�������X�g�̉��ԖځH
    {
        return slots.IndexOf(button); // �{�^���̃��X�g���̃C���f�b�N�X��Ԃ�
    }

    public ItemData GetItemDataAt(int index)//�w��X���b�g�̃A�C�e���f�[�^���擾
    {
        if (hotbarItems[index] != null)
            return hotbarItems[index].itemData;
        return null;
    }

    public void SetItemAt(int index, ItemStack itemStack)//�A�C�e�����Z�b�g�@�h���b�O�̎�
    {
        hotbarItems[index] = itemStack;
        UpdateSlotVisual(index);
    }

    public void ClearItemAt(int index)//�X���b�g�����Z�b�g
    {
        hotbarItems[index] = null;
        slotImages[index].sprite = emptySprite;
        UpdateSlotVisual(index);
    }

    public ItemStack GetItemStackAt(int index)
    {
        if (index >= 0 && index < hotbarItems.Count)
            return hotbarItems[index];
        return null;
    }

    public void UpdateSlotVisual(int index)
    {
        var stack = hotbarItems[index];
        Image iconImage = slots[index].GetComponent<Image>();
        TextMeshProUGUI countText = slots[index].transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

        Debug.Log(countText);
        if (stack != null && stack.itemData != null)
        {
            iconImage.sprite = stack.itemData.icon;
            iconImage.color = Color.white;

            // �X�^�b�N����2�ȏ�̂Ƃ������\��
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
