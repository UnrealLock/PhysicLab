using UnityEngine;

public class ContraptionZoneData : MonoBehaviour
{
    // Flag to indicate if the system is on
    //[SerializeField] private OnSwitchButtonScript onSwitchCoilData;
    //[SerializeField] private OnSwitchButtonScript onSwitchLampData;
    [SerializeField] private OnSwitchButtonScript onSwitchSystemData;

    //public bool IsLampOn => onSwitchLampData != null && onSwitchLampData.IsActivated;
    //public bool IsCoilOn => onSwitchCoilData != null && onSwitchCoilData.IsActivated;
    public bool IsSystemOn => onSwitchSystemData != null && onSwitchSystemData.IsActivated;
    [SerializeField] public bool IsReverse = false;

    // Values of the contraption
    public double Voltage = 0.00;
    public double Amperage = 0.00;
    public double RevAmperage = 0.00;
    public double RoomTemperature = 0.00;
    public double PermZone = 0.00;
    public const double elCharge = 1.6e-19;
    public const double k = 1.38e-23;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize constants k and T with random values in specified ranges
        var random = new System.Random();
        RoomTemperature = 18 + random.NextDouble() * 10;
        PermZone = 0.33 + random.NextDouble() * 0.04;
    }

    private void Update()
    {
        if ((IsReverse && Voltage > 0) || (!IsReverse && Voltage < 0))
            Voltage = 0.0;
    }
}
