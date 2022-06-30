using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [System.Serializable]
    public struct OwnItem
    {
        public string Name;
        public string PickMap;
    }
    public struct ItemInfo
    {
        public string Name;
        public string Description;
    }
    public static List<ItemInfo> Items = new List<ItemInfo>();
    public static List<OwnItem> OwnItems = new List<OwnItem>();
    static ItemManager()
    {
        string[] s = Resources.Load<TextAsset>("GameItem\\Items").text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach(string item in s)
        {
            string[] t = item.Split('|');
            Items.Add(new ItemInfo
            {
                Name = t[0],
                Description = t[1]
            });
        }
    }
}
