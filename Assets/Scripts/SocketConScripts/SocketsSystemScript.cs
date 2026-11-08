using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CircuitType
{
    None,
    Forward,
    Reverse
}

public class SocketsSystemScript : MonoBehaviour
{
    // Dictionary: socketID -> list of connectable socket IDs
    Dictionary<int, List<int>> socketDictionary = new();
    // Set of already created connections (normalized pairs)
    HashSet<(int, int)> createdConnections = new();
    // Array of all socket GameObjects indexed by SocketID
    GameObject[] sockets;
    // Temporary list of sockets currently being matched
    List<GameObject> matchingSockets;
    public bool IsAllMatched = false;

    public CircuitType AssembledCircuit { get; private set; } = CircuitType.None;

    [Tooltip("Логические условия проверки.\n" +
             "Формат: группы разделяются ';', варианты внутри группы '|', соединения внутри варианта '&'\n" +
             "Пример: (5-6 & 6-7) | (5-7 & 6-7) ; 1-2 ; 3-4")]
    [TextArea(5, 10)]
    [SerializeField]
    private string logicalConditionsText;
    // Parsed logical conditions structure: AND groups containing OR variants
    private List<List<HashSet<(int, int)>>> logicalConditions = new();

    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject wirePrefab;
    [SerializeField] private Material wireMaterial;
    private float wireOffset = 0.25f; // Curve offset for bezier wire
    private float wireThickness = 0.004f;

    [Header("Условия сборки")]

    [TextArea(3, 6)]
    [SerializeField]
    private string forwardConditionsText =
    "(3-4 & 4-7) | (3-7 & 4-7) | (4-3 & 3-7);" +
    "(5-6 & 5-8) | (5-8 & 6-8) | (5-6 & 6-8);" +
    "1-2";

    [TextArea(3, 6)]
    [SerializeField]
    private string reverseConditionsText =
        "(8-13 & 13-7) | (8-13 & 8-7) | (8-7 & 13-7);" +
        "(5-6 & 5-1) | (5-1 & 6-1) | (5-6 & 6-1);" +
        "14-4;" +
        "9-10;" +
        "11-12";

    // A single complete valid circuit configuration.
    private sealed class CircuitVariant
    {
        public CircuitType Type { get; }
        public HashSet<(int, int)> Connections { get; }

        public CircuitVariant(
            CircuitType type,
            HashSet<(int, int)> connections)
        {
            Type = type;
            Connections = connections;
        }
    }

    // All valid complete configurations for both circuits.
    private readonly List<CircuitVariant> circuitVariants = new();


    private void Start()
    {
        ParseLogicalConditions();
        ExtractConnectionsFromLogicalConditions();

        var allSockets = FindObjectsOfType<SocketScript>();

        sockets = new GameObject[17]; // Fixed size array for socket lookup
        matchingSockets = new List<GameObject>();

        // Organize sockets by ID for quick lookup
        foreach (var socket in allSockets)
        {
            sockets[socket.SocketID] = socket.gameObject;
        }
    }

    private void Update()
    {
        // Try to match any pending socket selections
        MatchSockets();
        // Update completion status
        IsAllMatched = CheckLogicalConditionsMet();
    }

    #region Logical Conditions Parsing & Checking

    /// <summary>
    /// Extracts all valid connections from logicalConditionsText and adds them to socketDictionary
    /// </summary>
    private void ExtractConnectionsFromLogicalConditions()
    {
        socketDictionary.Clear();

        foreach (var variant in circuitVariants)
        {
            foreach (var connection in variant.Connections)
            {
                AddLink(connection.Item1, connection.Item2);
                AddLink(connection.Item2, connection.Item1);
            }
        }
    }

    /// <summary>
    /// Parses the logical conditions text into structured data
    /// </summary>
    private void ParseLogicalConditions()
    {
        circuitVariants.Clear();

        AddCircuitVariants(
            forwardConditionsText,
            CircuitType.Forward);

        AddCircuitVariants(
            reverseConditionsText,
            CircuitType.Reverse);
    }

