using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    public Text Title, Pickup;
    public static bool Clicked = false;
    public void OnClick()
    {
        if (Clicked) return;
        Clicked = true;
        string ItemName = Title.text;
        Dramas d = Dramas.LaunchScript("ItemInfo", () => { Clicked = false; });
        d.LifeTime = Dramas.DramaLifeTime.DieWhenReadToEnd;
        d.Drama.Add(new Dramas.DramaData
        {
            Character = ItemName,
            motion = "",
            Effect = WordEffect.Effect.None,
            content = ItemManager.Items.Find(m => m.Name == ItemName).Description,
            Speed = 0.03f
        });
        d.IgnoreSettingBlocks = true;
        d.NoRecord = true;
        d.ReadDrama();
    }
}
