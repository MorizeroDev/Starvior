using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBoxEvent : MonoBehaviour
{
    public int Value;
    public virtual void ValueChanged()
    {

    }
}
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
    public int style = 0;
    public string LinkDataName = "";
    public int DefaultValue;
    public Animator animator;
    private CheckBoxEvent UIEvent = null;
    private List<CheckBox> CheckBoxes = new List<CheckBox>();
    private int v;
    private bool Initialized = false;
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
            if (!Initialized) return;
            if (LinkDataName != "") PlayerPrefs.SetInt(LinkDataName, v);
            if (UIEvent != null)
            {
                UIEvent.Value = v;
                UIEvent.ValueChanged();
            }
        } 
    }
    [HideInInspector]
    public CheckBox Parent;
    private void Awake()
    {
        if (!isController) return;
        TryGetComponent<CheckBoxEvent>(out UIEvent);
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
            if (style == 0)
                x += (rect.sizeDelta.x / 2 + 200);
            else
                y -= (rect.sizeDelta.y / 2 + 30);
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
        Initialized = true;
    }
    public void MouseUpAsButton()
    {
        if (isController) return;
        if (Parent.Value == id) return;
        Parent.Value = id;
        animator.Play("CheckBoxLit", 0, 0.0f);
    }
}
