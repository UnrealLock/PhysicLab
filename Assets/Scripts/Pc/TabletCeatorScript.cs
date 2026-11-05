using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TabletCeatorScript : MonoBehaviour
{
    // References to necessary objects and data
    GameObject tablet;
    GameObject tabletRowPrefab;
    GameObject tabletContent;
    GameObject tabletStartRow;
    // Lists to store data rows for both tablets
    List<float[]> tabletDataRows = new List<float[]>();
    // Reference to contraption zone data and second graph creator script
    ContraptionZoneData contraptionZoneData;
    SecondGraphCreatorScript secondGraphCreatorScript;
    // Start is called before the first frame update
    void Start()
    {
        //Search and initialisation of necessary objects and components
        tabletRowPrefab = Resources.Load<GameObject>("Objects/Pc/App/TextRow");
        tablet = this.transform.GetChild(2).GetChild(0).gameObject;
        tabletContent = tablet.transform.GetChild(0).GetChild(0).gameObject;
        tabletStartRow = tabletContent.transform.GetChild(1).gameObject;
        contraptionZoneData = GameObject.FindGameObjectWithTag("ContraptionZone").GetComponent<ContraptionZoneData>();
        secondGraphCreatorScript = transform.GetComponent<SecondGraphCreatorScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FillTabletRow(float coilAmperage, float lampAmperage)
    {
        // Create data row with data we have
        var derivative = 0f;
        tabletDataRows.Add(new float[3] { coilAmperage, lampAmperage, derivative });
        //If we can count derivative - create new row in tablet
        if (tabletDataRows.Count > 2)
        {
            //Instantiate new row object and set its parameters
            var newRow = Instantiate(tabletRowPrefab, tabletContent.transform);
            newRow.transform.SetParent(tabletContent.transform);
            newRow.transform.localScale = new Vector3(1, 1, 1);
            newRow.transform.localPosition = tabletStartRow.transform.localPosition + new Vector3(0, -3.44f * (tabletDataRows.Count - 3), 0);
            var dataRow = tabletDataRows[tabletDataRows.Count - 2];
            //Calculate derivative using the two points method
            derivative = System.Math.Abs(
                tabletDataRows[tabletDataRows.Count - 1][1] - tabletDataRows[tabletDataRows.Count - 3][1]) /
                (tabletDataRows[tabletDataRows.Count - 1][0] - tabletDataRows[tabletDataRows.Count - 3][0]);
            //Fill row texts with data
            tabletDataRows[tabletDataRows.Count - 2][2] = derivative;
            dataRow[2] = derivative;
            for (int i = 0; i < dataRow.Length; i++)
                newRow.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text = NormaliseValue(dataRow[i],2);
            // After filling row - refill derivatives row with normalised values and redraw graph
            RefillDerivativesRow();
            RedrawDerivativesGraph();
        }
    }


    void RefillDerivativesRow()
    {
        //Find max derivative
        var maxDerivative = 0f;
        for (int i = 1; i < tabletDataRows.Count - 1; i++)
        {
            if (tabletDataRows[i][2] > maxDerivative)
                maxDerivative = tabletDataRows[i][2];
        }
        //Refill experimental power row with normalised derivative values
        for (int i = 2; i < tabletContent.transform.childCount; i++)
        {
            float derivativeNorm = (float)tabletDataRows[i - 2 + 1][2] / maxDerivative;
            tabletContent.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text =
                NormaliseValue(derivativeNorm, 3);
        }
    }

    void RedrawDerivativesGraph()
    {
        //Empty lists for graph data
        var amperagesList = new List<float>();
        var derivativesList = new List<float>();
        //Find max theoretical power
        var maxDerivative = 0f;
        for (int i = 1; i < tabletDataRows.Count - 1; i++)
        {
            if (tabletDataRows[i][2] > maxDerivative)
                maxDerivative = tabletDataRows[i][2];
        }
        //Fill lists with normalised data
        for (int i = 1; i < tabletDataRows.Count - 1; i++)
        {
            amperagesList.Add(tabletDataRows[i][0]);
            derivativesList.Add(tabletDataRows[i][2] / maxDerivative);
        }
        //Redraw graph with new data
        secondGraphCreatorScript.RedrawGraph(amperagesList, derivativesList);
    }

    public void FillTabletTestData()
    {
        //Filling tablets with test data to check its work
        for (int i = 0; i < 20; i++)
        {
            var newFirstRow = Instantiate(tabletRowPrefab, tabletContent.transform);
            newFirstRow.transform.SetParent(tabletContent.transform);
            newFirstRow.transform.localScale = new Vector3(1, 1, 1);
            newFirstRow.transform.localPosition = tabletStartRow.transform.localPosition + new Vector3(0, -3.44f * i, 0);
            foreach (Transform child in newFirstRow.transform)
                child.GetComponent<TextMeshProUGUI>().text = string.Concat(i.ToString(),".00");
        }
    }

    string NormaliseValue(float value, int degree)
    {
        //Convert float to string with necessary number of decimal places
        var stringValue = System.Math.Round(value,degree).ToString();
        var indexOfDot = stringValue.IndexOf(',');
        if (degree == 0)
            return stringValue;
        if (indexOfDot == -1 && degree > 0)
            stringValue = string.Concat(stringValue,",0");
        while (stringValue.Split(',')[1].Length < degree)
            stringValue = string.Concat(stringValue,"0");
        return stringValue;
    }
}
