using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class AlchemyVectorManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameObject alchemyTileObject; // Alchemy�^�C���{��
    public KeyCode generateVectorKey = KeyCode.I;
    public KeyCode resetKey = KeyCode.T;

    [Header("Vector Settings")]
    public GameObject vectorPrefab; // LineRenderer�����v���n�u
    public float vectorMin = -3f;
    public float vectorMax = 3f;

    private List<GameObject> vectorObjects = new List<GameObject>();
    private List<Vector2> vectorSegments = new List<Vector2>();
    private Vector3 origin;

   // public FogTile fogTileScript;
    public AlchemyTile alchemyTileScript;


    public TextMeshProUGUI RegionText;//���݂̐��������N
   public int rarityCode;


    public static AlchemyVectorManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ReceiveItem(ItemStack item)
    {
        Debug.Log($"Received: {item.itemName} x{item.count}");
        // �����ɘB�������p�̎󂯎��o�b�t�@�ǉ��Ȃǂ�����
        GenerateVector(item.AVector);
    }


    void Start()
    {
        origin = new Vector3(16, 16, 0); // ���S�Œ�B���ƂŎ擾���ɂ��Ă�OK
    }

    void Update()
    {

       
        // �A���P�~�[�^�C������A�N�e�B�u�Ȃ�X�L�b�v
        if (!alchemyTileObject.activeSelf) return;

        if (Input.GetKeyDown(generateVectorKey))
        {
          //  GenerateVector();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetVectors();
        }
    }

    void GenerateVector(Vector2 newVec)
    {

        vectorSegments.Add(newVec);

        Vector3 startPos = origin;
        foreach (var vec in vectorSegments.GetRange(0, vectorSegments.Count - 1))
        {
            startPos += new Vector3(vec.x, vec.y, 0f);
        }
        Vector3 endPos = startPos + new Vector3(newVec.x, newVec.y, 0f);

        GameObject vecObj = Instantiate(vectorPrefab, Vector3.zero, Quaternion.identity, transform);
        LineRenderer lr = vecObj.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);

        // ���[���h���W �� �^�C�����W�֕ϊ�
        Vector3Int tilePos = alchemyTileScript.tilemapAlchemy.WorldToCell(endPos);


        
            rarityCode = alchemyTileScript.GetRarityAt(tilePos.x, tilePos.y);
            string rarityName = rarityCode switch
            {
                0 => "�����Ȗ��͔���������...",
                1 => "���͂Ȕ���������!",
                2 => "�q�킶��Ȃ��͂�������...�I",
                _ => "���m"
            };
            Debug.Log($"�x�N�g����[: ({tilePos.x},{tilePos.y}) - �����Ă�  / �F: {rarityName}");
        

        RegionText.text=rarityName.ToString();
        vectorObjects.Add(vecObj);
        lr.startColor = lr.endColor = new Color(Random.value, Random.value, Random.value);

    }

   public void ResetVectors()
    {
        foreach (var obj in vectorObjects)
        {
            Destroy(obj);
        }
        vectorObjects.Clear();
        vectorSegments.Clear();
    }
}
