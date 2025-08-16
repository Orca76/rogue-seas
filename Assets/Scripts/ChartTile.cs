using UnityEngine;
using UnityEngine.Tilemaps;

public class ChartTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapChart;

    [Header("Sea Area Tile Types")]
    public TileBase tropicalTile;    // 0 ���g
    public TileBase frigidTile;      // 1 ����
    public TileBase foggyTile;       // 2 �Z��
    public TileBase tempestTile;     // 3 �\��
    public TileBase organicTile;     // 4 �L�@
    public TileBase mysticTile;      // 5 �_��

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
    public Player playerRef;                 // �� ���Ȃ���Player
    public int maxTurnForCurves = 9;         // 1~3 ���� / 3~6 ���� / 6~9 �I�� �� 0..1 �ɐ��K��

    [Header("Design Curves (0..1 -> 0..1)")]
    [Tooltip("���Ձ��I�Ղ� ���g �������Ă����Ȑ��it=0..1�j�o�͂͊����̈ꕔ�Ƃ��Ďg��")]
    public AnimationCurve tropicalCurve = AnimationCurve.Linear(0, 0.55f, 1, 0.05f);

    [Tooltip("���Ձ��I�Ղ� ���� ������i���g���͊ɂ߁j")]
    public AnimationCurve frigidCurve = AnimationCurve.Linear(0, 0.30f, 1, 0.05f);

    [Tooltip("�L�@�͒��Ոȍ~�ň�C�ɑ��₵������i�V�O���C�h�C���j")]
    public AnimationCurve organicCurve = new AnimationCurve(
        new Keyframe(0f, 0.02f), new Keyframe(0.5f, 0.12f), new Keyframe(1f, 0.35f)
    );

    [Tooltip("�_��͏I�Ղɂ����đ��₷")]
    public AnimationCurve mysticCurve = new AnimationCurve(
        new Keyframe(0f, 0.00f), new Keyframe(0.6f, 0.08f), new Keyframe(1f, 0.25f)
    );

    [Tooltip("�c�](=1-���g-����-�L�@-�_��)�̂��� �Z�� �Ɋ��������Ȑ��i0..1�j�B�c��͖\���ցB")]
    public AnimationCurve fogShareCurve = new AnimationCurve(
        new Keyframe(0f, 0.6f), new Keyframe(0.5f, 0.55f), new Keyframe(1f, 0.45f)
    );

    [Header("Safety")]
    public bool UseDynamic = true;   // �� �s�����OFF�ŋ����W�b�N�ɖ߂�
    public bool AutoUpdateOnProgress = true; // �i�s�ω��������Ŕ��f

    private int[,] seaRegionData; // 0..5
    private float[] thresholds01 = new float[6]; // �ݐς������l�i0..1�j
    private int lastVisited = -1;

    private void Reset()
    {
        // �C���X�y�N�^�Ŗ��ݒ�̂Ƃ��̈��S�����l�i�C�Ӂj
        if (tilemapChart == null)
            tilemapChart = GetComponentInChildren<Tilemap>();
    }

    private void Start()
    {
        seaRegionData = new int[mapWidth, mapHeight];

        // �v���C���[�����蓖�ĂȂ玩���擾������
        if (playerRef == null) playerRef = FindObjectOfType<Player>();

        // �ˑ��F����seed
        var island = GetComponent<IslandTile>();
        if (island != null) noiseSeed = island.noiseSeed;

        tilemapChart.gameObject.SetActive(true);

        // ����Z�b�g�A�b�v
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

        // �i�s�̕ω������m���Ď����Ŕ��f
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

    // ==== �i�s�x�̎��� ====
    private int GetVisited()
    {
        return (playerRef != null) ? playerRef.VisitedIslandCount : 0;
    }

    private float GetT()
    {
        // 1..maxTurnForCurves �� 0..1 �ɐ��K���i0�����⒴�߂�Clamp�j
        int v = Mathf.Max(0, GetVisited());
        float denom = Mathf.Max(1, maxTurnForCurves);
        return Mathf.Clamp01(v / denom);
    }

    // ==== �������ݐς������l ====
    private void RecomputeThresholds(float t01)
    {
        if (!UseDynamic)
        {
            // ���������l�i�Œ�j�B�K�v�Ȃ炱�������Ȃ��̊����l�ɍ��킹�Ă��������B
            thresholds01[0] = 0.30f; // ���g
            thresholds01[1] = 0.50f; // ����
            thresholds01[2] = 0.65f; // �Z��
            thresholds01[3] = 0.80f; // �\��
            thresholds01[4] = 0.95f; // �L�@
            thresholds01[5] = 1.00f; // �_��
            return;
        }

        // �e�Ȑ�����u�f�̔z���v���擾
        float pWarm = Mathf.Clamp01(tropicalCurve.Evaluate(t01));
        float pFrigid = Mathf.Clamp01(frigidCurve.Evaluate(t01));
        float pOrganic = Mathf.Clamp01(organicCurve.Evaluate(t01));
        float pMystic = Mathf.Clamp01(mysticCurve.Evaluate(t01));

        // �c��� �Z��/�\�� �ŕ��z
        float remain = Mathf.Max(0f, 1f - (pWarm + pFrigid + pOrganic + pMystic));
        float fogShare = Mathf.Clamp01(fogShareCurve.Evaluate(t01));
        float pFog = remain * fogShare;
        float pStorm = remain - pFog;

        // ���K���i�덷�΍�j
        float sum = pWarm + pFrigid + pFog + pStorm + pOrganic + pMystic;
        if (sum <= 0f) { pWarm = 1f; pFrigid = pFog = pStorm = pOrganic = pMystic = 0f; sum = 1f; }
        pWarm /= sum; pFrigid /= sum; pFog /= sum; pStorm /= sum; pOrganic /= sum; pMystic /= sum;

        // �ݐω��i0..1�j
        thresholds01[0] = pWarm;
        thresholds01[1] = thresholds01[0] + pFrigid;
        thresholds01[2] = thresholds01[1] + pFog;
        thresholds01[3] = thresholds01[2] + pStorm;
        thresholds01[4] = thresholds01[3] + pOrganic;
        thresholds01[5] = 1f; // �_��
    }

    // ==== ���� ====
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

    // ���F�Œ肵�����l
    private TileBase GetTileByNoiseFixed(float val, out int regionCode)
    {
        if (val < 0.3f) { regionCode = 0; return tropicalTile; }
        if (val < 0.5f) { regionCode = 1; return frigidTile; }
        if (val < 0.65f) { regionCode = 2; return foggyTile; }
        if (val < 0.8f) { regionCode = 3; return tempestTile; }
        if (val < 0.95f) { regionCode = 4; return organicTile; }
        regionCode = 5; return mysticTile;
    }

    // �V�F���I�������l
    private TileBase GetTileByNoiseDynamic(float v, out int regionCode)
    {
        if (v < thresholds01[0]) { regionCode = 0; return tropicalTile; }
        if (v < thresholds01[1]) { regionCode = 1; return frigidTile; }
        if (v < thresholds01[2]) { regionCode = 2; return foggyTile; }
        if (v < thresholds01[3]) { regionCode = 3; return tempestTile; }
        if (v < thresholds01[4]) { regionCode = 4; return organicTile; }
        regionCode = 5; return mysticTile;
    }

    // ==== ���[�e�B���e�B ====
    private Vector2 GetNoiseOffset(int seed)
    {
        // �V���v���ȋ[���I�t�Z�b�g�i�K�v�Ȃ� System.Random �ɕύX�j
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

    // �蓮�X�V�p�i�f�o�b�O/�{�^������Ăׂ�j
    public void ForceRebuildFromPlayer()
    {
        RecomputeThresholds(GetT());
        GenerateChartMap();
        lastVisited = GetVisited();
    }
}
