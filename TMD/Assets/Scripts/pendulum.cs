using UnityEngine;

public class pendulum : MonoBehaviour
{
    public JointBending building;
    public GameObject swing;
    public GameObject weight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setLenghts(1,1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = building.tmdPoint;

    }

    public void setLenghts(float l, float m) {
        swing.transform.localScale = new Vector3(swing.transform.localScale.x, l, swing.transform.localScale.z);
        swing.transform.localPosition = new Vector3(0, -l / 2.0f, 0);
        weight.transform.localPosition = new Vector3(0, -l,0);
    }
}
