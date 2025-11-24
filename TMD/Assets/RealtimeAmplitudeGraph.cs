using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RawImage))]
public class RealtimeAmplitudeHistoryGraph : MonoBehaviour
{
    [Header("Graph Size (pixels)")]
    public int graphWidth = 400;
    public int graphHeight = 120;

    [Header("Appearance")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Color axisColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color lineColor = Color.green;

    [Header("Value Readout (optional)")]
    public TextMeshProUGUI currentXText; // shows time (x)
    public TextMeshProUGUI currentYText; // shows amplitude (y)

    [Header("Sampling")]
    [Tooltip("Seconds between samples.")]
    public float sampleInterval = 0.02f;

    [Header("Amplitude Range")]
    [Tooltip("Expected max absolute amplitude. For -1..1 use 1.")]
    public float maxAmplitude = 1f; // we’re targeting [-1, 1]

    private RawImage _rawImage;
    private Texture2D _texture;

    private readonly List<float> _values = new List<float>();
    private float _sampleTimer;

    private Color[] _pixels;

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _texture = new Texture2D(graphWidth, graphHeight, TextureFormat.RGBA32, false);
        _texture.wrapMode = TextureWrapMode.Clamp;
        _rawImage.texture = _texture;

        _pixels = new Color[graphWidth * graphHeight];
        ClearTexture();
    }

    void ClearTexture()
    {
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = backgroundColor;

        _texture.SetPixels(_pixels);
        _texture.Apply();
    }

    void Update()
    {
        // DEMO INPUT: clean sine between -1 and 1
        _sampleTimer += Time.deltaTime;
        if (_sampleTimer >= sampleInterval)
        {
            _sampleTimer -= sampleInterval;

            float t = Time.time;          // seconds since start
            float value = Mathf.Sin(t);   // in [-1, 1]

            //AddSample(value);
        }
    }

    /// <summary>
    /// Call this from your simulation instead of the demo sine.
    /// Value should be in [-maxAmplitude, maxAmplitude].
    /// </summary>
    public void AddSample(float value)
    {
        float absValue = Mathf.Abs(value);
        if (absValue > maxAmplitude)
            maxAmplitude = Mathf.Lerp(maxAmplitude, absValue, 0.1f);

        _values.Add(value);

        int index = _values.Count - 1;
        float time = index * sampleInterval;

        if (currentXText != null)
            currentXText.text = $"t = {time:F2} s";

        if (currentYText != null)
            currentYText.text = $"y = {value:F4}";
        Redraw();
    }

    void Redraw()
    {
        for (int i = 0; i < _pixels.Length; i++)
            _pixels[i] = backgroundColor;

        // Draw zero axis
        int midY = graphHeight / 2;
        for (int x = 0; x < graphWidth; x++)
            _pixels[midY * graphWidth + x] = axisColor;

        if (_values.Count > 1)
        {
            int lastX = 0;
            int lastY = ValueToY(_values[0]);

            for (int i = 1; i < _values.Count; i++)
            {
                float t = (float)i / (_values.Count - 1);
                int x = Mathf.Clamp(Mathf.RoundToInt(t * (graphWidth - 1)), 0, graphWidth - 1);
                int y = ValueToY(_values[i]);

                DrawLine(_pixels, lastX, lastY, x, y, lineColor);

                lastX = x;
                lastY = y;
            }
        }

        _texture.SetPixels(_pixels);
        _texture.Apply();
    }

    int ValueToY(float value)
    {
        if (maxAmplitude <= 0f)
            maxAmplitude = 1f;

        float v = Mathf.InverseLerp(-maxAmplitude, maxAmplitude, value);
        int y = Mathf.RoundToInt(v * (graphHeight - 1));
        return Mathf.Clamp(y, 0, graphHeight - 1);
    }
    
    void DrawLine(Color[] pixels, int x0, int y0, int x1, int y1, Color col)
    {
        int w = graphWidth;
        int h = graphHeight;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
                pixels[y0 * w + x0] = col;

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
