using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessCorridor : MonoBehaviour
{
    public GameObject MapUnit;
    public GameObject baseUnit;
    public GameObject Character;
    public static GameObject watchUnit;
    public static GameObject oldUnit;
    public static Transform watchFloor;
    public float UnitWide;
    private void Awake() {
        watchUnit = baseUnit;
        watchFloor = watchUnit.transform.Find("Floor");
    }
    private void Update() {
        Vector3 pos = Character.transform.position;
        Vector3 wpos = watchFloor.position;
        Camera.main.transform.position = new Vector3(pos.x + 2.35f,Camera.main.transform.position.y,Camera.main.transform.position.z);
        if(pos.x >= wpos.x){
            GameObject newunit = Instantiate(MapUnit,new Vector3(wpos.x + UnitWide * 3,watchUnit.transform.position.y,wpos.z),watchUnit.transform.rotation);
            if(oldUnit != null){
                Destroy(oldUnit);
                oldUnit = null;
            }
            oldUnit = watchUnit;
            watchUnit = newunit;
            watchFloor = watchUnit.transform.Find("Floor");
        }
    }
}
