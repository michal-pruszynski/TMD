using System;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public SliderController resonanceVelocitySlider;

    //INPUT VARIABLES
    double h, a, l, md, v, res;

    double t = 0;

    //MID VARIABLES
    double mass, wn;
   

	//END VARIABLES

	//OUTPUTS
	public TextMeshProUGUI massValue;
	public TextMeshProUGUI ampValue;
	public TextMeshProUGUI wnValue;
	public TextMeshProUGUI wdValue;
	public TextMeshProUGUI w0Value;
    public pendulum tmd;




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
        res = resonanceVelocitySlider.value;

		building.width = (float)(a / scale);
		building.height = (float)(h / scale);

		//CALCULATE MASS
		mass = a * a * h * 7500 * 0.07;
        massValue.text = (mass/1000).ToString("N0", spaceCulture) +"t";

        tmd.setLenghts((float)(l / scale), 1);


        //UBER EQUATION FOR AMPLITUDE
        double w0, wd, T, m, g;
        double Amplitude;
        m = mass/2;
        g = 9.81;
        wd = Math.Sqrt(g / l);
        T = 0.085 * Math.Pow(h, 0.75);
        wn = (2*Math.PI)/ T;


        w0 = wn*(res/100);// 2 * Math.PI * 0.3;//0.12 * (v / a);

        //double St = 0.12;
        //double B = a;
        //w0 = 2 * Math.PI * St * v / B;

        double rd, rn, k, f0;

        rd = w0 / wd;
        rn = w0 / wn;
        k = (4 * Math.PI * Math.PI * m) / (T * T);
        f0 = (1.25 * ((v * v) / 2) * 1.3 * a * h);

        //md = 0.0001;


        double denom = (1 - (rn * rn)) * (1 - (rd * rd)) - ((md / m) * rn * rn);



        Amplitude = (f0 / k) * Math.Abs(((1 - (rd * rd)) / denom));
        
		
		///Debug.Log(Amplitude);
        ampValue.text = Math.Abs(Amplitude).ToString("N5", spaceCulture)+"m";
        wnValue.text = Math.Abs(wn).ToString("N3", spaceCulture)+"rad";
        wdValue.text = Math.Abs(wd).ToString("N3", spaceCulture)+"rad";
        w0Value.text = Math.Abs(w0).ToString("N3", spaceCulture)+"rad";

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



        //ARCSIN(X/L) dla pendululum

        double kd = md*wd*wd;



        double AmpDamp = (f0 / kd) * (((md / m) * rn * rn) / denom);
		double xd = Math.Abs(AmpDamp) * Math.Sin(wn * t + Math.PI);
        tmd.transform.rotation = Quaternion.Euler(0, 0, (float)(Math.Asin(Math.Clamp(xd/l, -1.0, 1.0)) * 180/Math.PI));
        Debug.Log(1 - rd*rd + (md / m));

	}

	public void setLtoResonate()
	{

        //QUEST: set l so that wd = wn
        //lenghtSlider.forceSetValue((float)(9.81 / (wn * wn)));
        lenghtSlider.forceSetValue((float)(9.81 / (wn * wn)));
        //1 - (wd / w0) ^ 2 + md(md / m);

	}
}
