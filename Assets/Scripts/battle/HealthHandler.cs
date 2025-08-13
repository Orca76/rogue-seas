using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{

    public float HP;
    public float Max;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EnemyAt")
        {
            HP -= collision.GetComponent<EnemyBullet>().Damage;
        }
    }
}
