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
    public Animator Lit, UnLit;
    public MakeChoiceCallback Callback;
    public int id;
    private int lastid = 0;
    public MakeChoice parent;
    public static int choiceId = -1, choiceMax = 0;
    public static bool choiceFinished = false;
    private List<GameObject> Choices = new List<GameObject>();

    public void DisableAnimator()
    {
        foreach (GameObject go in parent.Choices)
        {
            MakeChoice mc = go.GetComponent<MakeChoice>();
            mc.Lit.enabled = false;
            mc.UnLit.enabled = false;
        }
    }
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
            if(i == 0)
            {
                choice.Lit.gameObject.SetActive(true);
                choice.UnLit.gameObject.SetActive(false);
            }
            else
            {
                choice.Lit.gameObject.SetActive(false);
                choice.UnLit.gameObject.SetActive(true);
            }
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

        if (lastid != choiceId)
        {
            if(choiceId == id)
            {
                // 摆脱父对象的动画机控制并显示
                Lit.gameObject.SetActive(false);
                UnLit.gameObject.SetActive(false);
                Lit.gameObject.SetActive(true);
                UnLit.gameObject.SetActive(true);
                Lit.Play("ChoiceLit", 0);
                UnLit.Play("ChoiceUnLit", 0);
            }
            else if(lastid == id)
            {
                // 摆脱父对象的动画机控制并显示
                Lit.gameObject.SetActive(false);
                UnLit.gameObject.SetActive(false);
                Lit.gameObject.SetActive(true);
                UnLit.gameObject.SetActive(true);
                Lit.Play("ChoiceUnLit", 0);
                UnLit.Play("ChoiceUnLitLit", 0);
            }
            lastid = choiceId;
        }
    }
}
