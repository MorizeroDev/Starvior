using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBtnController : MonoBehaviour
{
    public void Hide()
    {
        if(this.transform.parent.GetComponent<MakeChoice>().id != MakeChoice.choiceId)
            this.gameObject.SetActive(false);
    }
}
