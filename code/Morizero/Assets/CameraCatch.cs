using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraCatch : MonoBehaviour
{
  public Texture2D getTexture2d(RenderTexture renderT)
    {
        if (renderT == null)
            return null;

        int width = renderT.width;
        int height = renderT.height;
        Texture2D tex2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderT;
        tex2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex2d.Apply();

        return tex2d;
    }

    void Start()
    {
        
    }

}
