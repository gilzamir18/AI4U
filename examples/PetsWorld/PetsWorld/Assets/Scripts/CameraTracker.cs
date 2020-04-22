using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
	private float refreshDisplayFreq = 1;

	public Text wasdIndicator;

	public Text displayEnergy;

	public Dropdown agentList;
    
	private RawImage agentViewAsImage;

	public GameObject agentView;

	private float displayDelta = 0;

    public GameObject target;
    public float height;
    public float distance;

	private Manager manager;

	private bool WASD = true;

	public float speed = 30;

	private Texture2D agentViewTexture = null;

    // Start is called before the first frame update
    void Start()
    {

		if (agentView != null) {
			agentViewAsImage = agentView.GetComponent<RawImage>();
		}
        displayDelta = 0;
		refreshDisplayFreq = 1;
		manager = Manager.instance.GetComponent<Manager>();
		agentList.onValueChanged.AddListener(delegate
			{
				OnValueChanged(agentList);
			}
		);
		UpdateTargetValue(agentList);
		UpdateWASDIndicator();
		UpdateAgentView();
	}


	private void  UpdateWASDIndicator(){
		if (wasdIndicator != null) {
			wasdIndicator.text = WASD ? "wasd on" : "wasd off";
		}
	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		
	}

	private Color eatingColor = new Color(0.96f, 0.96f, 0.86f, 1.0f);
	private Color rockColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
	private Color skyColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);
	private Color wallColor = Color.yellow;
	private Color bocaColor = Color.red;
	private void UpdateAgentView(){
		if (agentView != null) {
			if (target != null) {
				if (this.agentViewTexture == null) {
					this.agentViewTexture = new Texture2D(20, 20);
				}
				int[,] img = target.GetComponent<petanim>().GetViewMatrix();
				for (int i = 0; i < 20; i++) {
					for (int j = 0; j < 20; j++) {
						if (img[i, j] == 0){
							agentViewTexture.SetPixel(i, j, skyColor);
						} else if (img[i, j] == 1) {
							agentViewTexture.SetPixel(i, j, Color.green);
						} else if (img[i, j] == 2) {
							agentViewTexture.SetPixel(i, j, wallColor);
						} else if (img[i, j] == 3) {
							agentViewTexture.SetPixel(i, j, rockColor);
						} else if (img[i, j] == 4) {
							agentViewTexture.SetPixel(i, j, eatingColor);
						} else if (img[i,j] < 20) {
							agentViewTexture.SetPixel(i, j, Color.magenta);
						} else {
							agentViewTexture.SetPixel(i, j, Color.red);
						}
					}
				}
				agentViewAsImage.texture = agentViewTexture;
				agentViewTexture.Apply();
			}
		}
	}

    // Update is called once per frame
    void Update()
    {


		Vector3 p = gameObject.transform.position;
		p.y = height;
		gameObject.transform.position = p;

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
						gameObject.transform.position.y, target.transform.position.z + distance);
			if (target != null) {
				UpdateAgentView();
				gameObject.transform.LookAt(target.transform);
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
    }


	private void UpdateTargetValue(Dropdown dropdown){
		int idx = dropdown.value - 1;
		//Debug.Log("Selected Agent " + idx);
		if (idx >= 0) {
			target = manager.Agents[idx];
			WASD = false;
		} else {
			WASD = true;
			target = null;
			displayEnergy.text = "";
			transform.rotation = Quaternion.identity;
		}
		UpdateWASDIndicator();
	}

	public void OnValueChanged(Dropdown dropdown){
		UpdateTargetValue(dropdown);
	}
}
