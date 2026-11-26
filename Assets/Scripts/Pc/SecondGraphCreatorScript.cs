using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;

public class SecondGraphCreatorScript : MonoBehaviour
{
    // Test graph data
    List<double> testXList = new List<double>() { 3.1, 3.15, 3.2, 3.25, 3.3, 3.35, 3.4 };
    List<double> testYList = new List<double>() { 0.7, 0.8, 0.9, 1, 1.1, 1.2, 1.3 };
    // Lists to store graph points
    List<double> xList = new List<double>();
    List<double> yList = new List<double>();
    // Graph parameters
    // Coordinates of graph zero point
    Vector2 zero;
    // Scale of how much graph height is reduced
    double scale;
    // Size of one pixel in graph units
    double pixelSize = 2.12;
    // Pixels per graph unit
    double xPixels = 10;
    double yPixels = 20;
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
        graphField = transform.GetChild(2).GetChild(3).GetChild(1).gameObject;
        zero = transform.GetChild(2).GetChild(3).GetChild(0).localPosition;
        // Set scale of graph rectangle by hands
        scale = graphField.GetComponent<RectTransform>().rect.size.x / graphField.GetComponent<RectTransform>().rect.size.y - 0.85;
        // Load dot and line prefabs
        dotObj = Resources.Load<GameObject>("Objects/Pc/App/Dot");
        lineObj = Resources.Load<GameObject>("Objects/Pc/App/Line");
        //AddTestLines();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddLineToGraph(float voltage, float amperage)
    {
        // Calculate graph coordinates
        var x = voltage;
        var y = amperage;
        // Adjust x coordinate to align with graph scale
        x -= 3.1f;
        y -= 0.7f;
        // Store coordinates in lists
        xList.Add(x);
        yList.Add(y);
        // Instantiate dot at calculated position and parent to graph field
        var dot = Instantiate(dotObj);
        dot.transform.SetParent(graphField.transform, false);
        // Set dot position based on graph coordinates, scaling, and pixel size
        dot.transform.localPosition = new Vector3((float)(zero.x + x * pixelSize * xPixels / xFirstValue),
            (float)(zero.y + y * scale * pixelSize * yPixels / yFirstValue), -105f);
        if (xList.Count > 1)
        {
            // If there is more than one point, draw a line connecting the last two points
            var previousDotPosition = new Vector3((float)(zero.x + xList[xList.Count - 2] * pixelSize * xPixels / xFirstValue),
                (float)(zero.y + yList[yList.Count - 2] * scale * pixelSize * yPixels / yFirstValue), -10f);
            var line = Instantiate(lineObj);
            line.transform.SetParent(graphField.transform, false);
            // Set line position to midpoint between current and previous dot
            line.transform.localPosition = (dot.transform.localPosition + previousDotPosition) / 2;
            // Calculate angle for line rotation based on slope between points
            var angleRad = Math.Atan((dot.transform.localPosition.y - previousDotPosition.y) /
                (dot.transform.localPosition.x - previousDotPosition.x));
            var angleDeg = angleRad * Mathf.Rad2Deg;
            line.transform.localRotation = Quaternion.Euler(0, 0, 90 + (float)angleDeg);
            // Calculate length of line based on distance between points
            var dx = dot.transform.localPosition.x - previousDotPosition.x;
            var dy = dot.transform.localPosition.y - previousDotPosition.y;
            var lineLength = (float)Math.Sqrt(dx * dx + dy * dy);
            // Scale line to match calculated length
            line.transform.localScale = new Vector3(0.5f, lineLength / 2f, 0.5f);
        }
    }

    public void AddTestLines()
    {
        //Drawing graphs with test data to check its work
        for (int i = 0; i < testXList.Count; ++i)
            AddLineToGraph((float)testXList[i], (float)testYList[i]);
    }      
}
