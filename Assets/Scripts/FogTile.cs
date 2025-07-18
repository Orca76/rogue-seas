using UnityEngine;
using UnityEngine.Tilemaps;

public class FogTile : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap tilemapFog;
    public TileBase fogTile; // �D�F�ŕ����^�C��

    [Header("Map Size")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Control Keys")]
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode hideKey = KeyCode.T;
    public KeyCode expandKey = KeyCode.Y;

    [Header("Reveal Settings")]
    public int chunkSize = 5; // �𑜓x�������đe������P�ʃT�C�Y
    public float maxRadius = 30f; // �ŏI�I�ɓ��B�������ő唼�a
    public int maxReveals = 10;   // ���i�K�ł����܂œ��B���邩�i�Q�ߓI�Ɂj

    private int revealLevel = 0; // ���i�K�ڂ̖��@�΂��������ꂽ���i���S����̉���͈́j
    private Vector2Int center;

    public bool[,] fogRevealed; // true = �����Ă�, false = �����Ă���

    private void Start()
    {
        center = new Vector2Int(mapWidth / 2, mapHeight / 2);
        fogRevealed = new bool[mapWidth, mapHeight];
        ReloadFog();
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            tilemapFog.gameObject.SetActive(true);
            ReloadFog();
        }

        if (Input.GetKeyDown(hideKey))
        {
            tilemapFog.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(expandKey))
        {
            ExpandReveal();
        }
    }

    public void ReloadFog()
    {
        tilemapFog.ClearAllTiles();
        revealLevel = 0;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tilemapFog.SetTile(new Vector3Int(x, y, 0), fogTile);
                fogRevealed[x, y] = false;
            }
        }
    }

    void ExpandReveal()
    {
        revealLevel++;

        float normalized = Mathf.Clamp01((float)revealLevel / maxReveals);
        float newRadius = maxRadius * Mathf.Pow(normalized, 0.5f);

        for (int cy = 0; cy < mapHeight; cy += chunkSize)
        {
            for (int cx = 0; cx < mapWidth; cx += chunkSize)
            {
                Vector2Int chunkCenter = new Vector2Int(cx + chunkSize / 2, cy + chunkSize / 2);
                float dist = Vector2Int.Distance(chunkCenter, center);

                if (dist <= newRadius)
                {
                    for (int dy = 0; dy < chunkSize; dy++)
                    {
                        for (int dx = 0; dx < chunkSize; dx++)
                        {
                            int tx = cx + dx;
                            int ty = cy + dy;
                            if (tx < mapWidth && ty < mapHeight)
                            {
                                tilemapFog.SetTile(new Vector3Int(tx, ty, 0), null);
                                fogRevealed[tx, ty] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    //public bool IsRevealed(int x, int y)
    //{
    //    if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return false;
    //    return fogRevealed[x, y];
    //}
}
