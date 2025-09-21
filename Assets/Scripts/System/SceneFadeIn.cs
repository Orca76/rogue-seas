using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup; // ���ɃA�^�b�`����CanvasGroup
    [SerializeField] float duration = 1f;     // �t�F�[�h�C���ɂ����鎞��

    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 1f; // �ŏ��͐^����
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f; // ���S�ɖ��邭
    }
}