    /// <summary>
    /// Converts condition groups into complete circuit configurations.
    /// Supports the same format as the original script:
    /// ';' separates required groups,
    /// '|' separates alternatives,
    /// '&' separates connections within an alternative.
    /// Parentheses are used only for formatting.
    /// </summary>
    private void AddCircuitVariants(string text, CircuitType type)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var builds = new List<HashSet<(int, int)>>
    {
        new HashSet<(int, int)>()
    };

        foreach (string groupText in text.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(groupText))
                continue;

            var groupVariants = new List<HashSet<(int, int)>>();

            foreach (string variantText in groupText.Split('|'))
            {
                string cleanText = variantText
                    .Replace("(", "")
                    .Replace(")", "")
                    .Trim();

                var connections = new HashSet<(int, int)>();

                foreach (string pairText in cleanText.Split('&'))
                {
                    string[] parts = pairText.Trim().Split('-');

                    if (parts.Length != 2 ||
                        !int.TryParse(parts[0].Trim(), out int a) ||
                        !int.TryParse(parts[1].Trim(), out int b) ||
                        a < 0 ||
                        b < 0 ||
                        a == b)
                    {
                        // Do not register a circuit with invalid conditions.
                        return;
                    }

                    connections.Add(NormalizePair(a, b));
                }

                groupVariants.Add(connections);
            }

            // Combine the previously processed groups with the current group.
            var expandedBuilds = new List<HashSet<(int, int)>>();

            foreach (var build in builds)
            {
                foreach (var groupVariant in groupVariants)
                {
                    var expanded = new HashSet<(int, int)>(build);
                    expanded.UnionWith(groupVariant);

                    expandedBuilds.Add(expanded);
                }
            }

