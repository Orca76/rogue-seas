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
    private float moveSpeed = 5f; // �C���X�y�N�^���璲���ł��鑬�x
    public float accel = 10f;//�ړ������x

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Floor Switch Settings")]
    public float surfaceZ = 0f;
    public float undergroundZ = 1f;
    public KeyCode switchFloorKey = KeyCode.U;
  public  bool isUnderground;


    public int[] levels = new int[3];
    public int Exp;//�o���l
    public int totalLevel=1;//�������x��
    // 0:HP, 1:�U����, 2:�U�����x




    public TextMeshProUGUI[] LevelTexts;//���x����UI

    int nextLevel;
    public GameObject LevelUpUI;//UI ���x���A�b�v

    public Slider ExpSlider;
    public Image HPbar;//HP�o�[


    public float HP;
    public float MaxHP;

    public DamagePopup popupPrefab;

    public int VisitedIslandCount;//�K�ꂽ���̐�
    public int NextDest;//���̃o�C�I�[��



    public GameObject ChartUI;//�����[�h��ɎQ�Ƃ�������Ȃ�����
    public GameObject DragImage;//�Q�Ǝc��
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
                //UI�\�����ł͂Ȃ�
                LevelUpUI.SetActive(true);

                //UI���o��
                // ���Ԃ��~�߂�i�Q�[������Update�╨���������~�܂�j
                Time.timeScale = 0f;

                // LevelUpUI.GetComponent<LevelUpSelectionUI>().
                //Exp-NextLevel
                int x = nextLevel;
                nextLevel = 30 * totalLevel * totalLevel;
                Exp -= x;//���ݎg�������̌o���l������
            }
           

        }

        isUnderground = Mathf.Approximately(transform.position.z, undergroundZ);

        // ���͎擾
        movement.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        movement.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        movement = movement.normalized;

        // ���͗L��
        bool hasInput = movement.sqrMagnitude > 0.01f;

        // �ڕW���x
        Vector2 targetVelocity = movement * moveSpeed;

        // �ړ��i�ʂ���Ɖ����^����~�j
        if (hasInput)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, accel * Time.deltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        // �n��/�n���̐؂�ւ�
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
