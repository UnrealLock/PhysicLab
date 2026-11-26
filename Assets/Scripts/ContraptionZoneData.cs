using UnityEngine;

public class ContraptionZoneData : MonoBehaviour
{
    // Flag to indicate if the system is on
    [SerializeField] private OnSwitchButtonScript onSwitchSystemData;
    [SerializeField] private SocketsSystemScript socketsSystemScript;
    public bool IsSystemOn => onSwitchSystemData != null && onSwitchSystemData.IsActivated;
    [SerializeField] public bool IsFurnaceOn = false;
    public CircuitType currentConnection => socketsSystemScript.AssembledCircuit;

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
        if ((currentConnection == CircuitType.Reverse && Voltage > 0) 
            || (currentConnection == CircuitType.Forward && Voltage < 0))
            Voltage = 0.0;
    }
}
