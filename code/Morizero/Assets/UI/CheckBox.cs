using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBox : MonoBehaviour
{
    [Tooltip("仅当isController开启时有效。")]
    public GameObject CheckBoxPrefab;
    public bool isController;
    [Tooltip("仅当isController关闭时有效。")]
    public Text text;
    [Tooltip("仅当isController关闭时有效。")]
    public Image image;
    [Tooltip("仅当isController开启时有效。")]
    public Sprite YesSprite, NoSprite;
    [Tooltip("仅当isController开启时有效。")]
    public List<string> Items;
    [HideInInspector]
    public int id;
    public string LinkDataName = "";
    public int DefaultValue;
    private List<CheckBox> CheckBoxes = new List<CheckBox>();
    private int v;
    public int Value {
        get
        {
            return v;
        }
        set
        {
            CheckBoxes[v].image.sprite = NoSprite;
            v = value;
            CheckBoxes[v].image.sprite = YesSprite;
            if (LinkDataName != "") PlayerPrefs.SetInt(LinkDataName, v);
        } 
    }
    [HideInInspector]
    public CheckBox Parent;
    private void Awake()
    {
        if (!isController) return;
        RectTransform r = CheckBoxPrefab.GetComponent<RectTransform>();
        float x = r.localPosition.x,y = r.localPosition.y;
        for(int i = 0;i < Items.Count;i++)
        {
            CheckBox check = Instantiate(CheckBoxPrefab, this.transform).GetComponent<CheckBox>();
            r = check.GetComponent<RectTransform>();
            check.text.text = Items[i];
            check.id = i;
            check.Parent = this;
            check.image.sprite = NoSprite;
            check.gameObject.SetActive(true);
            RectTransform rect = check.text.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            r.localPosition = new Vector3(x, y, 0);
            x += (rect.sizeDelta.x / 2 + 200);
            CheckBoxes.Add(check);
        }
        CheckBoxPrefab.SetActive(false);
        if(LinkDataName == "")
        {
            Value = 0;
        }
        else
        {
            Value = PlayerPrefs.GetInt(LinkDataName, DefaultValue);
        }
        
    }
    public void MouseUpAsButton()
    {
        if (isController) return;
        if (Parent.Value == id) return;
        Parent.Value = id;
    }
}
