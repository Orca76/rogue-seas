using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;


public class ChestBase : MonoBehaviour, IInventoryUI
{
    // Start is called before the first frame update
    public GameObject chestUI; // �`�F�X�g��UI�i�J����p�j
    private bool isOpen = false;

    [SerializeField] private List<Image> slotImages; // �X���b�gUI��Image�i6�j
    [SerializeField] private Sprite emptySprite;     // �����Ȃ��Ƃ��̉摜
    public List<Button> slots;

    public List<ItemStack> chestItems = new();      // ���ۂ̏����A�C�e��
    private int selectedIndex = 0;

    void Start()
    {
        if (chestUI != null)
        {
            chestUI.SetActive(false); // �ŏ��͕��Ƃ�
        }

        // �������F�S����ɂ���
        for (int i = 0; i < slotImages.Count; i++)
        {
            slotImages[i].sprite = emptySprite;
            chestItems.Add(null);
        }
    }

    void Update()
    {
        if (IsPlayerInRange() && Input.GetMouseButtonDown(1)) // �E�N���b�N
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
        //   ChestUIManager.Instance.OpenChest(this); // �O���[�o���ō��J���Ă�`�F�X�g�Ǘ�����Ȃ�
    }

    private void CloseChest()
    {
        isOpen = false;
        chestUI.SetActive(false);
        //   ChestUIManager.Instance.CloseChest();
    }

    private bool IsPlayerInRange()
    {
        // �������F���true�ɂ��Ƃ��Ă�����
        // �����A����������͈̓`�F�b�N���ꂽ���Ȃ����炱��
        return true;
    }
    //�X����`�F�X�g��UI����----------------------------------------------------------------------------------------------------------
    public bool TryAddItem(ItemData item)//�󂢂Ă���X���b�g�ɐV�����A�C�e��������@�E�������̏���
    {
       // for (int i = 0; i < chestItems.Count; i++)
        {
            //// ��X���b�g�Ȃ�i�[
            //if (chestItems[i] == null)
            //{
            //    chestItems[i] = item;
            //    slotImages[i].sprite = item.icon;
            //    return true;
            //}
            // �����A�C�e���Ȃ�X�^�b�N�i�g���\��j

            //  ���̎��s�̓X�^�b�N�\�ȃA�C�e���̂�
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

            //  �X�^�b�N�ł��Ȃ����� or �X�^�b�N�s�� �� �󂫃X���b�g�ɒǉ�
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

        //Debug.Log("�z�b�g�o�[���t�I");
        return false;
    }

    public int GetSlotIndex(Button button)//�{�^�������X�g�̉��ԖځH
    {
        return slots.IndexOf(button); // �{�^���̃��X�g���̃C���f�b�N�X��Ԃ�
    }
    public int GetSlotCount()
    {
        return chestItems.Count;
    }
    public ItemData GetItemDataAt(int index)//�w��X���b�g�̃A�C�e���f�[�^���擾
    {
        if (chestItems[index] != null)
            return chestItems[index].itemData;
        return null;
    }

    public void SetItemAt(int index, ItemStack itemStack)//�A�C�e�����Z�b�g�@�h���b�O�̎�
    {
        chestItems[index] = itemStack;
        UpdateSlotVisual(index);
    }

    public void ClearItemAt(int index)//�X���b�g�����Z�b�g
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
            iconImage.color = new Color(1, 1, 1, 0); // ���S�����ɂ���
        }
    }
}
