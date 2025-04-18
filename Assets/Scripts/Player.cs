using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float moveSpeed = 5f; // インスペクタから調整できる速度

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Floor Switch Settings")]
    public float surfaceZ = 0f;
    public float undergroundZ = 1f;
    public KeyCode switchFloorKey = KeyCode.U;
  public  bool isUnderground;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isUnderground = Mathf.Approximately(transform.position.z, undergroundZ);

        // 入力取得
        movement.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        movement.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        movement = movement.normalized;

        // 地上/地下の切り替え
        if (Input.GetKeyDown(switchFloorKey))
        {
            float currentZ = transform.position.z;
            float newZ = Mathf.Approximately(currentZ, surfaceZ) ? undergroundZ : surfaceZ;
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
        }
    }

    void FixedUpdate()
    {
        // 移動
        rb.velocity = movement * moveSpeed;
    }
}
