using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class MeshCopy : MonoBehaviour
{
    public MeshFilter OriginalMesh;
    [ColorUsage (false)] public Color Color = Color.white;
    public float Intensity = 1;

    MeshFilter meshFilter;
    Renderer rend;
    Color oldColor = Color.black;
    float oldIntensity = -1;
    MaterialPropertyBlock propBlock;
    string colorProp = "_Color";

    void Update ()
    {
        CheckReferences ();
        if (OriginalMesh && OriginalMesh.sharedMesh)
            meshFilter.sharedMesh = OriginalMesh.sharedMesh;
        else meshFilter.sharedMesh = null;
        UpdateColor ();
    }
    void CheckReferences ()
    {
        if (!meshFilter) meshFilter = GetComponent<MeshFilter> ();
        if (!rend) rend = GetComponent<Renderer> ();
        if (propBlock == null) propBlock = new MaterialPropertyBlock ();
    }
    void UpdateColor ()
    {
        if (oldColor != Color || oldIntensity != Intensity)
        {
            oldColor = Color;
            oldIntensity = Intensity;
            rend.GetPropertyBlock (propBlock);
            propBlock.SetColor (colorProp, Color * Intensity);
            rend.SetPropertyBlock (propBlock);
        }
    }
}