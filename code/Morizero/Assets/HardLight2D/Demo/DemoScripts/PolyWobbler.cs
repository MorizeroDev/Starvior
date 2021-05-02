using UnityEngine;

[RequireComponent (typeof (LineRenderer))]
[RequireComponent (typeof (PolygonCollider2D))]
public class PolyWobbler : MonoBehaviour
{
    PolygonCollider2D Poly;
    LineRenderer LineRend;
    public float Wobbles = 1;
    Vector2[] points;

    private void Start ()
    {
        Poly = GetComponent<PolygonCollider2D> ();
        LineRend = GetComponent<LineRenderer> ();
    }

    void Update ()
    {
        points = Poly.GetPath (0);
        LineRend.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += Random.insideUnitCircle * Time.deltaTime * Wobbles;
            LineRend.SetPosition (i, points[i]);
        }
        Poly.SetPath (0, points);
        HardLight2DManager.RefreshColliderReference (Poly);
    }
}