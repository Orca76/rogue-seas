using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// �j��^�ݒu�C�x���g�Ŕ��a fadeMax ���������e�[�u�����X�V���A
/// ���̃Z���ɑ��݂����v���n�u�� SpriteRenderer.color �𓯊�����}�l�[�W���B
/// Tilemap �̒��_�J���[�ɂ͈�؈ˑ����Ȃ��B
/// </summary>
public class RockDepthManager : MonoBehaviour
{
    /* ---------- Inspector ---------- */
    [Header("Core refs")]
    [SerializeField] Tilemap rockMap;              // �₪�~����Ă��� Tilemap
    [SerializeField] TileObjectSpawner spawner;    // ��v���n�u�������������V�X�e��
    [SerializeField] TileBase[] rockTiles;         // ��Ɣ��肷�� TileBase ��S��

    [Header("Params")]
    [SerializeField] int fadeMax = 6;   // ���^�C�����Ő^����
    [SerializeField] int rebakeRadius = 8;   // �Ǐ��Čv�Z���a

    /* ---------- Runtime ---------- */
    public static RockDepthManager Instance { get; private set; }

    // �����e�[�u���i-1 = ��ȊO�j
    private int[,] depth;
    private readonly Queue<Vector2Int> pending = new(); // �Čv�Z�L���[
    private readonly HashSet<Vector2Int> seen = new(); // ���t���[���d������

    private BoundsInt bounds;
    private int ox, oy, w, h;

    /* ---------- Unity ---------- */
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // �̈�L���b�V��
        bounds = rockMap.cellBounds;
        ox = bounds.xMin; oy = bounds.yMin;
        w = bounds.size.x; h = bounds.size.y;

        depth = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                depth[x, y] = -1;

        BakeWholeMap();            // �N������ 1 �񋗗����Ă�
    }

    void LateUpdate()
    {
        if (pending.Count == 0) return;

        while (pending.Count > 0)
            RebakeLocal(pending.Dequeue());

        seen.Clear();
    }

    /* ---------- Public API ---------- */
    public void RequestRebake(Vector2Int tilePos)
    {
        if (seen.Add(tilePos))
            pending.Enqueue(tilePos);
    }

    /// Spawn ���E�Ċ��������ɌĂяo���A���݂̋����ɉ������F��Ԃ�
    public Color GetColor(Vector3Int cell)
    {
        int x = cell.x - ox, y = cell.y - oy;
        int d = (x < 0 || x >= w || y < 0 || y >= h) ? fadeMax : depth[x, y];
        if (d < 0) d = fadeMax;
        float t = Mathf.Clamp01(d / (float)fadeMax);
        return Color.Lerp(Color.white, Color.black, t);
    }

    /* ---------- Core ---------- */
    static readonly Vector3Int[] DIR4 = {
        new( 1, 0, 0), new(-1, 0, 0),
        new( 0, 1, 0), new( 0,-1, 0)
    };

   public void BakeWholeMap()
    {
        Queue<Vector3Int> q = new();

        // �O����
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var cell = new Vector3Int(x + ox, y + oy, 0);
                if (!IsRock(cell)) continue;

                foreach (var d in DIR4)
                {
                    if (!IsRock(cell + d))                         // �ׂ���C
                    {
                        depth[x, y] = 0;
                        q.Enqueue(cell);
                        break;
                    }
                }
            }

        // BFS
        while (q.Count > 0)
        {
            var c = q.Dequeue();
            int cx = c.x - ox, cy = c.y - oy;
            foreach (var d in DIR4)
            {
                var n = c + d;
                int nx = n.x - ox, ny = n.y - oy;
                if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                if (depth[nx, ny] != -1) continue;
                if (!IsRock(n)) continue;

                depth[nx, ny] = Mathf.Min(depth[cx, cy] + 1, fadeMax);
                q.Enqueue(n);
            }
        }
    }

    void RebakeLocal(Vector2Int center)
    {
        int minX = Mathf.Max(center.x - rebakeRadius, bounds.xMin);
        int maxX = Mathf.Min(center.x + rebakeRadius, bounds.xMax - 1);
        int minY = Mathf.Max(center.y - rebakeRadius, bounds.yMin);
        int maxY = Mathf.Min(center.y + rebakeRadius, bounds.yMax - 1);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!IsRock(cell)) continue;

                // �����}���n�b�^�������T��
                int dist;
                bool edge = false;
                for (dist = 1; dist <= fadeMax; dist++)
                {
                    if (!IsRock(cell + DIR4[0] * dist) ||
                        !IsRock(cell + DIR4[1] * dist) ||
                        !IsRock(cell + DIR4[2] * dist) ||
                        !IsRock(cell + DIR4[3] * dist))
                    { edge = true; break; }
                }
                if (!edge) dist = fadeMax;

                depth[x - ox, y - oy] = dist;

                // �v���n�u�X�V
                if (spawner.TryGetSpawned(cell, out GameObject go))
                {
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        float t = Mathf.Clamp01(dist / (float)fadeMax);
                        sr.color = Color.Lerp(Color.white, Color.black, t);
                    }
                }
            }
    }

    bool IsRock(Vector3Int cell)
    {
        var t = rockMap.GetTile(cell);
        if (t == null) return false;
        foreach (var rt in rockTiles)
            if (rt == t) return true;
        return false;
    }
    public void RebuildAndBake()
    {
        // �@ �ŐV�� cellBounds ����蒼��
        bounds = rockMap.cellBounds;
        ox = bounds.xMin; oy = bounds.yMin;
        w = bounds.size.x; h = bounds.size.y;

        // �A depth[,] ����蒼��
        depth = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                depth[x, y] = -1;

        // �B �S�̂���x�����Ă�
        BakeWholeMap();
    }
}
