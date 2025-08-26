using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float moveSpeed = 5f; // インスペクタから調整できる速度
    public float accel = 10f;//移動加速度

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




    public TextMeshProUGUI[] LevelTexts;//レベルのUI

    int nextLevel;
    public GameObject LevelUpUI;//UI レベルアップ

    public Slider ExpSlider;
    public Image HPbar;//HPバー


    public float HP;
    public float MaxHP;

    public DamagePopup popupPrefab;

    public int VisitedIslandCount;//訪れた島の数
    public int NextDest;//次のバイオーム



    public GameObject ChartUI;//リロード後に参照を見失わないため
    public GameObject DragImage;//参照残し
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextLevel = 30;
        ExpSlider.interactable = false;
    }

   void HandleHPBar()
    {
        HPbar.fillAmount = HP / MaxHP;
    }

    void Update()
    {

        HandleHPBar();


        for (int i = 0; i < LevelTexts.Length && i < levels.Length; i++)
        {
            LevelTexts[i].text = "Lv"+levels[i].ToString();
        }
        ExpSlider.value = (float)Exp / nextLevel;
        Debug.Log($"Exp={Exp}, nextLevel={nextLevel}, Exp/nextLevel={(float)Exp / nextLevel}, SliderValue={ExpSlider.value}");


        if (Exp >= nextLevel)
        {
            if (!LevelUpUI.activeSelf)
            {
                //UI表示中ではない
                LevelUpUI.SetActive(true);

                //UIを出す
                // 時間を止める（ゲーム内のUpdateや物理挙動が止まる）
                Time.timeScale = 0f;

                // LevelUpUI.GetComponent<LevelUpSelectionUI>().
                //Exp-NextLevel
                int x = nextLevel;
                nextLevel = 30 * totalLevel * totalLevel;
                Exp -= x;//現在使った分の経験値を引く
            }
           

        }

        isUnderground = Mathf.Approximately(transform.position.z, undergroundZ);

        // 入力取得
        movement.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        movement.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        movement = movement.normalized;

        // 入力有無
        bool hasInput = movement.sqrMagnitude > 0.01f;

        // 目標速度
        Vector2 targetVelocity = movement * moveSpeed;

        // 移動（ぬるっと加速／即停止）
        if (hasInput)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, accel * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

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
      
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "EnemyAt")
        {
            HP -= collision.GetComponent<EnemyBullet>().Damage;
            var pos = transform.position;
            DamagePopup.Spawn(popupPrefab, pos, Mathf.RoundToInt(collision.GetComponent<EnemyBullet>().Damage));
            Destroy(collision.gameObject);
        }
    }
}
