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


    public int[] levels = new int[3];
    public int Exp;//経験値
    public int totalLevel=1;//総合レベル
    // 0:HP, 1:攻撃力, 2:攻撃速度

    int nextLevel;
    public GameObject LevelUpUI;//UI レベルアップ
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextLevel = 30;
    }

    void Update()
    {
        if (Exp > nextLevel)
        {
            if (!LevelUpUI.activeSelf)
            {
                //UI表示中ではない
                LevelUpUI.SetActive(true);

                //UIを出す
                LevelUpUI.GetComponent<LevelUpSelectionUI>().
                //Exp-NextLevel
                nextLevel = 30 * totalLevel * totalLevel;
            }
           

        }

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
