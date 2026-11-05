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
        // get contraption zone data
        var contraptionZoneData = GameObject.FindGameObjectWithTag("ContraptionZone").GetComponent<ContraptionZoneData>();

        if (contraptionZoneData.IsSystemOn)
        {
            // increase voltage
            contraptionZoneData.Voltage = System.Math.Round(contraptionZoneData.Voltage + VoltageChangeValue, 2);
            // wrap around if exceeds max
            if (!contraptionZoneData.IsReverse && contraptionZoneData.Voltage > 0.3)
                contraptionZoneData.Voltage = 0.0;
            if (contraptionZoneData.IsReverse && contraptionZoneData.Voltage > 0)
                contraptionZoneData.Voltage = -0.3;
            // start cooldown
            StartCoroutine(Cooldown(0.2f));
        }
    }

    public void OnRightClick()
    {
        // get contraption zone data
        var contraptionZoneData = GameObject.FindGameObjectWithTag("ContraptionZone").GetComponent<ContraptionZoneData>();
        if (contraptionZoneData.IsSystemOn)
        {
            // decrease voltage
            if (contraptionZoneData.IsSystemOn)
                contraptionZoneData.Voltage = System.Math.Round(contraptionZoneData.Voltage - VoltageChangeValue, 2);
            // wrap around if below min
            if (!contraptionZoneData.IsReverse && contraptionZoneData.Voltage < 0.0)
                contraptionZoneData.Voltage = 0.3;
            if (contraptionZoneData.IsReverse && contraptionZoneData.Voltage < -0.3)
                contraptionZoneData.Voltage = 0;

            // start cooldown
            StartCoroutine(Cooldown(0.2f));
        }
    }

    System.Collections.IEnumerator Cooldown(float cooldownTimer)
    {
        // Set cooldown flag and wait for the specified time
        yield return new WaitForSeconds(cooldownTimer);
    }
}