            builds = expandedBuilds;
        }

        foreach (var build in builds)
        {
            if (build.Count > 0)
            {
                circuitVariants.Add(
                    new CircuitVariant(type, build));
            }
        }
    }

    /// <summary>
    /// Checks if a connection is allowed based on logical conditions.
    /// Returns false if:
    /// 1. The group is already satisfied (no extra connections allowed)
    /// 2. The connection would make all variants in a group impossible
    /// </summary>
    private bool IsConnectionAllowed((int, int) connectionToAdd)
    {
        connectionToAdd = NormalizePair(
            connectionToAdd.Item1,
            connectionToAdd.Item2);

        if (connectionToAdd.Item1 == connectionToAdd.Item2 ||
            createdConnections.Contains(connectionToAdd))
        {
            return false;
        }

        var simulatedConnections =
            new HashSet<(int, int)>(createdConnections);

        simulatedConnections.Add(connectionToAdd);

        return circuitVariants.Any(variant =>
            simulatedConnections.IsSubsetOf(variant.Connections));
    }

    /// <summary>
    /// Checks if all logical conditions are satisfied
    /// </summary>
    private bool CheckLogicalConditionsMet()
    {
        AssembledCircuit = CircuitType.None;

        foreach (var variant in circuitVariants)
        {
            if (createdConnections.SetEquals(variant.Connections))
            {
                AssembledCircuit = variant.Type;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Automatically creates all valid connections from the socket dictionary
    /// </summary>
    // Assign to the first UI button.
    public void AutoCreateForwardConnections()
    {
        AutoCreateCircuit(CircuitType.Forward);
    }

    // Assign to the second UI button.
    public void AutoCreateReverseConnections()
    {
        AutoCreateCircuit(CircuitType.Reverse);
    }

    private void AutoCreateCircuit(CircuitType type)
    {
        if (sockets == null || matchingSockets == null)
        {
            return;
        }

        if (connectorPrefab == null || wirePrefab == null)
        {
            return;
        }

        // The prefab must have an available socket
        // to allow connectors to be stacked.
        if (connectorPrefab.GetComponentInChildren<SocketScript>() == null)
        {
            return;
        }

        // Read the conditions again from the current Inspector fields.
        ParseLogicalConditions();
        ExtractConnectionsFromLogicalConditions();

        // Select one complete configuration for the requested circuit type.
        CircuitVariant variant = circuitVariants
            .FirstOrDefault(v => v.Type == type);

        if (variant == null)
        {
            return;
        }

        // Check that all required panel sockets exist before resetting the circuit.
        foreach (var connection in variant.Connections)
        {
            int a = connection.Item1;
            int b = connection.Item2;

            if (a < 0 || a >= sockets.Length ||
                b < 0 || b >= sockets.Length ||
                sockets[a] == null || sockets[b] == null)
            {
                return;
            }
        }

        ResetAllConnections();

        // ID -> physical socket for the next connector.
        // After each insertion, replace it with the socket
        // inside the newly created connector.
        var nextSockets = new Dictionary<int, GameObject>();

        // Use a fixed order to make automatic assembly repeatable.
        var orderedConnections = variant.Connections
            .OrderBy(c => c.Item1)
            .ThenBy(c => c.Item2);

        foreach (var connection in orderedConnections)
        {
            int a = connection.Item1;
            int b = connection.Item2;

            GameObject startSocket =
                nextSockets.TryGetValue(a, out var nextA)
                    ? nextA
                    : sockets[a];

            GameObject endSocket =
                nextSockets.TryGetValue(b, out var nextB)
                    ? nextB
                    : sockets[b];

            var (startConnector, endConnector) =
                CreateWireConnection(startSocket, endSocket);

            SocketScript startNext =
                startConnector.GetComponentInChildren<SocketScript>();

            SocketScript endNext =
                endConnector.GetComponentInChildren<SocketScript>();

            if (startNext == null || endNext == null)
            {
                ResetAllConnections();
                return;
            }

            createdConnections.Add(NormalizePair(a, b));

            // Mark the used physical sockets as matched.
            startSocket.GetComponent<SocketScript>().ApplyMatching();
            endSocket.GetComponent<SocketScript>().ApplyMatching();

            // Also mark the main panel sockets as matched.
            sockets[a].GetComponent<SocketScript>().ApplyMatching();
            sockets[b].GetComponent<SocketScript>().ApplyMatching();

            // Connect the next wire with this ID
            // to the socket on top of the installed connector.
            nextSockets[a] = startNext.gameObject;
            nextSockets[b] = endNext.gameObject;
        }

        IsAllMatched = CheckLogicalConditionsMet();
    }

    /// <summary>
    /// Registers a socket for potential matching
    /// </summary>
    public bool RegisterSocket(GameObject socket)
    {
        if (socket == null || matchingSockets == null || sockets == null)
            return false;

        var socketScript = socket.GetComponent<SocketScript>();

        if (socketScript == null)
            return false;

        int id = socketScript.SocketID;

        if (id < 0 || id >= sockets.Length || sockets[id] == null)
            return false;

        if (matchingSockets.Count >= 2)
            return false;

        // Two physical objects representing the same logical socket
        // cannot be selected for a connection to each other.
        foreach (var selected in matchingSockets)
        {
            if (selected == null)
                continue;

            if (selected.GetComponent<SocketScript>().SocketID == id)
                return false;
        }

        // Store the actual selected object, not just its ID.
        matchingSockets.Add(socket);
        return true;
    }

    /// <summary>
    /// Resets all connections and socket states
    /// </summary>
    public void ResetAllConnections()
    {
        createdConnections.Clear();

        // Remove all wire visualizations
        var wires = FindObjectsOfType<LineRenderer>()
            .Where(lr => lr.gameObject.name.StartsWith("Wire:"))
            .ToList();

        foreach (var wire in wires)
            Destroy(wire.gameObject);

        // Remove all connector objects
        var connectors = FindObjectsOfType<Transform>()
            .Where(t => t.name == "WireConnector")
            .ToList();

        foreach (var connector in connectors)
            Destroy(connector.gameObject);

        // Reset all sockets in the scene
        var allSocketScripts = FindObjectsOfType<SocketScript>();
        foreach (var socketScript in allSocketScripts)
        {
            socketScript.ResetSocket();

            var outline = socketScript.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.StopOutline();
        }

        matchingSockets.Clear();
        IsAllMatched = false;
        AssembledCircuit = CircuitType.None;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Normalizes a pair so smaller ID is always first
    /// </summary>
    private (int, int) NormalizePair(int first, int second)
    {
        return first < second ? (first, second) : (second, first);
    }

    /// <summary>
    /// Checks if all sockets in the dictionary are matched
    /// </summary>
    private bool CheckAllConnectionsMatched()
    {
        foreach (var pair in socketDictionary)
        {
            int socketID = pair.Key;
            if (sockets[socketID] == null) return false;

            var socketScript = sockets[socketID].GetComponent<SocketScript>();
            if (!socketScript.IsMatched) return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to match two pending sockets
    /// </summary>
    private void MatchSockets()
    {
        if (matchingSockets.Count < 2)
            return;

        GameObject startSocket = matchingSockets[0];
        GameObject endSocket = matchingSockets[1];

        matchingSockets.Clear();

        if (startSocket == null || endSocket == null)
        {
            if (startSocket != null)
            {
                startSocket.GetComponent<SocketScript>().CancelSelection();
                startSocket.GetComponentInChildren<Outline>()?.StopOutline();
            }

            if (endSocket != null)
            {
                endSocket.GetComponent<SocketScript>().CancelSelection();
                endSocket.GetComponentInChildren<Outline>()?.StopOutline();
            }

            return;
        }

        var startScript = startSocket.GetComponent<SocketScript>();
        var endScript = endSocket.GetComponent<SocketScript>();

        // Get the outlines BEFORE creating any new child objects.
        Outline startOutline = startSocket.GetComponentInChildren<Outline>();
        Outline endOutline = endSocket.GetComponentInChildren<Outline>();

        int idA = startScript.SocketID;
        int idB = endScript.SocketID;

        var pairKey = NormalizePair(idA, idB);

        bool isValidConnection =
            socketDictionary.TryGetValue(idA, out var allowedIDs) &&
            allowedIDs.Contains(idB);

        bool canConnect =
            idA != idB &&
            !createdConnections.Contains(pairKey) &&
            isValidConnection &&
            IsConnectionAllowed(pairKey);

        if (canConnect)
        {
            // Use the selected physical sockets as insertion points.
            CreateWireConnection(startSocket, endSocket);
            createdConnections.Add(pairKey);

            startOutline?.StopOutline();
            endOutline?.StopOutline();

            startScript.ApplyMatching();
            endScript.ApplyMatching();

            // Also mark the main panel sockets as matched.
            sockets[idA].GetComponent<SocketScript>().ApplyMatching();
            sockets[idB].GetComponent<SocketScript>().ApplyMatching();
        }
        else
        {
            // Cancel only the selection.
            // Existing connections remain valid.
            startScript.CancelSelection();
            endScript.CancelSelection();

            startOutline?.FlashRed();
            endOutline?.FlashRed();
        }

        IsAllMatched = CheckLogicalConditionsMet();
    }

    /// <summary>
    /// Adds a connection link to the dictionary
    /// </summary>
    private void AddLink(int from, int to)
    {
        if (!socketDictionary.TryGetValue(from, out var list))
        {
            list = new List<int>();
            socketDictionary[from] = list;
        }

        if (!list.Contains(to)) list.Add(to);
    }

    /// <summary>
    /// Creates a complete wire connection between two sockets
    /// </summary>
    private (GameObject startConnector, GameObject endConnector)
    CreateWireConnection(GameObject startSocket, GameObject endSocket)
    {
        GameObject startConnector = CreateConnector(startSocket);
        GameObject endConnector = CreateConnector(endSocket);

        SetupConnectorComponents(startConnector, startSocket);
        SetupConnectorComponents(endConnector, endSocket);

        GameObject wire = CreateWireObject(startSocket, endSocket);
        StartWireRender(wire, startConnector, endConnector);

        return (startConnector, endConnector);
    }

    /// <summary>
    /// Creates a connector object at socket position with proper orientation
    /// </summary>
    private GameObject CreateConnector(GameObject socket)
    {
        SocketScript ss = socket.GetComponent<SocketScript>();

        GameObject connector = Instantiate(connectorPrefab, socket.transform.position, Quaternion.identity);
        connector.name = "WireConnector";

        Vector3 directionWorld = socket.transform.TransformDirection(ss.direction.normalized);
        connector.transform.rotation = Quaternion.LookRotation(directionWorld, Vector3.up);
        connector.transform.localScale = connectorPrefab.transform.lossyScale;
        connector.transform.SetParent(socket.transform, true);

        return connector;
    }

    /// <summary>
    /// Configures connector components to match parent socket
    /// </summary>
    private void SetupConnectorComponents(GameObject connector, GameObject parentSocket)
    {
        SocketScript childrenSocket = connector.GetComponentInChildren<SocketScript>();
        if (childrenSocket != null)
        {
            childrenSocket.SocketID = parentSocket.GetComponent<SocketScript>().SocketID;
        }
    }

    /// <summary>
    /// Creates a wire GameObject with LineRenderer
    /// </summary>
    private GameObject CreateWireObject(GameObject startSocket, GameObject endSocket)
    {
        GameObject wire = Instantiate(wirePrefab, startSocket.transform);

        var startID = startSocket.GetComponent<SocketScript>().SocketID;
        var endID = endSocket.GetComponent<SocketScript>().SocketID;
        wire.name = $"Wire:{startID}-{endID}";

        LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();
        if (wireMaterial) lineRenderer.material = wireMaterial;
        lineRenderer.startWidth = wireThickness;
        lineRenderer.endWidth = wireThickness;

        return wire;
    }

    /// <summary>
    /// Configures the LineRenderer to draw a bezier curve between connectors
    /// </summary>
    private void StartWireRender(GameObject wire, GameObject startConnector, GameObject endConnector)
    {
        LineRenderer lineRenderer = wire.GetComponent<LineRenderer>();

        Vector3 wireExitStartPoint = startConnector.transform.Find("WireIn")?.position ?? startConnector.transform.position;
        Vector3 wireExitEndPoint = endConnector.transform.Find("WireIn")?.position ?? endConnector.transform.position;

        Vector3[] bezierPoints = CalculateBezierPoints(
            wireExitStartPoint,
            wireExitEndPoint,
            startConnector.transform.forward,
            endConnector.transform.forward,
            wireOffset);

        lineRenderer.positionCount = bezierPoints.Length;
        lineRenderer.SetPositions(bezierPoints);
    }

    /// <summary>
    /// Calculates bezier curve points for wire visualization
    /// </summary>
    private Vector3[] CalculateBezierPoints(Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, float offset)
    {
        Vector3 p0 = startPos;
        Vector3 p3 = endPos;
        Vector3 p1 = p0 + startDir * offset;
        Vector3 p2 = p3 + endDir * offset;

        const int SEGMENT_COUNT = 30;
        Vector3[] points = new Vector3[SEGMENT_COUNT];

        for (int i = 0; i < SEGMENT_COUNT; i++)
        {
            float t = i / (SEGMENT_COUNT - 1f);
            points[i] = CalculateBezierPoint(t, p0, p1, p2, p3);
        }

        return points;
    }

    /// <summary>
    /// Calculates a single point on a cubic bezier curve
    /// </summary>
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        return (u * u * u) * p0
             + 3 * (u * u) * t * p1
             + 3 * u * (t * t) * p2
             + (t * t * t) * p3;
    }

    #endregion
}
