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
                    // �h���b�O�J�n
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
        isDragging = false; // �h���b�O�t���O����

        dragImage.gameObject.SetActive(false); // �}�E�X�Ǐ]�A�C�R�����\��

        draggingIndex = -1; // ���h���b�O���Ă��X���b�g�ԍ������Z�b�g
    }
}
