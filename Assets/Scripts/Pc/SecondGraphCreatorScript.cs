using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;

public class SecondGraphCreatorScript : MonoBehaviour
{
    // Lists to store graph points
    List<float> xList = new List<float>();
    List<float> yList = new List<float>();
    // Graph parameters
    // Coordinates of graph zero point
    Vector2 zero;
    // Scale of how much graph height is reduced
    double scale;
    double linesScale;
    // Size of one pixel in graph units
    double pixelSize = 1.265;
    // Pixels per graph unit
    double xPixels = 10;
    double yPixels = 5;
    // First graph values for scaling
    double xFirstValue = 0.1;
    double yFirstValue = 0.1;
    // References to game objects
    GameObject graphField;
    GameObject dotObj;
    GameObject lineObj;

    // Property to check if any graphic exists
    public bool IsGraphicExist => xList.Count > 0;
    void Start()
    {
        // Initialize references
        graphField = transform.GetChild(2).GetChild(1).GetChild(1).gameObject;
        zero = transform.GetChild(2).GetChild(1).GetChild(0).localPosition;
        // Set scale of graph rectangle by hands
        scale = 1.09;
        linesScale = 0.7;
        // Load dot and line prefabs
        dotObj = Resources.Load<GameObject>("Objects/Pc/App/Dot");
        lineObj = Resources.Load<GameObject>("Objects/Pc/App/Line");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddLineToGraph(float coilAmperage, float derivative)
    {
        // Calculate graph coordinates
        var x = coilAmperage - 0.4f;
        // Adjust x coordinate to align with graph scale
        var y = derivative;
        // Store coordinates in lists
        xList.Add(x);
        yList.Add(y);
        // Create dot object and set its parameters
        var dot = Instantiate(dotObj);
        dot.transform.SetParent(graphField.transform, false);
        // Set dot position based on graph scaling
        dot.transform.localPosition = new Vector3((float)(zero.x + x * pixelSize * xPixels / xFirstValue),
            (float)(zero.y + y * scale * pixelSize * yPixels / yFirstValue), -105f);
    }

    public void RedrawGraph(List<float> amperages, List<float> derivatives)
    {
        // Clear existing data
        xList = new List<float>();
        yList = new List<float>();
        // Destroy existing graphs
        for (int i = 0; i < graphField.transform.childCount; i++)
            Destroy(graphField.transform.GetChild(i).gameObject);
        // Re-add lines based on new data
        for (int i = 0; i < amperages.Count; i++)
            AddLineToGraph(amperages[i], derivatives[i]);
    }

    public void AddTestLines()
    {
        //Drawing graphs with test data to check its work
        for (int i = 0; i < 6; i++)
        {
            AddLineToGraph(0.4f + i * 0.1f, i * 10 * 2);
        }
    }       
}
