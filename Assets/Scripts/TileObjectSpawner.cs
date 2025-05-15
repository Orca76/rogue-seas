using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileObjectSpawner : MonoBehaviour
{
    public IslandTile tileController; // ��tileMapData�������Ă���
    public GameObject tileMapObj;     // �^�C���}�b�v�I�u�W�F�N�g

  //  public IslandTile tileControllerUnderground; // �n����tileMapData�������Ă���
  //  public GameObject tileMapObjUnderground;     // �n���^�C���}�b�v�I�u�W�F�N�g

    public Transform player;
    Player playerSc;
    public GameObject rockPrefab;        // �n��̊�v���n�u
    public GameObject deepstonePrefab;   // �n���̐[�w��v���n�u
    public GameObject bedRockPrefab;//���

    public int viewRange = 10;           // �v���C���[���͂̐����͈�
    float tileSize = 0.16f;

    Dictionary<Vector2Int, GameObject> spawnedObjectsSurface = new();    // �n��p�����I�u�W�F�N�g����
    Dictionary<Vector2Int, GameObject> spawnedObjectsUnderground = new(); // �n���p�����I�u�W�F�N�g����

    private Vector3 lastPlayerPos;

    [SerializeField] private ObjectWithScript<IslandTile> tile;

   
    private void Start()
    {
        tileController = tileMapObj.GetComponent<IslandTile>();
    //    tileControllerUnderground = tileMapObjUnderground.GetComponent<IslandTile>();
        lastPlayerPos = player.position;
        playerSc=player.GetComponent<Player>();

    }

    void Update()
    {
        if (!Mathf.Approximately((player.position - lastPlayerPos).sqrMagnitude, 0f) || Input.GetKeyDown(KeyCode.U))
        {
            lastPlayerPos = player.position;
            RefreshAroundPlayer();
        }
    }

    void RefreshAroundPlayer()
    {
        Vector2 playerPos = player.position;
        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / tileSize),
            Mathf.FloorToInt(playerPos.y / tileSize)
        );

        bool isUnderground = Mathf.Approximately(player.position.z, 1f);

        // �n��ƒn���𖾊m�ɕ���
        if (isUnderground)
        {
            RefreshLayer(spawnedObjectsUnderground, playerTile);
        }
        else
        {
           // Debug.Log(1);
            RefreshLayer(spawnedObjectsSurface, playerTile);
        }
    }

    // �n�w���Ƃ̋��ʏ���
    void RefreshLayer( Dictionary<Vector2Int, GameObject> objDict, Vector2Int playerTile)
    {
       
        for (int x = playerTile.x - viewRange; x <= playerTile.x + viewRange; x++)
        {
            for (int y = playerTile.y - viewRange; y <= playerTile.y + viewRange; y++)
            {



                if (!IsInBounds(tileController, x, y)) continue;

                Vector2Int pos = new Vector2Int(x, y);
                int id = (playerSc.isUnderground == false ? tileController.tileMapData : tileController.tileMapDataUnderground)[x, y];//���͂̃^�C���̃f�[�^���擾
              //  Debug.Log(id);

                switch (id)
                {
                   case 31:
                        //�n���^�C��
                        if (!objDict.ContainsKey(pos) || objDict[pos] == null)
                        {
                            Vector3 worldPos = new Vector3(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f, 0);
                            GameObject obj = Instantiate(rockPrefab, worldPos, Quaternion.identity);
                            // obj.transform.parent = tile.obj.transform;
                            obj.transform.parent = tileController.islandRoot.transform;
                            objDict[pos] = obj;

                            RockBlock rb = obj.GetComponent<RockBlock>();
                            rb.tilePos = pos;
                            rb.islandTiles = tileController;
                        }
                        else
                        {
                            objDict[pos].SetActive(true);
                        }
                        break;

                    case 50:
                        //�n���[�w��^�C��
                        if (!objDict.ContainsKey(pos) || objDict[pos] == null)
                        {
                            Vector3 worldPos = new Vector3(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f,1);
                            GameObject obj = Instantiate(deepstonePrefab, worldPos, Quaternion.identity);
                            obj.transform.parent = tileController.islandRoot.transform;

                            objDict[pos] = obj;

                            DeepstoneBlock rb = obj.GetComponent<DeepstoneBlock>();
                            rb.tilePos = pos;
                            rb.islandTiles = tileController;
                        }
                        else
                        {
                            objDict[pos].SetActive(true);
                        }
                        break;

                    case 60:
                        //�C��n�����
                        if (!objDict.ContainsKey(pos) || objDict[pos] == null)
                        {
                            Vector3 worldPos = new Vector3(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f, 1);
                            GameObject obj = Instantiate(bedRockPrefab, worldPos, Quaternion.identity);
                            obj.transform.parent = tileController.islandRoot.transform;
                            objDict[pos] = obj;

                            BedRock rb = obj.GetComponent<BedRock>();
                            rb.tilePos = pos;
                            rb.islandTiles = tileController;
                        }
                        else
                        {
                            objDict[pos].SetActive(true);
                        }
                        break;

                }

            }
        }

        // �͈͊O�͍폜
        List<Vector2Int> toRemove = new();
        foreach (var kvp in objDict)
        {
            if (Vector2Int.Distance(kvp.Key, playerTile) > viewRange)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            objDict.Remove(key);
    }

    bool IsInBounds(IslandTile tileCtrl, int x, int y)
    {
        if (tileCtrl == null || tileCtrl.tileMapData == null)
        {
            Debug.Log(tileCtrl.tileMapData);
            return false;
        }

        return x >= 0 && y >= 0 &&
               x < tileCtrl.tileMapData.GetLength(0) &&
               y < tileCtrl.tileMapData.GetLength(1);
    }
}
