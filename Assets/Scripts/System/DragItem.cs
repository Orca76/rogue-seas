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
    public Image dragImage; // マウス追従用の仮アイコン

    private int draggingIndex = -1; // 今ドラッグしてるアイテムのスロット番号
    private IInventoryUI currentInventoryUI;

    private IInventoryUI draggingInventoryUI; // どのインベントリから引っ張ったか
   
    void Start()
    {
        hotbarUI = FindObjectOfType<HotbarUI>(); // まず取得
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
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            Button clickedButton = result.gameObject.GetComponent<Button>();
            if (clickedButton != null)
            {
                currentInventoryUI = result.gameObject.GetComponentInParent<IInventoryUI>();
                draggingInventoryUI = currentInventoryUI;
              

                if (currentInventoryUI == null)
                    return; // 対応してないUIなら無視
                int index = currentInventoryUI.GetSlotIndex(clickedButton);
               // Debug.Log(index);
                if (index != -1 && currentInventoryUI.GetItemDataAt(index) != null)
                {
                    // ドラッグ開始
                    ItemData itemData = currentInventoryUI.GetItemDataAt(index);

                    dragImage.sprite = itemData.icon;
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
                    return; // 対応してないUIなら無視
                int dropIndex = currentInventoryUI.GetSlotIndex(dropButton);

               
                if (dropIndex != -1 && (currentInventoryUI.GetItemDataAt(dropIndex) == null))
                {
                    // 空きスロットなら、アイテム配置


                    var draggingItem = draggingInventoryUI.GetItemStackAt(draggingIndex);

                    Debug.Log(draggingItem);
                    currentInventoryUI.SetItemAt(dropIndex, draggingItem);
                    draggingInventoryUI.ClearItemAt(draggingIndex); // 元スロットは空にする

                    currentInventoryUI.UpdateSlotVisual(dropIndex);
                    currentInventoryUI.UpdateSlotVisual(draggingIndex);
                    break;
                }
            }
        }
        isDragging = false; // ドラッグフラグ解除

        dragImage.gameObject.SetActive(false); // マウス追従アイコンを非表示

        draggingIndex = -1; // 今ドラッグしてたスロット番号をリセット
    }
}
