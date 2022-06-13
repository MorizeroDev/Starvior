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
    public MakeChoiceCallback Callback;
    public int id;
    public MakeChoice parent;
    public static int choiceId = -1;
    private List<GameObject> Choices = new List<GameObject>();

    public static void Create(MakeChoiceCallback callback, string explain,string[] choices){
        GameObject fab = (GameObject)Resources.Load("Prefabs\\MakeChoice");    // 载入母体
        GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        MakeChoice mc = box.GetComponent<MakeChoice>();
        float y = (choices.Length - 1) * 150;
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
            y -= 300;
        }
        mc.Explaination.text = explain;
        mc.Callback = callback;
        choiceId = -1;
        box.SetActive(true);
    }

    void UnloadMakeChoice(){
        if(choiceId != id) return;
        parent.Callback();
        Destroy(parent.gameObject);
    }

    public void OnClick(BaseEventData data) {
        if(choiceId != -1) return;
        foreach(GameObject go in parent.Choices){
            if(go != this.gameObject) go.GetComponent<Animator>().Play("ChoiceNo",0);
        }
        this.GetComponent<Animator>().Play("ChoiceYes",0);
        this.transform.parent.gameObject.GetComponent<Animator>().Play("MakeChoiceExit",0);
        choiceId = id;
    }
}
