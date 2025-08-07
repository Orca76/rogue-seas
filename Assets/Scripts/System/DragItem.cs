﻿using System;
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


    public GameObject ChartUI;
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
        Debug.Log("StartDrag called");  // ← ここで呼ばれてるかまず確認
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            Debug.Log("Raycast hit: " + result.gameObject.name); // ← 何かヒットしてるか確認
            Button clickedButton = result.gameObject.GetComponent<Button>();
            if (clickedButton != null)
            {
                Debug.Log("Button hit: " + clickedButton.gameObject.name); // ← ボタンまで到達できてる？
                currentInventoryUI = result.gameObject.GetComponentInParent<IInventoryUI>();
                draggingInventoryUI = currentInventoryUI;


                if (currentInventoryUI == null)
                    return; // 対応してないUIなら無視
                int index = currentInventoryUI.GetSlotIndex(clickedButton);
                // Debug.Log(index);
                if (index != -1 && currentInventoryUI.GetItemDataAt(index) != null)
                {
                    // ドラッグ開始
                    // ItemData itemData = currentInventoryUI.GetItemDataAt(index);
                    ItemStack itemStack = currentInventoryUI.GetItemDataAt(index); // ✅ 型を合わせる

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

                //ここから錬成処理

                if (dropButton.CompareTag("InputSlot"))
                {
                    var draggingItem = draggingInventoryUI.GetItemStackAt(draggingIndex);
                    if (draggingItem != null && draggingItem.count > 0)
                    {
                        // 一個だけ渡す
                        ItemStack oneItem = new ItemStack(
                            draggingItem.itemName,
                            draggingItem.icon,
                            1,
                            draggingItem.isStackable,
                            draggingItem.AVector
                        );



                        // 🔸 錬成台などに送る処理（仮）

                        if (ChartUI.activeSelf)//プレイヤー下の海図が表示されている→錬成盤は表示されていない
                        {
                            ChartVectorManager.Instance.ReceiveItem(oneItem);//海図の方にデータを渡す
                        }
                        else
                        {
                            AlchemyVectorManager.Instance.ReceiveItem(oneItem); // 錬成盤
                        }
                          

                        // 元スロットの個数を1減らす
                        draggingItem.count--;
                        if (draggingItem.count <= 0)
                        {
                            draggingInventoryUI.ClearItemAt(draggingIndex);
                        }

                        draggingInventoryUI.UpdateSlotVisual(draggingIndex);
                    }

                    break; // 投入口に落としたらループ終了
                }


                    //----------------ここから通常処理　ここから上が錬成処理


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
