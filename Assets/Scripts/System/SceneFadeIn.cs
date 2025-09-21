using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] CanvasGroup fadeCg;  // ���p�L�����o�X�O���[�v
    [SerializeField] float duration = 1f; // �t�F�[�h�C�����ԁi�b�j

    void Start()
    {
        if (!fadeCg) fadeCg = GetComponent<CanvasGroup>();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        // �O�̂��߁F�J�ڎ��� timeScale=0 �ł��i�߂�悤��
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        fadeCg.alpha = 1f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;  // �� deltaTime����Ȃ�����I
            fadeCg.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        fadeCg.alpha = 0f;
        fadeCg.blocksRaycasts = false; // ���͂��u���b�N���Ȃ��悤�ɉ���
    }
}
