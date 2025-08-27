using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Transform))]
public class SentryNexus : MonoBehaviour
{
    public List<GameObject> targets;      // セントリーのリスト（リアルタイム更新される想定）
    public Material lineMaterial;        // 線のマテリアル（発光やブラーはマテリアル側で）
    public float width = 0.05f;          // 線の太さ
    public int segments = 12;            // アーチの分割数
    public float arcHeight = 0.15f;      // アーチの高さ（距離×この値）

    private List<LineRenderer> lines = new List<LineRenderer>();

    private void Start()
    {
        targets = gameObject.GetComponent<SentryManager>().sentryList;
    }
    void Update()
    {
        // まず全削除
        foreach (var lr in lines)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        lines.Clear();

        // targets に合わせて作り直す
        foreach (var t in targets)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            var go = new GameObject("SentryLine");
            go.transform.SetParent(transform);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.widthMultiplier = width;
            lines.Add(lr);

            // 位置と色を更新
            UpdateLine(lr, t);
        }
    }

    void UpdateLine(LineRenderer lr, GameObject target)
    {
        // アーチ描画
        Vector3 p0 = transform.position;
        Vector3 p2 = target.transform.position;
        Vector3 mid = (p0 + p2) * 0.5f;
        float h = Vector3.Distance(p0, p2) * arcHeight;
        Vector3 p1 = mid + Vector3.up * h;

        lr.positionCount = segments + 1;
        for (int j = 0; j <= segments; j++)
        {
            float t = j / (float)segments;
            Vector3 pos = (1 - t) * (1 - t) * p0
                        + 2 * (1 - t) * t * p1
                        + t * t * p2;
            lr.SetPosition(j, pos);
        }

        // HPによる色
        var sb = target.GetComponent<SentryBase>();
        float hpRatio = sb ? sb.HP / sb.MaxHP : 1f;

        Color c;
        if (hpRatio > 0.5f)
            c = Color.Lerp(Color.yellow, new Color(0.6f, 1f, 0.4f), (hpRatio - 0.5f) / 0.5f);
        else
            c = Color.Lerp(Color.red, Color.yellow, hpRatio / 0.5f);

        lr.startColor = lr.endColor = c;
        if (lr.material.HasProperty("_Color"))
            lr.material.color = c;
    }
}
