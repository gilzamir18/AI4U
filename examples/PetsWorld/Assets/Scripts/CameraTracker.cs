using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
	private float refreshDisplayFreq = 1;

	public Text displayEnergy;

	public Dropdown agentList;

	public Text epCounter;
    
	private float displayDelta = 0;

    public GameObject target;
    public float verticalDistance;
    public float distance;
    public bool FPS;

	private bool firstTime = true;

	public bool alwaysUpdateOrientation;

	private Manager manager;

    // Start is called before the first frame update
    void Start()
    {
        displayDelta = 0;
		refreshDisplayFreq = 1;
		manager = Manager.instance.GetComponent<Manager>();
		agentList.onValueChanged.AddListener(delegate
			{
				OnValueChanged(agentList);
			}
		);
		if (epCounter != null) {
			epCounter.text = "Ep Counter " + Manager.epCounter;
		}
	}


	private void UpdateOrientation(){
		if (FPS){
			gameObject.transform.eulerAngles = new Vector3(
			target.transform.eulerAngles.x,
			target.transform.eulerAngles.y,
			target.transform.eulerAngles.z);
		} else {	
			gameObject.transform.eulerAngles = new Vector3(
			target.transform.eulerAngles.x,
			target.transform.eulerAngles.y + 180,
			target.transform.eulerAngles.z);
		}
	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		
	}

    // Update is called once per frame
    void Update()
    {
		if (target != null) {
			gameObject.transform.position = new Vector3(target.transform.position.x,
						target.transform.position.y + verticalDistance,  						  target.transform.position.z + distance);

			if (firstTime) {
				UpdateOrientation();
				firstTime = false;
			} else if (alwaysUpdateOrientation) {
				UpdateOrientation();
			}
		}

		if (target != null) {
			float energy  = target.GetComponent<petanim>().Energy;
			displayDelta += Time.deltaTime;
			if (displayDelta > refreshDisplayFreq ) {
				if (manager.SumOfEnergy <= 0) {
					displayEnergy.text = "GameOver!";
				} else if (displayEnergy != null) {
					displayEnergy.text = energy + "";
				}
				displayDelta = 0;
			}
		}
    }

	public void OnValueChanged(Dropdown dropdown){
		int idx = dropdown.value - 1;
		Debug.Log("Selected Agent " + idx);
		if (idx >= 0) {
			target = manager.Agents[idx];
		} else {
			target = null;
			displayEnergy.text = "";
		}
	}
}
