using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltageControllerScript : MonoBehaviour, IClickable
{
    public double VoltageChangeValue = 0.01;
    private ContraptionZoneData contraptionZoneData;
    // Start is called before the first frame update
    void Start()
    {
        contraptionZoneData = GameObject.FindGameObjectWithTag("ContraptionZone").GetComponent<ContraptionZoneData>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLeftClick()
    {
        if (contraptionZoneData.IsSystemOn)
        {
            // increase voltage
            contraptionZoneData.Voltage = System.Math.Round(contraptionZoneData.Voltage + VoltageChangeValue, 2);
            // wrap around if exceeds max
            var (min, max) = GetMinMax();
            if (contraptionZoneData.Voltage > max)
                contraptionZoneData.Voltage = min;
            // start cooldown
            StartCoroutine(Cooldown(0.2f));
        }
    }

    public void OnRightClick()
    {
        // get contraption zone data
        if (contraptionZoneData.IsSystemOn)
        {
            // decrease voltage
            contraptionZoneData.Voltage = System.Math.Round(contraptionZoneData.Voltage - VoltageChangeValue, 2);
            // wrap around if below min
            var (min, max) = GetMinMax();
            if (contraptionZoneData.Voltage < min)
                contraptionZoneData.Voltage = max;
            // start cooldown
            StartCoroutine(Cooldown(0.2f));
        }
    }

    System.Collections.IEnumerator Cooldown(float cooldownTimer)
    {
        // Set cooldown flag and wait for the specified time
        yield return new WaitForSeconds(cooldownTimer);
    }

    Tuple<double, double> GetMinMax() {
        switch (contraptionZoneData.currentConnection)
        {
            case CircuitType.Forward:
                return Tuple.Create(0.0, 0.3);
            case CircuitType.Reverse:
                return Tuple.Create(-0.3, 0.0);
        }
        return Tuple.Create(double.MinValue, double.MaxValue);
    }
}
