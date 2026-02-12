using UnityEngine;
using System.Collections.Generic;

public class DrippingEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("When enabled, drips will reset and loop continuously. When disabled, drips will stop when they reach the bottom.")]
    [SerializeField] private bool loopAnimation = true;

    [Header("Drip Line Settings")]
    [Tooltip("Number of individual drip lines that fall down the screen. More lines = smoother curve but more performance cost.")]
    [SerializeField] private int numberOfLines = 15;

    [Tooltip("Minimum speed at which drip lines will fall down the screen.")]
    [SerializeField] private float minSpeed = 5f;

    [Tooltip("Maximum speed at which drip lines will fall down the screen.")]
    [SerializeField] private float maxSpeed = 10f;

    [Tooltip("Time (in seconds) for drips to accelerate from 0 to their target speed. Higher values = slower, smoother acceleration.")]
    [SerializeField] private float accelerationTime = 2f;

    [Tooltip("Color of the individual drip lines and connecting curve.")]
    [SerializeField] private Color lineColor = Color.red;

    [Header("Screen Settings")]
    [Tooltip("Padding from screen edges where drip lines will start/stop. Prevents drips from appearing exactly at screen borders.")]
    [SerializeField] private float screenPadding = 0.1f;

    [Tooltip("How far past the bottom of the screen drips will continue falling before stopping. 0 = stop exactly at screen bottom.")]
    [SerializeField] private float dripPastScreen = 2f;

    [Header("Drip Delay Settings")]
    [Tooltip("Maximum time (in seconds) a drip can wait before starting to fall. 0 = no delay.")]
    [SerializeField] private float maxDripDelay = 2f;

    [Tooltip("Chance (0-1) that a drip will use delay time. 0 = no drips delay, 1 = all drips delay, 0.5 = half of drips delay.")]
    [SerializeField][Range(0f, 1f)] private float delayChance = 0.3f;

    [Header("Connecting Line Settings")]
    [Tooltip("Width/thickness of the connecting curve that forms the dripping shape.")]
    [SerializeField] private float connectingLineWidth = 1f;

    [Tooltip("Number of segments between each drip point for smoothing the curve. Higher values = smoother curve but more vertices.")]
    [SerializeField] private int connectingLineSmoothness = 15;

    [Tooltip("Tension of the curve interpolation. 0.5 = centripetal Catmull-Rom (recommended), 0 = uniform, 1 = chordal.")]
    [SerializeField] private float curveTension = 0.5f;

    [Tooltip("Custom material for the connecting curve. If not set, uses default sprite material.")]
    [SerializeField] private Material connectingLineMaterial;

    [Header("Render Settings")]
    [Tooltip("Sorting layer name for the dripping effect. Use this to control render order relative to other objects.")]
    [SerializeField] private string sortingLayerName = "Default";

    [Tooltip("Order in layer for the dripping effect. Higher values render on top of lower values.")]
    [SerializeField] private int orderInLayer = 10;

    [Header("Fill Settings")]
    [Tooltip("When enabled, fills the area between the connecting curve and top of screen with a transparent mesh.")]
    [SerializeField] private bool fillTopArea = true;

    [Tooltip("Opacity/transparency of the fill area (0 = fully transparent, 1 = fully opaque).")]
    [SerializeField] private float fillOpacity = 1f;

    [Tooltip("Controls how much the fill fades near the bottom curve. 0 = no fade, 1 = full fade to transparent at curve.")]
    [SerializeField][Range(0f, 1f)] private float bottomEdgeFade = 0.7f;

    [Tooltip("Distance from the bottom curve where the fade effect reaches full strength.")]
    [SerializeField] private float edgeFadeDistance = 1.5f;

    private List<FallingLine> fallingLines = new List<FallingLine>();
    private LineRenderer connectingLineRenderer;
    private MeshRenderer fillMeshRenderer;
    private MeshFilter fillMeshFilter;
    private Mesh fillMesh;
    private List<Vector3> currentDripPoints = new List<Vector3>();

    // Track animation state
    private bool animationActive = true;
    private int linesAtBottom = 0;
    private float bottomThreshold;
    private float topStartY;
    private float screenBottom;
    private float camWidth;
    private float camHeight;
    private float previousTopStartY;

    void Start()
    {
        // Calculate screen boundaries first
        CalculateScreenBoundaries();
        previousTopStartY = topStartY;

        GenerateFallingLines();
        SetupConnectingLine();
        if (fillTopArea) SetupFillMesh();
    }

    void CalculateScreenBoundaries()
    {
        float previousTopStartY = this.topStartY;

        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
        screenBottom = -camHeight; // Camera-relative bottom of screen
        bottomThreshold = screenBottom - dripPastScreen; // Bottom threshold including drip past screen
        topStartY = camHeight - screenPadding; // Camera-relative top starting position

        // If camera moved (orthographicSize changed), adjust existing lines
        if (fallingLines != null && fallingLines.Count > 0 && Mathf.Abs(this.previousTopStartY - topStartY) > 0.001f)
        {
            float yOffset = topStartY - this.previousTopStartY;

            foreach (FallingLine line in fallingLines)
            {
                line.startY += yOffset;
                line.currentY += yOffset;

                // Update bottom status based on new threshold
                if (line.currentY <= bottomThreshold && !line.isAtBottom)
                {
                    line.isAtBottom = true;
                    line.currentY = bottomThreshold;
                }
                else if (line.currentY > bottomThreshold && line.isAtBottom)
                {
                    line.isAtBottom = false;
                }

                UpdateLinePositions(line);
            }
        }

        this.previousTopStartY = topStartY;
    }

    void Update()
    {
        if (!animationActive) return;

        // Recalculate screen boundaries each frame to track camera movement
        CalculateScreenBoundaries();

        UpdateFallingLines();
        UpdateConnectingLine();
    }

    void GenerateFallingLines()
    {
        float startX = -camWidth + screenPadding;
        float endX = camWidth - screenPadding;
        float spacing = (endX - startX) / (numberOfLines - 1);

        for (int i = 0; i < numberOfLines; i++)
        {
            float xPos = startX + i * spacing;
            float targetSpeed = Random.Range(minSpeed, maxSpeed);
            float width = 0.05f; // Fixed small width for visual reference only

            if (i == 0) xPos = -camWidth - width / 2f;
            if (i == numberOfLines - 1) xPos = camWidth + width / 2f;

            GameObject lineObj = new GameObject($"FallingLine_{i}");
            lineObj.transform.parent = transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.useWorldSpace = true;

            // Set sorting layer and order
            lr.sortingLayerName = sortingLayerName;
            lr.sortingOrder = orderInLayer;

            // Make drips completely transparent by default
            Color transparent = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            lr.startColor = transparent;
            lr.endColor = transparent;

            // Determine if this drip should have a delay
            bool hasDelay = maxDripDelay > 0 && Random.value <= delayChance;
            float delayTime = hasDelay ? Random.Range(0f, maxDripDelay) : 0f;
            float delayTimer = delayTime;

            FallingLine fl = new FallingLine
            {
                renderer = lr,
                targetSpeed = targetSpeed,
                currentSpeed = 0f,
                accelerationTime = accelerationTime,
                accelerationTimer = 0f,
                startY = topStartY,
                currentY = topStartY,
                xPosition = xPos,
                isAtBottom = false,
                hasDelay = hasDelay,
                delayTime = delayTime,
                delayTimer = delayTimer,
                isDelaying = hasDelay && delayTime > 0f,
                shouldMove = !hasDelay || delayTime <= 0f,
                isAccelerating = false
            };

            // Start accelerating immediately if no delay
            if (!hasDelay || delayTime <= 0f)
            {
                fl.isAccelerating = true;
                fl.accelerationTimer = 0f;
            }

            fallingLines.Add(fl);
            UpdateLinePositions(fl);
        }
    }

    void UpdateFallingLines()
    {
        int newLinesAtBottom = 0;
        int activeLines = 0;

        foreach (FallingLine line in fallingLines)
        {
            // Skip lines that are already at bottom in non-looping mode
            if (line.isAtBottom && !loopAnimation)
            {
                continue;
            }

            // Handle delay timer for lines that are waiting to start
            if (line.isDelaying)
            {
                line.delayTimer -= Time.deltaTime;
                if (line.delayTimer <= 0f)
                {
                    line.isDelaying = false;
                    line.delayTimer = 0f;
                    line.shouldMove = true;
                    line.isAccelerating = true;
                    line.accelerationTimer = 0f;

                    activeLines++;
                }
                else
                {
                    activeLines++;
                    continue;
                }
            }

            // Handle acceleration for lines that are accelerating
            if (line.isAccelerating)
            {
                line.accelerationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(line.accelerationTimer / line.accelerationTime);
                t = 1f - Mathf.Pow(1f - t, 3f);
                line.currentSpeed = Mathf.Lerp(0f, line.targetSpeed, t);

                if (line.accelerationTimer >= line.accelerationTime)
                {
                    line.isAccelerating = false;
                    line.currentSpeed = line.targetSpeed;
                }

                activeLines++;
            }

            // Only move lines that should move and haven't reached bottom
            if (line.shouldMove && !line.isAtBottom)
            {
                line.currentY -= line.currentSpeed * Time.deltaTime;

                if (line.currentY <= bottomThreshold)
                {
                    line.currentY = bottomThreshold;
                    line.isAtBottom = true;
                    newLinesAtBottom++;
                    line.isAccelerating = false;
                    line.currentSpeed = 0f;
                }
                else
                {
                    activeLines++;
                }
            }
            else if (line.isAtBottom)
            {
                newLinesAtBottom++;
            }
            else if (!line.shouldMove && !line.isDelaying)
            {
                Debug.LogWarning($"Line at {line.xPosition} is stuck! Resetting...");
                line.shouldMove = true;
                line.isAccelerating = true;
                line.accelerationTimer = 0f;
                activeLines++;
            }

            UpdateLinePositions(line);
        }

        // Update counter of lines at bottom
        linesAtBottom = newLinesAtBottom;

        // Handle loop or stop logic
        if (loopAnimation)
        {
            if (linesAtBottom >= fallingLines.Count && activeLines == 0)
            {
                ResetAllLinesToTop();
                linesAtBottom = 0;
            }
        }
        else
        {
            if (linesAtBottom >= fallingLines.Count && activeLines == 0)
            {
                animationActive = false;
                if (connectingLineRenderer != null)
                    connectingLineRenderer.positionCount = 0;
                if (fillMesh != null)
                    ClearFillMesh();
            }
        }
    }

    void ResetAllLinesToTop()
    {
        CalculateScreenBoundaries(); // Recalculate in case camera moved or dripPastScreen changed

        foreach (FallingLine line in fallingLines)
        {
            line.currentY = topStartY;
            line.isAtBottom = false;
            line.currentSpeed = 0f;
            line.accelerationTimer = 0f;
            line.isAccelerating = false;

            // RE-ROLL ALL RANDOM VALUES: speed and delay

            // 1. Re-roll the speed
            line.targetSpeed = Random.Range(minSpeed, maxSpeed);

            // 2. RE-ROLL whether this line should have a delay based on current settings
            bool shouldHaveDelay = maxDripDelay > 0 && Random.value <= delayChance;

            if (shouldHaveDelay)
            {
                line.hasDelay = true;
                line.delayTime = Random.Range(0f, maxDripDelay);
                line.delayTimer = line.delayTime;
                line.isDelaying = line.delayTime > 0f;
                line.shouldMove = line.delayTime <= 0f;
                line.isAccelerating = false;

                // If no delay or delay is 0, start accelerating immediately
                if (line.delayTime <= 0f)
                {
                    line.isAccelerating = true;
                    line.accelerationTimer = 0f;
                }
            }
            else
            {
                line.hasDelay = false;
                line.delayTime = 0f;
                line.delayTimer = 0f;
                line.isDelaying = false;
                line.shouldMove = true;
                line.isAccelerating = true; // Start accelerating immediately
                line.accelerationTimer = 0f;
            }

            UpdateLinePositions(line);
        }
    }

    void UpdateLinePositions(FallingLine line)
    {
        if (line.renderer != null)
        {
            // Always set positions - delayed drips exist at the top
            line.renderer.SetPosition(0, new Vector3(line.xPosition, line.startY, 0));
            line.renderer.SetPosition(1, new Vector3(line.xPosition, line.currentY, 0));

            // Keep all drips completely transparent
            Color transparent = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            line.renderer.startColor = transparent;
            line.renderer.endColor = transparent;
        }
    }

    void SetupConnectingLine()
    {
        GameObject obj = new GameObject("ConnectingLine");
        obj.transform.parent = transform;

        connectingLineRenderer = obj.AddComponent<LineRenderer>();
        connectingLineRenderer.startWidth = connectingLineWidth;
        connectingLineRenderer.endWidth = connectingLineWidth;
        connectingLineRenderer.material = connectingLineMaterial != null
            ? connectingLineMaterial
            : new Material(Shader.Find("Sprites/Default"));
        connectingLineRenderer.useWorldSpace = true;

        // Set sorting layer and order
        connectingLineRenderer.sortingLayerName = sortingLayerName;
        connectingLineRenderer.sortingOrder = orderInLayer;

        // Make connecting line use the line color with full opacity
        connectingLineRenderer.startColor = lineColor;
        connectingLineRenderer.endColor = lineColor;

        // Initialize with empty positions to avoid errors
        connectingLineRenderer.positionCount = 0;
    }

    void UpdateConnectingLine()
    {
        currentDripPoints.Clear();

        // Include ALL drips in the curve
        // For non-looping mode: include bottom drips at their bottom position
        // For looping mode: include all drips (they reset when all reach bottom)
        foreach (FallingLine line in fallingLines)
        {
            // Always include the drip - if it's at bottom, use bottom position
            currentDripPoints.Add(new Vector3(line.xPosition, line.currentY, 0));
        }

        // If no points or not enough points, hide the connecting line
        if (currentDripPoints.Count < 2)
        {
            if (connectingLineRenderer != null)
                connectingLineRenderer.positionCount = 0;
            if (fillTopArea && fillMesh != null)
                ClearFillMesh();
            return;
        }

        currentDripPoints.Sort((a, b) => a.x.CompareTo(b.x));

        Vector3 leftEdgePoint = new Vector3(-camWidth, currentDripPoints[0].y, 0);
        currentDripPoints.Insert(0, leftEdgePoint);

        Vector3 rightEdgePoint = new Vector3(camWidth, currentDripPoints[currentDripPoints.Count - 1].y, 0);
        currentDripPoints.Add(rightEdgePoint);

        List<Vector3> curvePoints = GenerateSmoothCurve(currentDripPoints);
        curvePoints = RoundCurveEnds(curvePoints);

        // Ensure curve doesn't go below drip threshold
        for (int i = 0; i < curvePoints.Count; i++)
        {
            if (curvePoints[i].y < bottomThreshold)
            {
                curvePoints[i] = new Vector3(curvePoints[i].x, bottomThreshold, curvePoints[i].z);
            }
        }

        // Check if we have enough points for a valid line
        if (curvePoints.Count > 1)
        {
            connectingLineRenderer.positionCount = curvePoints.Count;
            connectingLineRenderer.SetPositions(curvePoints.ToArray());
        }
        else
        {
            connectingLineRenderer.positionCount = 0;
        }

        if (fillTopArea)
            UpdateFillMesh(curvePoints);
    }

    List<Vector3> GenerateSmoothCurve(List<Vector3> points)
    {
        if (points.Count < 2) return new List<Vector3>();

        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? points[i - 1] : points[i];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = i < points.Count - 2 ? points[i + 2] : points[i + 1];

            // Validate points to prevent NaN/infinite values
            if (!IsValidVector(p0) || !IsValidVector(p1) || !IsValidVector(p2) || !IsValidVector(p3))
                continue;

            for (int j = 0; j <= connectingLineSmoothness; j++)
            {
                float t = j / (float)connectingLineSmoothness;
                Vector3 p = CalculateCatmullRomPoint(t, p0, p1, p2, p3, curveTension);

                // Validate the calculated point
                if (!IsValidVector(p))
                    continue;

                // Only add wave effect if not at bottom threshold
                if (p.y > bottomThreshold + 0.1f)
                {
                    float wave = Mathf.Sin(p.x * 2f + Time.time) * 0.1f;
                    p.y += wave;
                }

                result.Add(p);
            }
        }

        if (result.Count > 0 && IsValidVector(points[points.Count - 1]))
        {
            result.Add(points[points.Count - 1]);
        }

        return result;
    }

    bool IsValidVector(Vector3 v)
    {
        return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) &&
               !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }

    Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float alpha)
    {
        // Clamp alpha to prevent extreme values
        alpha = Mathf.Clamp(alpha, 0.001f, 1f);

        float t0 = 0f;
        float t1 = GetT(t0, p0, p1, alpha);
        float t2 = GetT(t1, p1, p2, alpha);
        float t3 = GetT(t2, p2, p3, alpha);

        t = Mathf.Lerp(t1, t2, t);

        Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
        Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
        Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

        Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
        Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

        return (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;
    }

    float GetT(float t, Vector3 p0, Vector3 p1, float alpha)
    {
        float distance = Vector3.Distance(p0, p1);
        // Clamp distance to prevent extreme values
        distance = Mathf.Clamp(distance, 0.001f, 100f);
        return t + Mathf.Pow(distance, Mathf.Clamp(alpha, 0.001f, 1f));
    }

    List<Vector3> RoundCurveEnds(List<Vector3> points)
    {
        if (points.Count < 3) return points;

        List<Vector3> p = new List<Vector3>(points);
        int count = Mathf.Min(10, p.Count / 4);

        for (int i = 0; i < count; i++)
        {
            float f = (float)i / count;
            p[i] = Vector3.Lerp(p[i], p[count], f * f);
            int r = p.Count - 1 - i;
            p[r] = Vector3.Lerp(p[r], p[p.Count - 1 - count], f * f);
        }

        return p;
    }

    void SetupFillMesh()
    {
        GameObject obj = new GameObject("FillMesh");
        obj.transform.parent = transform;

        fillMeshRenderer = obj.AddComponent<MeshRenderer>();
        fillMeshFilter = obj.AddComponent<MeshFilter>();

        fillMesh = new Mesh();
        fillMeshFilter.mesh = fillMesh;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(lineColor.r, lineColor.g, lineColor.b, fillOpacity);
        fillMeshRenderer.material = mat;

        // Set sorting layer and order
        fillMeshRenderer.sortingLayerName = sortingLayerName;
        fillMeshRenderer.sortingOrder = orderInLayer;

        // Initialize with empty mesh to avoid errors
        ClearFillMesh();
    }

    void UpdateFillMesh(List<Vector3> curvePoints)
    {
        if (curvePoints.Count < 2)
        {
            ClearFillMesh();
            return;
        }

        // Use camera-relative top (screen space)
        float topY = camHeight;

        // Validate all curve points
        for (int i = 0; i < curvePoints.Count; i++)
        {
            if (!IsValidVector(curvePoints[i]))
            {
                ClearFillMesh();
                return;
            }
        }

        Vector3[] verts = new Vector3[curvePoints.Count * 2];
        Color[] cols = new Color[verts.Length];

        for (int i = 0; i < curvePoints.Count; i++)
        {
            Vector3 curve = curvePoints[i];

            // Clamp values to reasonable ranges
            float x = Mathf.Clamp(curve.x, -camWidth * 2f, camWidth * 2f);
            float y = Mathf.Clamp(curve.y, screenBottom - 10f, topY + 10f);

            verts[i * 2] = new Vector3(x, y, 0);
            verts[i * 2 + 1] = new Vector3(x, topY, 0);

            float distanceFromEdge = verts[i * 2 + 1].y - verts[i * 2].y;
            float t = Mathf.Clamp01(distanceFromEdge / edgeFadeDistance);

            float alpha = Mathf.Lerp(
                fillOpacity,
                fillOpacity * (1f - bottomEdgeFade),
                t
            );

            cols[i * 2] = new Color(1, 1, 1, fillOpacity);
            cols[i * 2 + 1] = new Color(1, 1, 1, alpha);
        }

        int[] tris = new int[(curvePoints.Count - 1) * 6];
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            int v = i * 2;
            int t = i * 6;

            tris[t] = v;
            tris[t + 1] = v + 1;
            tris[t + 2] = v + 2;
            tris[t + 3] = v + 2;
            tris[t + 4] = v + 1;
            tris[t + 5] = v + 3;
        }

        fillMesh.Clear();
        fillMesh.vertices = verts;
        fillMesh.triangles = tris;
        fillMesh.colors = cols;

        // Manually set bounds to reasonable values
        Bounds bounds = new Bounds();
        bounds.center = new Vector3(0, (topY + bottomThreshold) / 2f, 0);
        bounds.size = new Vector3(camWidth * 2f, Mathf.Abs(topY - bottomThreshold), 0.1f);
        fillMesh.bounds = bounds;

        fillMesh.RecalculateNormals();
        fillMesh.RecalculateTangents();
    }

    void ClearFillMesh()
    {
        if (fillMesh != null)
        {
            fillMesh.Clear();
            // Create a small valid mesh
            fillMesh.vertices = new Vector3[] {
                new Vector3(-0.1f, -0.1f, 0),
                new Vector3(0.1f, -0.1f, 0),
                new Vector3(0, 0.1f, 0)
            };
            fillMesh.triangles = new int[] { 0, 1, 2 };
            fillMesh.colors = new Color[] { Color.clear, Color.clear, Color.clear };

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f));
            fillMesh.bounds = bounds;
        }
    }

    void OnDestroy()
    {
        // Clean up dynamically created meshes to prevent memory leaks
        if (fillMesh != null)
        {
            DestroyImmediate(fillMesh);
        }

        // Clean up line renderers
        foreach (FallingLine line in fallingLines)
        {
            if (line.renderer != null && line.renderer.gameObject != null)
            {
                DestroyImmediate(line.renderer.gameObject);
            }
        }

        if (connectingLineRenderer != null && connectingLineRenderer.gameObject != null)
        {
            DestroyImmediate(connectingLineRenderer.gameObject);
        }

        if (fillMeshRenderer != null && fillMeshRenderer.gameObject != null)
        {
            DestroyImmediate(fillMeshRenderer.gameObject);
        }
    }

    void OnDisable()
    {
        // Stop animation when component is disabled
        animationActive = false;
    }

    void OnEnable()
    {
        // Restart animation when component is enabled
        if (!animationActive && (loopAnimation || linesAtBottom < fallingLines.Count))
        {
            animationActive = true;
        }
    }

    // Public methods to control animation
    public void StartAnimation()
    {
        animationActive = true;
        ResetAllLines();
    }

    public void StopAnimation()
    {
        animationActive = false;
        // Clear visual elements when stopped
        if (connectingLineRenderer != null)
            connectingLineRenderer.positionCount = 0;
        if (fillMesh != null)
            ClearFillMesh();
    }

    public void ResetAllLines()
    {
        ResetAllLinesToTop();
        linesAtBottom = 0;
        animationActive = true;
    }

    public void SetLoopAnimation(bool loop)
    {
        loopAnimation = loop;

        // If switching to non-looping mode, reset all lines
        if (!loopAnimation)
        {
            ResetAllLines();
        }
    }

    // Method to update layer settings at runtime
    public void SetLayerSettings(string layerName, int order)
    {
        sortingLayerName = layerName;
        orderInLayer = order;

        // Update all renderers with new layer settings
        foreach (FallingLine line in fallingLines)
        {
            if (line.renderer != null)
            {
                line.renderer.sortingLayerName = sortingLayerName;
                line.renderer.sortingOrder = orderInLayer;
            }
        }

        if (connectingLineRenderer != null)
        {
            connectingLineRenderer.sortingLayerName = sortingLayerName;
            connectingLineRenderer.sortingOrder = orderInLayer;
        }

        if (fillMeshRenderer != null)
        {
            fillMeshRenderer.sortingLayerName = sortingLayerName;
            fillMeshRenderer.sortingOrder = orderInLayer;
        }
    }

    // Method to update dripPastScreen at runtime
    public void SetDripPastScreen(float value)
    {
        dripPastScreen = Mathf.Max(0, value); // Ensure it's not negative
        CalculateScreenBoundaries();

        // Update all lines' bottom status based on new threshold
        foreach (FallingLine line in fallingLines)
        {
            if (line.currentY <= bottomThreshold && !line.isAtBottom)
            {
                line.currentY = bottomThreshold;
                line.isAtBottom = true;
            }
            else if (line.currentY > bottomThreshold && line.isAtBottom)
            {
                line.isAtBottom = false;
            }
        }
    }

    // Method to update delay settings at runtime
    public void SetDelaySettings(float maxDelay, float chance)
    {
        maxDripDelay = Mathf.Max(0, maxDelay);
        delayChance = Mathf.Clamp01(chance);

        // Update existing lines with new delay settings
        foreach (FallingLine line in fallingLines)
        {
            // Determine if this drip should have a delay based on new chance
            bool shouldHaveDelay = maxDripDelay > 0 && Random.value <= delayChance;

            if (shouldHaveDelay)
            {
                line.hasDelay = true;
                line.delayTime = Random.Range(0f, maxDripDelay);
                line.delayTimer = line.delayTime;
                line.isDelaying = line.delayTime > 0f;
                line.shouldMove = line.delayTime <= 0f;
                line.isAccelerating = false;
                line.accelerationTimer = 0f;

                // If no delay or delay is 0, start accelerating immediately
                if (line.delayTime <= 0f)
                {
                    line.isAccelerating = true;
                    line.accelerationTimer = 0f;
                }
            }
            else
            {
                line.hasDelay = false;
                line.delayTime = 0f;
                line.delayTimer = 0f;
                line.isDelaying = false;
                line.shouldMove = true;
                line.isAccelerating = true; // Start accelerating immediately
                line.accelerationTimer = 0f;
            }

            // Update line appearance
            UpdateLinePositions(line);
        }
    }

    // Method to update acceleration time at runtime
    public void SetAccelerationTime(float time)
    {
        accelerationTime = Mathf.Max(0, time);

        // Update all lines with new acceleration time
        foreach (FallingLine line in fallingLines)
        {
            line.accelerationTime = accelerationTime;
        }
    }

    // Method to get current drip settings
    public float GetScreenBottom()
    {
        return screenBottom;
    }

    public float GetDripThreshold()
    {
        return bottomThreshold;
    }

    public float GetDripPastScreen()
    {
        return dripPastScreen;
    }

    public float GetMaxDripDelay()
    {
        return maxDripDelay;
    }

    public float GetDelayChance()
    {
        return delayChance;
    }

    public float GetAccelerationTime()
    {
        return accelerationTime;
    }

    public string GetSortingLayerName()
    {
        return sortingLayerName;
    }

    public int GetOrderInLayer()
    {
        return orderInLayer;
    }

    [System.Serializable]
    private class FallingLine
    {
        public LineRenderer renderer;
        public float targetSpeed;      // The final speed this drip should reach
        public float currentSpeed;     // Current speed (accelerating from 0 to targetSpeed)
        public float accelerationTime; // Time to accelerate from 0 to targetSpeed
        public float accelerationTimer;// Timer tracking acceleration progress
        public float startY;
        public float currentY;
        public float xPosition;
        public bool isAtBottom;
        public bool hasDelay;          // Whether this drip has a delay
        public float delayTime;        // Total delay time for this drip
        public float delayTimer;       // Current delay timer
        public bool isDelaying;        // Whether drip is currently delaying
        public bool shouldMove;        // Whether drip should actually move downward
        public bool isAccelerating;    // Whether drip is currently accelerating
    }
}