using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessCorridor : MonoBehaviour
{
    public GameObject MapUnit;
    public GameObject baseUnit;
    public GameObject Character;
    public static GameObject currentUnit;
    public static GameObject watchUnit;
    public static GameObject oldUnit;
    public static Transform watchFloor;
    public float UnitWide;
    private void Awake() {
        currentUnit = baseUnit;
        watchUnit = baseUnit;
        watchFloor = watchUnit.transform.Find("Floor");
    }
    private void Update() {
        Vector3 pos = Character.transform.position;
        Vector3 wpos = watchFloor.position;
        Camera.main.transform.position = new Vector3(pos.x - 1.5f,Camera.main.transform.position.y,Camera.main.transform.position.z);
        if(pos.x <= wpos.x){
            GameObject newunit = Instantiate(MapUnit,new Vector3(wpos.x - UnitWide,watchUnit.transform.position.y,wpos.z),watchUnit.transform.rotation);
            watchUnit = newunit;
            if(oldUnit != null){
                Destroy(oldUnit);
                oldUnit = null;
            }
            oldUnit = currentUnit;
            watchFloor = watchUnit.transform.Find("Floor");
        }
        if(watchUnit != currentUnit){
            if(pos.x <= wpos.x + UnitWide / 2){
                currentUnit = watchUnit;
            }
        }
    }
}
