using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChartVectorManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameClock gameClock; // 時間から角度を取得
    public GameObject chartTileObject; // 海図の親オブジェクト
    public GameObject vectorPrefab; // LineRendererを持つプレハブ
    public KeyCode generateVectorKey = KeyCode.O;
    public KeyCode resetKey = KeyCode.T;

    [Header("Vector Settings")]
    public float segmentLength = 2f; // 1魔法石分のベクトル長
    public Vector3 origin = new Vector3(16, 16, 0); // 中心点

    private List<GameObject> vectorObjects = new();
    private List<Vector2> vectorSegments = new();

    public ChartTile chartTileScript;

    public TextMeshProUGUI RegionText;//現在の目標海域

    public static ChartVectorManager Instance;//やり取り用インスタンス

   public int zoneType;//目的エリア情報

    private void Awake()
    {
        Instance = this;
    }

    public void ReceiveItem(ItemStack item)
    {
        Debug.Log($"Received: {item.itemName} x{item.count}");
        // ここに錬成処理用の受け取りバッファ追加などを実装
        GenerateVector(item.AVector);
    }
    void Update()
    {
        if (!chartTileObject.activeSelf) return;

        if (Input.GetKeyDown(generateVectorKey))
        {
           // GenerateVector();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetVectors();
        }
    }

    void GenerateVector(Vector2 newVec)
    {
      //  float angle = gameClock.GetAngleForCurrentTime(); // 0?360度
       // float rad = angle * Mathf.Deg2Rad;
      //  Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
      //  Vector2 segment = direction * segmentLength;

        vectorSegments.Add(newVec);

        //Vector3 start = origin;
        //for (int i = 0; i < vectorSegments.Count - 1; i++)
        //{
        //    start += (Vector3)vectorSegments[i];
        //}
        //Vector3 end = start + (Vector3)segment;

        //GameObject vecObj = Instantiate(vectorPrefab, Vector3.zero, Quaternion.identity, transform);
        //LineRenderer lr = vecObj.GetComponent<LineRenderer>();
        //lr.positionCount = 2;
        //lr.useWorldSpace = true;
        //lr.SetPosition(0, start);
        //lr.SetPosition(1, end);
        //lr.startColor = lr.endColor = new Color(Random.value, Random.value, Random.value);

        //vectorObjects.Add(vecObj);
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

        Vector3Int tilePos = chartTileScript.tilemapChart.WorldToCell(endPos); // タイル座標へ変換
         zoneType = chartTileScript.GetZoneTypeAt(tilePos.x, tilePos.y);
        string zoneName = zoneType switch
        {
            0 => "温暖海域",
            1 => "寒冷海域",
            2 => "濃霧海域",
            3 => "暴風海域",
            4 => "有機海域",
            5 => "神秘海域",
            _ => "未知"
        };

        Debug.Log($"出航ベクトル先端: ({tilePos.x}, {tilePos.y}) - 海域: {zoneName}");
        RegionText.text = zoneName.ToString();
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
