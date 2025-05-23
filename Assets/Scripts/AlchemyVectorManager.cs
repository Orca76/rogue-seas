using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AlchemyVectorManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameObject alchemyTileObject; // Alchemyタイル本体
    public KeyCode generateVectorKey = KeyCode.I;
    public KeyCode resetKey = KeyCode.T;

    [Header("Vector Settings")]
    public GameObject vectorPrefab; // LineRendererを持つプレハブ
    public float vectorMin = -3f;
    public float vectorMax = 3f;

    private List<GameObject> vectorObjects = new List<GameObject>();
    private List<Vector2> vectorSegments = new List<Vector2>();
    private Vector3 origin;

    public FogTile fogTileScript;
    public AlchemyTile alchemyTileScript;
    void Start()
    {
        origin = new Vector3(16, 16, 0); // 中心固定。あとで取得式にしてもOK
    }

    void Update()
    {
        // アルケミータイルが非アクティブならスキップ
        if (!alchemyTileObject.activeSelf) return;

        if (Input.GetKeyDown(generateVectorKey))
        {
            GenerateVector();
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetVectors();
        }
    }

    void GenerateVector()
    {
        Vector2 newVec = new Vector2(
            Random.Range(vectorMin, vectorMax),
            Random.Range(vectorMin, vectorMax)
        );
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

        // ワールド座標 → タイル座標へ変換
        Vector3Int tilePos = alchemyTileScript.tilemapAlchemy.WorldToCell(endPos);
        // Fogに問い合わせ
        bool isVisible = fogTileScript.IsRevealed(tilePos.x, tilePos.y);

        if (isVisible)
        {
            int rarityCode = alchemyTileScript.GetRarityAt(tilePos.x, tilePos.y);
            string rarityName = rarityCode switch
            {
                0 => "青 (コモン)",
                1 => "紫 (レア)",
                2 => "黄 (スーパーレア)",
                _ => "未知"
            };
            Debug.Log($"ベクトル先端: ({tilePos.x},{tilePos.y}) - 見えてる  / 色: {rarityName}");
        }
        else
        {
            Debug.Log($"ベクトル先端: ({tilePos.x},{tilePos.y}) - 覆われてる（Fog）× ");
        }

        vectorObjects.Add(vecObj);
        lr.startColor = lr.endColor = new Color(Random.value, Random.value, Random.value);

    }

    void ResetVectors()
    {
        foreach (var obj in vectorObjects)
        {
            Destroy(obj);
        }
        vectorObjects.Clear();
        vectorSegments.Clear();
    }
}
