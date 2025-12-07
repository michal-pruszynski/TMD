using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RealtimeAmplitudeHistoryGraph : MonoBehaviour
{
    [Header("Graph Size (pixels)")]
    public int graphWidth = 400;
    public int graphHeight = 120;

    [Header("Appearance")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Color axisColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color lineColor = Color.red;
    public Color lineColorNoTmd = Color.blue;

    [Header("Value Readout (optional)")]
    public TextMeshProUGUI currentXText; // shows time (x)
    public TextMeshProUGUI currentYText; // shows amplitude (y)

    [Header("Sampling")]
    [Tooltip("Seconds between samples.")]
    public float sampleInterval = 0.02f;

    [Header("Amplitude Range")]
    [Tooltip("Expected max absolute amplitude. For -1..1 use 1.")]
    public float maxAmplitude = 1f; // we’re targeting [-1, 1]

    [Header("History")]
    [Tooltip("Maximum samples to keep (per series).")]
    public int maxSamples = 400;

    private RawImage _rawImage;
    private Texture2D _texture;

    private readonly List<float> _values = new List<float>();
    private readonly List<float> _valuesNoTmd = new List<float>();
    private float _sampleTimer;

    private Color[] _pixels;

    bool needsRedraw = false;

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
        if (needsRedraw)
        {
            Redraw();
            needsRedraw = false;
        }
    }


    public void AddSample(float value)
    {
        if (_values.Count >= maxSamples)
            _values.RemoveAt(0);

        _values.Add(value);

        RecalculateMaxAmplitude();
        Redraw();
    }

    public void AddSampleNoTmd(float value)
    {
        if (_valuesNoTmd.Count >= maxSamples)
            _valuesNoTmd.RemoveAt(0);

        _valuesNoTmd.Add(value);

        RecalculateMaxAmplitude();
        Redraw();
    }

    void Redraw()
    {
        Color[] pixels = _pixels;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = backgroundColor;

        // zero axis
        int midY = graphHeight / 2;
        for (int x = 0; x < graphWidth; x++)
            pixels[midY * graphWidth + x] = axisColor;

        // 1) TMD series (blue)
        if (_values.Count > 1)
            DrawSeries(_values, lineColor, pixels);

        // 2) no-TMD series (red), drawn on top if present
        if (_valuesNoTmd.Count > 1)
            DrawSeries(_valuesNoTmd, lineColorNoTmd, pixels);

        _texture.SetPixels(pixels);
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
    void DrawSeries(List<float> values, Color col, Color[] pixels)
    {
        int count = values.Count;
        int lastX = 0;
        int lastY = ValueToY(values[0]);

        for (int i = 1; i < count; i++)
        {
            float t = (float)i / (count - 1);  // 0..1 along X
            int x = Mathf.Clamp(Mathf.RoundToInt(t * (graphWidth - 1)), 0, graphWidth - 1);
            int y = ValueToY(values[i]);

            DrawLine(pixels, lastX, lastY, x, y, col);

            lastX = x;
            lastY = y;
        }
    }

    void RecalculateMaxAmplitude()
    {
        float maxAbs = 0f;

        // TMD series
        for (int i = 0; i < _values.Count; i++)
        {
            float v = Mathf.Abs(_values[i]);
            if (v > maxAbs) maxAbs = v;
        }

        // no-TMD series (if you have it)
        for (int i = 0; i < _valuesNoTmd.Count; i++)
        {
            float v = Mathf.Abs(_valuesNoTmd[i]);
            if (v > maxAbs) maxAbs = v;
        }

        // fallback so graph doesn't break if everything is zero
        if (maxAbs <= 0f)
            maxAbs = 1f;

        maxAmplitude = maxAbs;
    }


}
