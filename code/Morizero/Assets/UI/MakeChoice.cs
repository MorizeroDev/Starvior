using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void MakeChoiceCallback();
public class MakeChoice : MonoBehaviour
{
    public Text Explaination;
    public GameObject ChoicePrefab;
    public Sprite Focus, UnFocus;
    public Image BtnImage;
    public MakeChoiceCallback Callback;
    public int id;
    public MakeChoice parent;
    public static int choiceId = -1, choiceMax = 0;
    public static bool choiceFinished = false;
    private List<GameObject> Choices = new List<GameObject>();

    public static void Create(MakeChoiceCallback callback, string explain,string[] choices){
        GameObject fab = (GameObject)Resources.Load("Prefabs\\MakeChoice");    // 载入母体
        GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        MakeChoice mc = box.GetComponent<MakeChoice>();
        float y = (choices.Length - 1) * 100 - 140;
        for(int i = 0;i < choices.Length;i++){
            GameObject Choice = Instantiate(mc.ChoicePrefab,
                                            new Vector3(mc.ChoicePrefab.transform.localPosition.x,y,-1),
                                            Quaternion.identity,
                                            box.transform);
            Choice.transform.localPosition = new Vector3(mc.ChoicePrefab.transform.localPosition.x,y,0);
            MakeChoice choice = Choice.GetComponent<MakeChoice>();
            choice.Explaination.text = choices[i];
            choice.id = i;
            choice.parent = mc;
            mc.Choices.Add(Choice);
            Choice.SetActive(true);
            y -= 200;
        }
        mc.parent = mc;
        mc.Explaination.text = explain;
        mc.Callback = callback;
        choiceId = 0;
        choiceMax = choices.Length - 1;
        choiceFinished = false;
        box.SetActive(true);
    }

    void UnloadMakeChoice(){
        if(choiceId != id) return;
        parent.Callback();
        Destroy(parent.gameObject);
    }

    public void OnClick(BaseEventData data) {
        ChoiceClick(id);
    }

    void ChoiceClick(int Id)
    {
        if (choiceFinished) return;
        if (choiceId != Id)
        {
            choiceId = Id;
            return;
        }
        foreach (GameObject go in parent.Choices)
        {
            if (go.GetComponent<MakeChoice>().id != Id) 
                go.GetComponent<Animator>().Play("ChoiceNo", 0);
            else
                go.GetComponent<Animator>().Play("ChoiceYes", 0);
        }
        parent.gameObject.GetComponent<Animator>().Play("MakeChoiceExit", 0);
        choiceFinished = true;
    }

    private void Update()
    {
        if (id == -1)
        {
            if (Input.GetKeyUp(KeyCode.DownArrow)) choiceId++;
            if (Input.GetKeyUp(KeyCode.UpArrow)) choiceId--;
            if (choiceId < 0) choiceId = choiceMax;
            if (choiceId > choiceMax) choiceId = 0;
            if (Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space)) ChoiceClick(choiceId);
            return;
        }

        if (id == choiceId)
            BtnImage.sprite = Focus;
        else
            BtnImage.sprite = UnFocus;
    }
}
