using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour
{
    private HotbarUI hotbarUI;
    private bool isDragging = false;
    public Image dragImage; // �}�E�X�Ǐ]�p�̉��A�C�R��

    private int draggingIndex = -1; // ���h���b�O���Ă�A�C�e���̃X���b�g�ԍ�
    private IInventoryUI currentInventoryUI;

    private IInventoryUI draggingInventoryUI; // �ǂ̃C���x���g�����������������
   
    void Start()
    {
        hotbarUI = FindObjectOfType<HotbarUI>(); // �܂��擾
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            StartDrag();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }

        if (isDragging)
        {
            dragImage.transform.position = Input.mousePosition;
        }
    }
    void StartDrag()
    {
        Debug.Log("StartDrag called");  // �� �����ŌĂ΂�Ă邩�܂��m�F
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            Debug.Log("Raycast hit: " + result.gameObject.name); // �� �����q�b�g���Ă邩�m�F
            Button clickedButton = result.gameObject.GetComponent<Button>();
            if (clickedButton != null)
            {
                Debug.Log("Button hit: " + clickedButton.gameObject.name); // �� �{�^���܂œ��B�ł��Ă�H
                currentInventoryUI = result.gameObject.GetComponentInParent<IInventoryUI>();
                draggingInventoryUI = currentInventoryUI;
              

                if (currentInventoryUI == null)
                    return; // �Ή����ĂȂ�UI�Ȃ疳��
                int index = currentInventoryUI.GetSlotIndex(clickedButton);
               // Debug.Log(index);
                if (index != -1 && currentInventoryUI.GetItemDataAt(index) != null)
                {
                    // �h���b�O�J�n
                    // ItemData itemData = currentInventoryUI.GetItemDataAt(index);
                    ItemStack itemStack = currentInventoryUI.GetItemDataAt(index); // ? �^�����킹��

                    Debug.Log($"Slot Index: {index}, ItemData: {itemStack?.itemName ?? "null"}");
                    dragImage.sprite = itemStack.icon;
                    dragImage.color = Color.white;
                    dragImage.gameObject.SetActive(true);

                    draggingIndex = index;
                    isDragging = true;
                    return;
                }
            }
        }
    }
    void EndDrag()
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
          
            Button dropButton = result.gameObject.GetComponent<Button>();
            if (dropButton != null)
            {

                currentInventoryUI = result.gameObject.GetComponentInParent<IInventoryUI>();

                if (currentInventoryUI == null)
                    return; // �Ή����ĂȂ�UI�Ȃ疳��
                int dropIndex = currentInventoryUI.GetSlotIndex(dropButton);

               
                if (dropIndex != -1 && (currentInventoryUI.GetItemDataAt(dropIndex) == null))
                {
                    // �󂫃X���b�g�Ȃ�A�A�C�e���z�u


                    var draggingItem = draggingInventoryUI.GetItemStackAt(draggingIndex);

                    Debug.Log(draggingItem);
                    currentInventoryUI.SetItemAt(dropIndex, draggingItem);
                    draggingInventoryUI.ClearItemAt(draggingIndex); // ���X���b�g�͋�ɂ���

                    currentInventoryUI.UpdateSlotVisual(dropIndex);
                    currentInventoryUI.UpdateSlotVisual(draggingIndex);
                    break;
                }
            }
        }
        isDragging = false; // �h���b�O�t���O����

        dragImage.gameObject.SetActive(false); // �}�E�X�Ǐ]�A�C�R�����\��

        draggingIndex = -1; // ���h���b�O���Ă��X���b�g�ԍ������Z�b�g
    }
}
