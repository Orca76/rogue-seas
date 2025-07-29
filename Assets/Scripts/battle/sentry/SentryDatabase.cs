using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryDatabase : MonoBehaviour
{
    public static SentryDatabase instance;
    // Inspector�Őݒ肷��GameObject�z��i�e�ۃv���n�u�Ƃ��j
    public GameObject[] bulletTier1;
    public GameObject[] bulletTier2;
    public GameObject[] bulletTier3;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
