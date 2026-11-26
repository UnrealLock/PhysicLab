using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AppScript : MonoBehaviour
{
    // References to other scripts and game objects
    ContraptionZoneData contraptionZoneData;
    GameObject graphicPage;
    GameObject resultPage;
    GraphCreatorScript graphCreatorScript;
    // Saved voltages for graphing
    List<double> savedVoltages;
    // User inputs
    float tempInput;
    int powerInput;
    // Temperature
    TextMeshProUGUI tempField;
    int heatAmount = 0;
    int timer = 0;

    //For Tasks
    public bool IsTempCorrect = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get references to other scripts and game objects
        contraptionZoneData = GameObject.FindGameObjectWithTag("ContraptionZone").GetComponent<ContraptionZoneData>();
        graphicPage = gameObject.transform.GetChild(1).gameObject;
        resultPage = gameObject.transform.GetChild(2).gameObject;
        graphCreatorScript = gameObject.transform.GetComponent<GraphCreatorScript>();
        savedVoltages = new List<double>();
        tempField = resultPage.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>();
        tempField.text = NormaliseValue((float)contraptionZoneData.RoomTemperature + 273, 1);
    }

    // Update is called once per frame
    void Update()
    {
        // Monitor data and check user inputs
        MonitoreVoltage();
        CheckDataCorrect();
    }

    void FixedUpdate()
    {
        ++timer;
        //Debug.Log(timer);
        if (heatAmount < 20 && contraptionZoneData.IsFurnaceOn)
        {
            if (timer > 3000)
            {
                tempField.text = NormaliseValue(float.Parse(tempField.text) + 2, 1);
                heatAmount += 2;
                timer = 0;
            }
        }
        else if (heatAmount > 0 && !contraptionZoneData.IsFurnaceOn)
        {
            if (timer > 3000)
            {
                tempField.text = NormaliseValue(float.Parse(tempField.text) - 1, 1);
                --heatAmount;
                timer = 0;
            }
        }
        else
            timer = 0;
    }

    void MonitoreVoltage()
    {
        // Get voltage text component
        var voltageField = transform.GetChild(1).GetChild(0);
        var voltageText = voltageField.GetChild(1).GetComponent<TextMeshProUGUI>();
        // Parse voltage value
        var voltageValue = contraptionZoneData.IsSystemOn ? double.Parse(voltageText.text) : 0.0;
        // Update value if system is on and there is a significant change
        if (System.Math.Abs(contraptionZoneData.Voltage - voltageValue) > 0.001)
        {
            // Update voltage text
            if (contraptionZoneData.IsSystemOn)
                voltageText.text = NormaliseValue((float)System.Math.Round(contraptionZoneData.Voltage, 2), 2);
            else
                voltageText.text = NormaliseValue(0.0f, 2);
            MonitoreAmperage();
            MonitoreRevAmperage();
        }
    }

    void MonitoreAmperage() {
        var amperageField = transform.GetChild(1).GetChild(1);
        // Get amperage text component
        var amperageText = amperageField.GetChild(1).GetComponent<TextMeshProUGUI>();
        //Get current coil amperage
        // Calculate lamp amperage value based on the formula
        var mn = Math.Exp((-1 * contraptionZoneData.PermZone * ContraptionZoneData.elCharge)
            / (ContraptionZoneData.k * (contraptionZoneData.RoomTemperature + 273)));
        var exponent = Math.Exp(ContraptionZoneData.elCharge * contraptionZoneData.Voltage
            / (ContraptionZoneData.k * (contraptionZoneData.RoomTemperature + 273)));
        var amperageNew = mn * 10e2 * (exponent - 1);
        if (!contraptionZoneData.IsSystemOn || contraptionZoneData.currentConnection == CircuitType.Reverse)
            amperageNew = 0.0;
        contraptionZoneData.Amperage = amperageNew;
        amperageText.text = NormaliseValue((float)System.Math.Round(amperageNew, 2), 2);
    }

    void MonitoreRevAmperage()
    {
        var random = new System.Random();
        var revAmperageField = transform.GetChild(1).GetChild(2);
        // Get reverse amperage text component
        var revAmperageText = revAmperageField.GetChild(1).GetComponent<TextMeshProUGUI>();
        // Calculate lamp amperage value based on the formula
        var mn = Math.Exp((-1 * contraptionZoneData.PermZone * ContraptionZoneData.elCharge)
            / (ContraptionZoneData.k * (contraptionZoneData.RoomTemperature + 273)));
        var exponent = Math.Exp(1 * ContraptionZoneData.elCharge * contraptionZoneData.Voltage
            / (ContraptionZoneData.k * (contraptionZoneData.RoomTemperature + 273)));
        var revAmperageNew = mn * 10e5 * (exponent - 1) + random.NextDouble() / 10;
        if (!contraptionZoneData.IsSystemOn || contraptionZoneData.currentConnection == CircuitType.Forward)
            revAmperageNew = 0.0;
        contraptionZoneData.RevAmperage = revAmperageNew;
        revAmperageText.text = NormaliseValue((float)System.Math.Round(revAmperageNew, 2), 2);
        resultPage.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            NormaliseValue((float)System.Math.Round(revAmperageNew, 2), 2);

    }

    public void AddData()
    {
        var voltageField = transform.GetChild(1).GetChild(0);
        var voltageText = voltageField.GetChild(1).GetComponent<TextMeshProUGUI>();
        var voltageValue = float.Parse(voltageText.text);
        var amperageField = transform.GetChild(1).GetChild(1);
        if (contraptionZoneData.currentConnection == CircuitType.Reverse)
            amperageField = transform.GetChild(1).GetChild(2);
        var amperageText = amperageField.GetChild(1).GetComponent<TextMeshProUGUI>();
        var currentLampAmperage = float.Parse(amperageText.text);
        // Add data point to graph and tablet if voltage is new and system is on
        if (savedVoltages.Contains(System.Math.Round(voltageValue, 3)) || !contraptionZoneData.IsSystemOn) return;
        savedVoltages.Add(System.Math.Round(voltageValue, 3));
        graphCreatorScript.AddAmperageLine(voltageValue, currentLampAmperage);
    }

    public void AddSecondData()
    {

    }

    public void ShowResultPage()
    {
        // Show result page and hide graphic page
        resultPage.SetActive(true);
        graphicPage.SetActive(false);
    }

    public void ShowGraphicPage()
    {
        // Show graphic page and hide result page
        graphicPage.SetActive(true);
        resultPage.SetActive(false);
    }

    void CheckDataCorrect()
    {
        // If result page is not active, do nothing
        if (!resultPage.activeSelf)
            return;
        // Get input fields for charge and power
        var tempInputField = resultPage.transform.GetChild(0).GetChild(2).GetComponent<TMP_InputField>();
        // Check charge input if text field is not empty
        if (tempInputField.text != "")
        {
            // Get new charge input value and check if it can be parsed to float
            if (!float.TryParse(tempInputField.text.Replace('.', ','), out var newTempInput))
                return;
            // Check if input has changed significantly
            if (System.Math.Abs(newTempInput - tempInput) > 0.01)
            {
                tempInput = newTempInput;
                // Check if input is within 5% of expected value and update color accordingly
                if (System.Math.Abs(tempInput - contraptionZoneData.RoomTemperature) < 0.2)
                {
                    //tempInputField.textComponent.color = Color.green;
                    IsTempCorrect = true;
                }
                else
                {
                    //tempInputField.textComponent.color = Color.black;
                    IsTempCorrect = false;
                }
            }
        }
    }

    string NormaliseValue(float value, int degree)
    {
        //Convert float to string with necessary number of decimal places
        var stringValue = System.Math.Round(value, degree).ToString();
        var indexOfDot = stringValue.IndexOf(',');
        if (degree == 0)
            return stringValue;
        if (indexOfDot == -1 && degree > 0)
            stringValue = string.Concat(stringValue, ",0");
        while (stringValue.Split(',')[1].Length < degree)
            stringValue = string.Concat(stringValue, "0");
        return stringValue;
    }
}
