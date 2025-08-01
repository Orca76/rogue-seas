using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpSelectionUI : MonoBehaviour
{
    public GameObject playerObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //public void OnHealthBtn( int x)
    //{
    //    if(playerObj != null)
    //    {
    //        playerObj.GetComponent<Player>().levels[0]++;//HP�̃��x�����グ��
    //    }
    //}
    //public void OnPowerBtn()
    //{
    //    if (playerObj != null)
    //    {
    //        playerObj.GetComponent<Player>().levels[1]++;//AT�̃��x�����グ��
    //    }
    //}
    //public void OnAttackSpeedBtn()
    //{
    //    if (playerObj != null)
    //    {
    //        playerObj.GetComponent<Player>().levels[2]++;//�U�����x�̃��x�����グ��
    //    }
    //}

    public void OnUpgradeButton(int index)
    {
        if (playerObj != null)
        {
            playerObj.GetComponent<Player>().levels[index]++;
        }
    }
    public void OnHealSentries()//�S�̉�
    {
        var sentryManager = playerObj.GetComponent<SentryManager>();
        if (sentryManager == null) return;

        foreach (var sentry in sentryManager.sentryList)
        {
            if (sentry == null) continue;

            var sb = sentry.GetComponent<SentryBase>();
            if (sb == null) continue;

            // 5���񕜁i�ő�HP��50%���񕜁j
            sb.HP = Mathf.Min(sb.HP + sb.MaxHP * 0.5f, sb.MaxHP);
        }
    }

}
