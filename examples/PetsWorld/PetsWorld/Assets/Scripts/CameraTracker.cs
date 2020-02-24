using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
	private float refreshDisplayFreq = 1;

	public Text displayEnergy;

	public Dropdown agentList;
    
	private float displayDelta = 0;

    public GameObject target;
    public float height;
    public float distance;

    public bool FPS;

	private bool firstTime = true;

	public bool alwaysUpdateOrientation;

	private Manager manager;

	private bool WASD = true;

	public float speed = 30;

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


		Vector3 p = gameObject.transform.position;
		p.y = height;
		gameObject.transform.position = p;

		if (Input.GetKey(KeyCode.F)){
			WASD = !WASD;
		}

		if (WASD) {
			// Get the horizontal and vertical axis.
			// By default they are mapped to the arrow keys.
			// The value is in the range -1 to 1
			float translation = Input.GetAxis("Vertical") * speed;
			float rotation = Input.GetAxis("Horizontal") * speed;

			// Make it move 10 meters per second instead of 10 meters per frame...
			translation *= Time.deltaTime;
			rotation *= Time.deltaTime;

			// Move translation along the object's z-axis
			transform.Translate(0, 0, translation);

			// Rotate around our y-axis
			transform.Rotate(0, rotation, 0);
		} else if (target != null) {
			gameObject.transform.position = new Vector3(target.transform.position.x,
						target.transform.position.y + height, target.transform.position.z + distance);

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
