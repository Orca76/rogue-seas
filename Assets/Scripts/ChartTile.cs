using UnityEngine;
using UnityEngine.Tilemaps;

public class ChartTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapChart;

    [Header("Sea Area Tile Types")]
    public TileBase tropicalTile;    // 0 温暖
    public TileBase frigidTile;      // 1 寒冷
    public TileBase foggyTile;       // 2 濃霧
    public TileBase tempestTile;     // 3 暴風
    public TileBase organicTile;     // 4 有機
    public TileBase mysticTile;      // 5 神秘

    [Header("Map Size")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Noise Settings")]
    public float scale = 0.05f;
    public int noiseSeed = 5678;

    [Header("Control Keys")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode hideKey = KeyCode.T;

    [Header("Player Progress (drag Player here)")]
    public Player playerRef;                 // ← あなたのPlayer
    public int maxTurnForCurves = 9;         // 1~3 序盤 / 3~6 中盤 / 6~9 終盤 を 0..1 に正規化

    [Header("Design Curves (0..1 -> 0..1)")]
    [Tooltip("序盤→終盤で 温暖 が減っていく曲線（t=0..1）出力は割合の一部として使う")]
    public AnimationCurve tropicalCurve = AnimationCurve.Linear(0, 0.55f, 1, 0.05f);

    [Tooltip("序盤→終盤で 寒冷 も減る（温暖よりは緩め）")]
    public AnimationCurve frigidCurve = AnimationCurve.Linear(0, 0.30f, 1, 0.05f);

    [Tooltip("有機は中盤以降で一気に増やしたい例（シグモイド気味）")]
    public AnimationCurve organicCurve = new AnimationCurve(
        new Keyframe(0f, 0.02f), new Keyframe(0.5f, 0.12f), new Keyframe(1f, 0.35f)
    );

    [Tooltip("神秘は終盤にかけて増やす")]
    public AnimationCurve mysticCurve = new AnimationCurve(
        new Keyframe(0f, 0.00f), new Keyframe(0.6f, 0.08f), new Keyframe(1f, 0.25f)
    );

    [Tooltip("残余(=1-温暖-寒冷-有機-神秘)のうち 濃霧 に割く割合曲線（0..1）。残りは暴風へ。")]
    public AnimationCurve fogShareCurve = new AnimationCurve(
        new Keyframe(0f, 0.6f), new Keyframe(0.5f, 0.55f), new Keyframe(1f, 0.45f)
    );

    [Header("Safety")]
    public bool UseDynamic = true;   // ← 不具合時にOFFで旧ロジックに戻す
    public bool AutoUpdateOnProgress = true; // 進行変化を自動で反映

    private int[,] seaRegionData; // 0..5
    private float[] thresholds01 = new float[6]; // 累積しきい値（0..1）
    private int lastVisited = -1;

    private void Reset()
    {
        // インスペクタで未設定のときの安全初期値（任意）
        if (tilemapChart == null)
            tilemapChart = GetComponentInChildren<Tilemap>();
    }

    private void Start()
    {
        seaRegionData = new int[mapWidth, mapHeight];

        // プレイヤー未割り当てなら自動取得を試す
        if (playerRef == null) playerRef = FindObjectOfType<Player>();

        // 依存：島のseed
        var island = GetComponent<IslandTile>();
        if (island != null) noiseSeed = island.noiseSeed;

        tilemapChart.gameObject.SetActive(true);

        // 初回セットアップ
        RecomputeThresholds(GetT());
        GenerateChartMap();
        lastVisited = GetVisited();
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            noiseSeed = Random.Range(0, 10000);
            tilemapChart.gameObject.SetActive(true);
            GenerateChartMap();
        }

        if (Input.GetKeyDown(hideKey))
        {
            tilemapChart.gameObject.SetActive(false);
        }

        // 進行の変化を検知して自動で反映
        if (AutoUpdateOnProgress)
        {
            int v = GetVisited();
            if (v != lastVisited)
            {
                lastVisited = v;
                RecomputeThresholds(GetT());
                GenerateChartMap();
            }
        }
    }

    // ==== 進行度の取り方 ====
    private int GetVisited()
    {
        return (playerRef != null) ? playerRef.VisitedIslandCount : 0;
    }

    private float GetT()
    {
        // 1..maxTurnForCurves を 0..1 に正規化（0未満や超過もClamp）
        int v = Mathf.Max(0, GetVisited());
        float denom = Mathf.Max(1, maxTurnForCurves);
        return Mathf.Clamp01(v / denom);
    }

    // ==== 割合→累積しきい値 ====
    private void RecomputeThresholds(float t01)
    {
        if (!UseDynamic)
        {
            // 旧しきい値（固定）。必要ならここをあなたの既存値に合わせてください。
            thresholds01[0] = 0.30f; // 温暖
            thresholds01[1] = 0.50f; // 寒冷
            thresholds01[2] = 0.65f; // 濃霧
            thresholds01[3] = 0.80f; // 暴風
            thresholds01[4] = 0.95f; // 有機
            thresholds01[5] = 1.00f; // 神秘
            return;
        }

        // 各曲線から「素の配分」を取得
        float pWarm = Mathf.Clamp01(tropicalCurve.Evaluate(t01));
        float pFrigid = Mathf.Clamp01(frigidCurve.Evaluate(t01));
        float pOrganic = Mathf.Clamp01(organicCurve.Evaluate(t01));
        float pMystic = Mathf.Clamp01(mysticCurve.Evaluate(t01));

        // 残りを 濃霧/暴風 で分配
        float remain = Mathf.Max(0f, 1f - (pWarm + pFrigid + pOrganic + pMystic));
        float fogShare = Mathf.Clamp01(fogShareCurve.Evaluate(t01));
        float pFog = remain * fogShare;
        float pStorm = remain - pFog;

        // 正規化（誤差対策）
        float sum = pWarm + pFrigid + pFog + pStorm + pOrganic + pMystic;
        if (sum <= 0f) { pWarm = 1f; pFrigid = pFog = pStorm = pOrganic = pMystic = 0f; sum = 1f; }
        pWarm /= sum; pFrigid /= sum; pFog /= sum; pStorm /= sum; pOrganic /= sum; pMystic /= sum;

        // 累積化（0..1）
        thresholds01[0] = pWarm;
        thresholds01[1] = thresholds01[0] + pFrigid;
        thresholds01[2] = thresholds01[1] + pFog;
        thresholds01[3] = thresholds01[2] + pStorm;
        thresholds01[4] = thresholds01[3] + pOrganic;
        thresholds01[5] = 1f; // 神秘
    }

    // ==== 生成 ====
    public void GenerateChartMap()
    {
        if (tilemapChart == null) return;

        tilemapChart.ClearAllTiles();
        Vector2 offset = GetNoiseOffset(noiseSeed);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float nx = (x + offset.x) * scale;
                float ny = (y + offset.y) * scale;
                float v = Mathf.PerlinNoise(nx, ny); // 0..1

                int code;
                TileBase tile = PickTile(v, out code);
                tilemapChart.SetTile(new Vector3Int(x, y, 0), tile);
                seaRegionData[x, y] = code;
            }
        }
    }

    private TileBase PickTile(float v, out int code)
    {
        if (!UseDynamic) return GetTileByNoiseFixed(v, out code);
        return GetTileByNoiseDynamic(v, out code);
    }

    // 旧：固定しきい値
    private TileBase GetTileByNoiseFixed(float val, out int regionCode)
    {
        if (val < 0.3f) { regionCode = 0; return tropicalTile; }
        if (val < 0.5f) { regionCode = 1; return frigidTile; }
        if (val < 0.65f) { regionCode = 2; return foggyTile; }
        if (val < 0.8f) { regionCode = 3; return tempestTile; }
        if (val < 0.95f) { regionCode = 4; return organicTile; }
        regionCode = 5; return mysticTile;
    }

    // 新：動的しきい値
    private TileBase GetTileByNoiseDynamic(float v, out int regionCode)
    {
        if (v < thresholds01[0]) { regionCode = 0; return tropicalTile; }
        if (v < thresholds01[1]) { regionCode = 1; return frigidTile; }
        if (v < thresholds01[2]) { regionCode = 2; return foggyTile; }
        if (v < thresholds01[3]) { regionCode = 3; return tempestTile; }
        if (v < thresholds01[4]) { regionCode = 4; return organicTile; }
        regionCode = 5; return mysticTile;
    }

    // ==== ユーティリティ ====
    private Vector2 GetNoiseOffset(int seed)
    {
        // シンプルな擬似オフセット（必要なら System.Random に変更）
        float ox = (seed * 12345 % 1000) / 1000f * 100f;
        float oy = (seed * 67890 % 1000) / 1000f * 100f;
        return new Vector2(ox, oy);
    }

    public int GetZoneTypeAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return -1;
        return seaRegionData[x, y];
    }

    public int GetRegionCodeAt(int x, int y) => GetZoneTypeAt(x, y);

    // 手動更新用（デバッグ/ボタンから呼べる）
    public void ForceRebuildFromPlayer()
    {
        RecomputeThresholds(GetT());
        GenerateChartMap();
        lastVisited = GetVisited();
    }
}
