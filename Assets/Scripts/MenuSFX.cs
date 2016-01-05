using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSFX : MonoBehaviour {

    private Master m;


	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        //

        test b = newFunc;
        UnityEngine.Events.UnityAction<float> a;
        a = new UnityEngine.Events.UnityAction<float>(b);

        //GetComponent<Slider>().onValueChanged.AddListener((0) =>
        
        //    test(a)
        
        //);
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Up") || Input.GetButtonDown("Right")
            || Input.GetButtonDown("Left") || Input.GetButtonDown("Down"))
        {
            m.PlaySFX(1);
        }
        if (Input.GetButtonDown("Submit"))
        {
            m.PlaySFX(0);
        }
	}

    public delegate void test(float a);

    public void newFunc(float a){
        Debug.Log("a is: " + a);
    }
}
