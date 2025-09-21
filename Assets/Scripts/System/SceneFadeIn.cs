using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] CanvasGroup fadeCg;  // 黒板用キャンバスグループ
    [SerializeField] float duration = 1f; // フェードイン時間（秒）

    void Start()
    {
        if (!fadeCg) fadeCg = GetComponent<CanvasGroup>();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        // 念のため：遷移時に timeScale=0 でも進めるように
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        fadeCg.alpha = 1f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;  // ← deltaTimeじゃなくこれ！
            fadeCg.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        fadeCg.alpha = 0f;
        fadeCg.blocksRaycasts = false; // 入力をブロックしないように解除
    }
}
