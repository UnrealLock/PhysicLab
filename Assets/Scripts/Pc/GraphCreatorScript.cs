using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphCreatorScript : MonoBehaviour
{
    // Test graph data
    List<double> testXList = new List<double>() { -0.3, -0.2, -0.1, 0, 0.1, 0.2, 0.3 };
    List<double> testYList = new List<double>() { 0, 40, 80, 120, 160, 200, 240 };
    // Lists to store graph points
    List<double> amperageXList = new List<double>();
    List<double> amperageYList = new List<double>();
    List<double> revAmperageXList = new List<double>();
    List<double> revAmperageYList = new List<double>();
    // Graph parameters
    // Coordinates of graph zero point
    Vector2 zero;
    // Scale of how much graph height is reduced
    double scale;
    // Size of one pixel in graph units
    double pixelSize = 1.455;
    // Pixels per graph unit
    double xPixels = 10;
    double yPixels = 4;
    // First graph values for scaling
    double xFirstValue = 0.1;
    double yFirstValue = 20;
    // References to game objects
    GameObject graphField;
    GameObject dotObj;
    GameObject lineObj;

    // Property to check if any graphic exists
    public bool IsHaveOnePoint = false;
    // Property to check if record data exists
    //public bool IsRecordData() => xList.Count >= 20;

    void Start()
    {
        // Get reference to graph field
        graphField = transform.GetChild(1).GetChild(4).gameObject;
        // Get zero point position
        zero = graphField.transform.GetChild(0).transform.localPosition;
        // Calculate scale based on graph field aspect ratio
        scale = graphField.GetComponent<RectTransform>().rect.size.x / graphField.GetComponent<RectTransform>().rect.size.y - 0.65;
        // Load dot and line prefabs
        dotObj = Resources.Load<GameObject>("Objects/Pc/App/Dot");
        lineObj = Resources.Load<GameObject>("Objects/Pc/App/Line");
        //AddLineToGraphTest();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddLineToGraph(float voltage, float amperage, List<double> xList, List<double> yList)
    {
        // Calculate graph coordinates
        var x = voltage;
        var y = amperage;
        // Adjust x coordinate to align with graph scale
        if (y < 0)
            y *= 10;
        x += 0.3f;
        y += 20;
        // Store coordinates in lists
        xList.Add(x);
        yList.Add(y);
        // Set flag indicating at least one point exists
        IsHaveOnePoint = true;
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

    public void AddAmperageLine(float voltage, float amperage)
    {
        if (voltage > 0)
            AddLineToGraph(voltage, amperage, amperageXList, amperageYList);
        else if (voltage < 0)
            AddLineToGraph(voltage, amperage, revAmperageXList, revAmperageYList);
        else
        {
            AddLineToGraph(voltage, amperage, amperageXList, amperageYList);
            AddLineToGraph(voltage, amperage, revAmperageXList, revAmperageYList);
        }
    }

    public void AddLineToGraphTest()
    {
        // Add test data points to the graph to see its work
        for (int i = 0; i < testXList.Count; i++)
        {
            var x = testXList[i];
            var y = testYList[i];
            x += 0.3;
            y += 20;
            amperageXList.Add(x);
            amperageYList.Add(y);
            var dot = Instantiate(dotObj);
            dot.transform.SetParent(graphField.transform, false);
            dot.transform.localPosition = new Vector3((float)(zero.x + x * pixelSize * xPixels / xFirstValue),
                (float)(zero.y + y * scale * pixelSize * yPixels / yFirstValue), -105f);
            if (amperageXList.Count > 1)
            {
                var previousDotPosition = new Vector3((float)(zero.x + amperageXList[amperageXList.Count - 2] * pixelSize * xPixels / xFirstValue),
                    (float)(zero.y + amperageYList[amperageYList.Count - 2] * scale * pixelSize * yPixels / yFirstValue), -10f);
                var line = Instantiate(lineObj);
                line.transform.SetParent(graphField.transform, false);
                line.transform.localPosition = (dot.transform.localPosition + previousDotPosition) / 2;
                var angleRad = Math.Atan((dot.transform.localPosition.y - previousDotPosition.y) /
                    (dot.transform.localPosition.x - previousDotPosition.x));
                var angleDeg = angleRad * Mathf.Rad2Deg;
                line.transform.localRotation = Quaternion.Euler(0,0,90 + (float)angleDeg);
                var dx = dot.transform.localPosition.x - previousDotPosition.x;
                var dy = dot.transform.localPosition.y - previousDotPosition.y;
                var lineLength = (float)Math.Sqrt(dx * dx + dy * dy);
                line.transform.localScale = new Vector3(0.5f, lineLength / 2f, 0.5f);
            }
        }
    }
}
