using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class GraphLine : MonoBehaviour
{

    public Color lineColor;
    private LineRenderer lineRenderer;
    private List<Vector3> dataPoints = new List<Vector3>();

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
    }

    public void StartNewLine()
    {
        dataPoints.Clear();
        lineRenderer.positionCount = 0;
    }

    public void AddDataPoint(float time, int count)
    {
        Debug.Log(1);
        Vector3 newPoint = new Vector3(time, count, 0);
        dataPoints.Add(newPoint);
        lineRenderer.positionCount = dataPoints.Count;
        lineRenderer.SetPositions(dataPoints.ToArray());
    }

}
