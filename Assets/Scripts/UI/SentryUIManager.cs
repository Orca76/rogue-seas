using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SentryUIManager : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] RectTransform listParent;   // ← RectTransform 推奨
    [SerializeField] SentryUI entryPrefab;

    [Header("レイアウト")]
    [SerializeField] float rowHeight = 28f;      // 行の高さ（プレハブに合わせて調整）
    [SerializeField] float rowSpacing = 6f;      // 行間


    [SerializeField] Color[] rarityColors;

    private SentryManager sentryMgr;

    private class Row { public SentryBase sb; public SentryUI ui; public SpriteRenderer sr; }
    private readonly Dictionary<GameObject, Row> map = new();

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) { Debug.LogError("Player(tag=Player)が見つからない"); return; }
        sentryMgr = player.GetComponent<SentryManager>();
        if (!sentryMgr) Debug.LogError("PlayerにSentryManagerが付いていない");
    }

    void Start() => Rebuild();

    void Update()
    {
        if (!sentryMgr) return;

        bool changed = (sentryMgr.sentryList.Count != map.Count);
        if (changed) Rebuild();
        else CleanupDestroyed();

        // 値更新
        foreach (var go in sentryMgr.sentryList)
        {
            if (!go) continue;
            if (!map.TryGetValue(go, out var row) || row.sb == null || row.ui == null) continue;

            float cur = Mathf.Max(0, row.sb.HP);
            float max = Mathf.Max(1, row.sb.MaxHP);
            if (row.ui.SentryHPSlider) row.ui.SentryHPSlider.value = cur / max;
            if (row.ui.SentryHPText) row.ui.SentryHPText.text = $"{(int)cur}/{(int)max}";

            // 初回だけアイコン適用（変わらない想定）
            if (row.ui.Sentryicon && row.sr && row.ui.Sentryicon.sprite == null)
                row.ui.Sentryicon.sprite = row.sr.sprite;
        }
    }

    void Rebuild()
    {
        // いなくなった行を削除
        var keys = new List<GameObject>(map.Keys);
        foreach (var k in keys)
        {
            if (k == null || !sentryMgr.sentryList.Contains(k))
            {
                if (map[k].ui) Destroy(map[k].ui.gameObject);
                map.Remove(k);
            }
        }

        // 新規追加
        foreach (var go in sentryMgr.sentryList)
        {
            if (!go || map.ContainsKey(go)) continue;

            var sb = go.GetComponent<SentryBase>();
            if (!sb) continue;

            var ui = Instantiate(entryPrefab, listParent);
            ui.GetComponent<Image>().color = rarityColors[sb.Rarity];

            // アンカー/ピボットを左上に固定（ズレ防止）
            var rt = ui.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            if (ui.SentryHPSlider) { ui.SentryHPSlider.minValue = 0f; ui.SentryHPSlider.maxValue = 1f; }

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (ui.Sentryicon && sr) ui.Sentryicon.sprite = sr.sprite;

            map.Add(go, new Row { sb = sb, ui = ui, sr = sr });
        }

        Relayout(); // ここで縦に並べる
    }

    void CleanupDestroyed()
    {
        var keys = new List<GameObject>(map.Keys);
        bool removed = false;
        foreach (var k in keys)
        {
            if (k == null)
            {
                if (map[k].ui) Destroy(map[k].ui.gameObject);
                map.Remove(k);
                removed = true;
            }
        }
        if (removed) Relayout();
    }

    void Relayout()
    {
        // 先頭行（表示できる最初のやつ）を探す
        RectTransform firstRT = null;
        GameObject firstGO = null;

        foreach (var go in sentryMgr.sentryList)
        {
            if (!go) continue;
            if (map.TryGetValue(go, out var row) && row.ui != null)
            {
                firstRT = row.ui.GetComponent<RectTransform>();
                firstGO = go;
                break;
            }
        }
        if (firstRT == null) return; // 何も無ければ終了

        // 基準座標（先頭行の現在位置）を取得
        Vector2 basePos = firstRT.anchoredPosition;
        float baseX = basePos.x;
        float baseY = basePos.y;

        float yAccum = 0f; // 先頭の直下から積み上げ

        foreach (var go in sentryMgr.sentryList)
        {
            if (!go) continue;
            if (!map.TryGetValue(go, out var row) || row.ui == null) continue;

            var rt = row.ui.GetComponent<RectTransform>();

            if (go == firstGO)
            {
                // 先頭は触らない（今の位置を尊重）
                // 次の行の開始オフセット用に先頭の高さを積む
                yAccum += GetRowHeight(rt) + rowSpacing;
            }
            else
            {
                // 先頭の下に順に並べる（上起点なので -yAccum）
                rt.anchoredPosition = new Vector2(baseX, baseY - yAccum);
                yAccum += GetRowHeight(rt) + rowSpacing;
            }
        }

        // （任意）スクロール用に親高さ更新したいならここで
        // listParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yAccum);
    }

    // 行の高さ取得（LayoutElement優先→RectTransform）
    float GetRowHeight(RectTransform rt)
    {
        var le = rt.GetComponent<UnityEngine.UI.LayoutElement>();
        if (le && le.minHeight > 0f) return le.preferredHeight > 0 ? le.preferredHeight : le.minHeight;
        var h = rt.rect.height;
        return h > 0 ? h : rowHeight; // rectが0の時の保険でrowHeight使用
    }
}
