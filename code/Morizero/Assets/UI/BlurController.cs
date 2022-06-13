using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurController : MonoBehaviour
{
    public RenderTexture renderTexture, blurTexture;
    public Canvas Container;
    public GameObject Displayer;
    private Texture2D outputTexture;
    private void Awake()
    {
        outputTexture = new Texture2D(renderTexture.width, renderTexture.height);
        Camera.main.targetTexture = renderTexture;
        Camera.main.Render();
        Camera.main.targetTexture = null;
        GameObject fab = (GameObject)Resources.Load("Prefabs\\UIBlurSnaphost");    // ‘ÿ»Îƒ∏ÃÂ
        GameObject cam = Instantiate(fab, new Vector3(0, 0, -1), Quaternion.identity, null);
        Camera orcam = Container.worldCamera,ncam = cam.GetComponent<Camera>();
        Container.worldCamera = ncam;
        bool[] active = new bool[Container.transform.childCount];
        for(int i = 0;i < Container.transform.childCount; i++)
        {
            active[i] = Container.transform.GetChild(i).gameObject.activeSelf;
            if(!Container.transform.GetChild(i).gameObject.Equals(this.gameObject))
                Container.transform.GetChild(i).gameObject.SetActive(false);
        }
        ncam.GetComponent<CameraFollow>().Adjust();
        ncam.Render();
        RenderTexture.active = blurTexture;
        outputTexture.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
        outputTexture.Apply();
        Displayer.GetComponent<RawImage>().texture = outputTexture;
        RenderTexture.active = null;
        blurTexture.Release();
        Destroy(cam);
        for (int i = 0; i < Container.transform.childCount; i++)
        {
            Container.transform.GetChild(i).gameObject.SetActive(active[i]);
        }
        Container.worldCamera = orcam;
        GetComponent<Image>().material = null;
        GetComponent<Image>().color = new Color(0, 0, 0, 0);
        Displayer.SetActive(true);
    }
    private void OnDestroy()
    {
        Destroy(outputTexture);
    }
}
