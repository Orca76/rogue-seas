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

    private int revealLevel = 0; // ���i�K�ڂ̖��@�΂��������ꂽ���i���S����̉���͈́j
    private Vector2Int center;

    private void Start()
    {
        center = new Vector2Int(mapWidth / 2, mapHeight / 2);
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
            }
        }
    }

    void ExpandReveal()
    {
        revealLevel++;

        // ���񓯂��u�ʐρv��ǉ��ŉ�������� �� radius^2 �����Ԋu�ő���
        float baseArea = Mathf.PI * Mathf.Pow(6, 2); // �ŏ��̔��a6�ŌŒ�
        float targetArea = baseArea * revealLevel;
        float newRadius = Mathf.Sqrt(targetArea / Mathf.PI);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                float dist = Vector2Int.Distance(pos, center);

                if (dist <= newRadius)
                {
                    tilemapFog.SetTile(new Vector3Int(x, y, 0), null); // Fog����
                }
            }
        }
    }
}
