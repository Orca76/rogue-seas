using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetUIManager : MonoBehaviour
{

    [Header("配置")]
    [SerializeField] RectTransform stackParent;   // ← Canvas内の置き場(親)
    [SerializeField] RectTransform spawnAnchor;   // ← 新規を出す基準位置(Empty)

    [Header("見た目")]
    [SerializeField] GameObject toastPrefab;      // ← ItemGetUIが付いたプレハブ
    [SerializeField] float slotHeight = 48f;      // ← 1段分の高さ（押し上げ距離）
    [SerializeField] int maxActive = 3;           // ← 同時表示上限
    [SerializeField] float pushAnimTime = 0.1f;   // ← 押し上げアニメ時間

    // 下(0) → 上 の順で保持
    readonly List<RectTransform> active = new();

    // 外部から呼ぶ：アイテム取得時に1行でOK
    public void CreateUI(Sprite uiSprite, string uiText)
    {
        CleanupActive(); // 破棄済みを除去

        // 1) 既存を下に押し下げ
        for (int i = 0; i < active.Count; i++)
        {
            var rt = active[i];
            if (!rt) continue;
            StartCoroutine(ShiftY(rt, rt.anchoredPosition.y - slotHeight, pushAnimTime));
        }

        // 2) 新規を生成して基準位置に配置
        var go = Instantiate(toastPrefab, stackParent);
        var rtNew = go.GetComponent<RectTransform>();
        rtNew.anchoredPosition = spawnAnchor.anchoredPosition;

        // 3) 見た目セット
        var toast = go.GetComponent<ItemGetUI>();
        toast.UIImage.sprite = uiSprite;
        toast.UIText.text = uiText;

        // 4) 末尾に追加（上段扱い）
        active.Add(rtNew);

        // 5) 上限超えたら一番下を削除
        if (active.Count > maxActive)
        {
            var bottom = active[0];
            if (bottom) Destroy(bottom.gameObject);
            active.RemoveAt(0);
        }
    }

    // 破棄されてnullになってる要素を掃除
    void CleanupActive()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] == null) active.RemoveAt(i);
        }
    }

    // Y座標だけを短時間でずらす簡易アニメ
    IEnumerator ShiftY(RectTransform rt, float targetY, float duration)
    {
        float t = 0f;
        var pos = rt.anchoredPosition;
        float startY = pos.y;
        while (t < duration && rt != null)
        {
            t += Time.unscaledDeltaTime; // UIなので時間停止の影響を受けにくく
            float y = Mathf.Lerp(startY, targetY, t / duration);
            rt.anchoredPosition = new Vector2(pos.x, y);
            yield return null;
        }
        if (rt != null)
            rt.anchoredPosition = new Vector2(pos.x, targetY);
    }
}
