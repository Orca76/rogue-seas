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

                
                int index = hotbarUI.GetSlotIndex(clickedButton);
                Debug.Log(index);
                if (index != -1 && hotbarUI.HasItemAt(index))
                {
                    // ドラッグ開始
                    ItemData itemData = hotbarUI.GetItemDataAt(index);

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
                int dropIndex = hotbarUI.GetSlotIndex(dropButton);
                if (dropIndex != -1 && !hotbarUI.HasItemAt(dropIndex))
                {
                    // 空きスロットなら、アイテム配置
                    var draggingItem = hotbarUI.GetItemDataAt(draggingIndex);
                    hotbarUI.SetItemAt(dropIndex, draggingItem);
                    hotbarUI.ClearItemAt(draggingIndex); // 元スロットは空にする
                    break;
                }
            }
        }
        isDragging = false; // ドラッグフラグ解除

        dragImage.gameObject.SetActive(false); // マウス追従アイコンを非表示

        draggingIndex = -1; // 今ドラッグしてたスロット番号をリセット
    }
}
