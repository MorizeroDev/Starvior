using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrama : MonoBehaviour
{
    public bool Testing;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    async void OnMouseUp()
    {
        // 测试对话喽
        if(Testing) return;
        Testing = true;
        await Dramas.Prepare();
        await Dramas.Msg("林桔","你......你......你不要这么盯着我看。","Drama_Focus");
        await Dramas.Msg("奈美","哈哈哈哈哈哈哈哈哈。","Drama_Leap", WordEffect.Effect.Shake);
        await Dramas.Msg("奈美","你叫我笑小声一些？那怎么可能啊，哈哈哈哈哈哈哈。","Drama_Leap", WordEffect.Effect.HeavyShake);
        await Dramas.Msg("兮","布拉布拉布拉乌拉乌拉。","Drama_Enter", WordEffect.Effect.Rainbow);
        await Dramas.Msg("兮","你说我的字太花了，让我换一个？","Drama_Focus");
        await Dramas.Msg("兮","什么，你说更看不清了？","Drama_Enter", WordEffect.Effect.Rotation);
        await Dramas.Msg("兮","？？？？那你要怎么样才好嘛？","Drama_Leap", WordEffect.Effect.Shine);
        await Dramas.Msg("林桔","看来只有我的话是正常的了。","Drama_Enter");
        await Dramas.End();
        Testing = false;
    }
}
