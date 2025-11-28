using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[ExecuteAlways]
public class SliderController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float minValue = 0;
    public float maxValue = 100;
    public float value = 0;

    public string varName = "height";
    public string varSymbol = "h";
    public string unit = "m"; //eg. m -> meters

    public TextMeshProUGUI sliderLabel;
    public TextMeshProUGUI sliderValueText;
    public Slider slider;

	void OnEnable()
	{
        Start();
	}

	void OnValidate()
	{

        Start();
	}

	void Start()
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;


        sliderLabel.text = varSymbol;
        sliderValueText.text = value.ToString()+unit;
    }

    // Update is called once per frame
    void Update()
    {
        value = slider.value;
		sliderLabel.text = varSymbol;
		sliderValueText.text = value.ToString() + unit;
	}

    public void forceSetValue(float x) {
        slider.value = x;


	}
}
