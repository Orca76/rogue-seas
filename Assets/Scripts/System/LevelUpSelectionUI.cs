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
    //        playerObj.GetComponent<Player>().levels[0]++;//HPのレベルを上げる
    //    }
    //}
    //public void OnPowerBtn()
    //{
    //    if (playerObj != null)
    //    {
    //        playerObj.GetComponent<Player>().levels[1]++;//ATのレベルを上げる
    //    }
    //}
    //public void OnAttackSpeedBtn()
    //{
    //    if (playerObj != null)
    //    {
    //        playerObj.GetComponent<Player>().levels[2]++;//攻撃速度のレベルを上げる
    //    }
    //}

    public void OnUpgradeButton(int index)
    {
        if (playerObj != null)
        {
            playerObj.GetComponent<Player>().levels[index]++;
        }
    }
    public void OnHealSentries()//全体回復
    {
        var sentryManager = playerObj.GetComponent<SentryManager>();
        if (sentryManager == null) return;

        foreach (var sentry in sentryManager.sentryList)
        {
            if (sentry == null) continue;

            var sb = sentry.GetComponent<SentryBase>();
            if (sb == null) continue;

            // 5割回復（最大HPの50%を回復）
            sb.HP = Mathf.Min(sb.HP + sb.MaxHP * 0.5f, sb.MaxHP);
        }
    }

}
