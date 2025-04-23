using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDsata : MonoBehaviour
{
    // Start is called before the first frame update
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HotbarUI hotbar = FindObjectOfType<HotbarUI>();
            if (hotbar != null)
            {
              //  hotbar.TryAddItem(this.);
                Destroy(gameObject); // èEÇ¡ÇΩÇÁè¡Ç∑
            }
        }
    }
}
