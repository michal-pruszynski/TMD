using System;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationLord : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    CultureInfo spaceCulture;



    //INPUTS
    public SliderController heightSlider;
    public SliderController widthSlider;
    public SliderController lenghtSlider;
    public SliderController massDamperSlider;
    public SliderController windVelocitySlider;

    //INPUT VARIABLES
    double h, a, l, md, v;

    double t = 0;

    //MID VARIABLES
    double mass;
   

	//END VARIABLES

	//OUTPUTS
	public TextMeshProUGUI massValue;
    public JointBending building;
    public double scale = 10;
    public RealtimeAmplitudeHistoryGraph graph;


	void Start()
    {
		spaceCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		spaceCulture.NumberFormat.NumberGroupSeparator = " ";
	}

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        //GET INPUT VARIABLES
        h = heightSlider.value;
        a = widthSlider.value;
        l = lenghtSlider.value;
		md = massDamperSlider.value*1000;
        v = windVelocitySlider.value;

		building.width = (float)(a / scale);
		building.height = (float)(h / scale);

		//CALCULATE MASS
		mass = a * a * h * 400;
        mass *= 0.5;
        massValue.text = (mass/1000).ToString("N0", spaceCulture) +"t";




        //UBER EQUATION FOR AMPLITUDE
        double w0, wn, wd, T, m, g;
        double Amplitude;
        m = mass;
        g = 9.81;
        wd = Math.Sqrt(g / l);
        T = 0.085 * Math.Pow(h, 0.75);
        wn = (2*Math.PI)/ T;
        w0 = 2 * Math.PI * 3;//0.12 * (v / a);


        double rd, rn, k, f0;

        rd = w0 / wd;
        rn = w0 / wn;
        k = (4 * Math.PI * Math.PI * m) / (T * T);
        f0 = (1.25 * ((v * v) / 2) * 1.3 * a * h);

        md = 0.0001;


        double denom = (1 - (rn * rn)) * (1 - (rd * rd)) - ((md / m) * rn * rn);



        Amplitude = (f0 / k) * (((1 - (rd * rd)) / denom));
		
		Debug.Log(Amplitude);

        double x = Math.Abs(Amplitude) * Math.Sin(wn * t);

		
        graph.AddSample((float)x);


        if (x < 1e-4 && x > 0) {
            x = 1e-4;
        }
		if (x > -1e-4 && x < 0)
		{
			x = -1e-4;
		}
		building.bendAmount = (float)(x / scale);

	}
}
