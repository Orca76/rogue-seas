using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayoutManager : MonoBehaviour
{

    public enum LayoutType
    {
        Normal,
        Inventory,
        Alchemy,
        Departure
    }

    [System.Serializable]
    public class UIElementLayout
    {
        public string name;
        public RectTransform target;

        public Vector2 position_Normal;
        public Vector2 position_Inventory;
        public Vector2 position_Alchemy;
        public Vector2 position_Departure;

        public bool activeInNormal = true;
        public bool activeInInventory = true;
        public bool activeInAlchemy = true;
        public bool activeInDeparture = true;
    }

    public List<UIElementLayout> elements = new();

    public void SetLayout(LayoutType layout)
    {
        foreach (var element in elements)
        {
            switch (layout)
            {
                case LayoutType.Normal:
                    element.target.anchoredPosition = element.position_Normal;
                    element.target.gameObject.SetActive(element.activeInNormal);
                    break;
                case LayoutType.Inventory:
                    element.target.anchoredPosition = element.position_Inventory;
                    element.target.gameObject.SetActive(element.activeInInventory);
                    break;
                case LayoutType.Alchemy:
                    element.target.anchoredPosition = element.position_Alchemy;
                    element.target.gameObject.SetActive(element.activeInAlchemy);
                    break;
                case LayoutType.Departure:
                    element.target.anchoredPosition = element.position_Departure;
                    element.target.gameObject.SetActive(element.activeInDeparture);
                    break;
            }
        }
    }

    private LayoutType currentLayout = LayoutType.Normal;


    private void Start()
    {
        SetLayout(currentLayout);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            // 次のレイアウトへ（Enumの繰り返し）
            currentLayout = (LayoutType)(((int)currentLayout + 1) % System.Enum.GetNames(typeof(LayoutType)).Length);
            SetLayout(currentLayout);
            Debug.Log("Switched to layout: " + currentLayout);
        }
    }

}