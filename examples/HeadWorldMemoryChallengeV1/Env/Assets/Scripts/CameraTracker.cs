using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;

public class CameraTracker : MonoBehaviour
{
	private float refreshDisplayFreq = 1;

	public Text wasdIndicator;

	public Text displayEnergy;

	private RawImage agentViewAsImage;

	public GameObject agentView;

	private float displayDelta = 0;

    public GameObject target;
    public float height;
    public float distance;

	private bool TopView = true;

	public float speed = 30;

	private Texture2D agentViewTexture = null;

	public Text displayTimeToLeft;

	public petanim agent;

	public Button btnChangeView;

	private static Vector3 cameraPosition;
	private static Quaternion cameraRotation;


	private static bool firstEpisode = true;

    // Start is called before the first frame update
    void Start()
    {
		if (agentView != null) {
			agentViewAsImage = agentView.GetComponent<RawImage>();
		}
        displayDelta = 0;

		btnChangeView.onClick.AddListener(TaskOnClick);

		refreshDisplayFreq = 1;
		if (firstEpisode) {
			cameraPosition = gameObject.transform.position;
			cameraRotation = gameObject.transform.rotation;
			firstEpisode = false;
			GetComponent<Camera>().orthographic = true;
			UpdateAgentView();
		} else {
			gameObject.transform.position = cameraPosition;
			gameObject.transform.rotation = cameraRotation;
		}
		UpdateWASDIndicator();
	}

	public void TaskOnClick()
    {
		TopView = !TopView;
		if (TopView)
        {
			transform.position = cameraPosition;
			transform.rotation = cameraRotation;
			GetComponent<Camera>().orthographic = true;
		}
	}

	private void  UpdateWASDIndicator(){
		if (wasdIndicator != null) {
			wasdIndicator.text = TopView ? "wasd on" : "wasd off";
		}
	}

	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		
	}


	private Color skyColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);
	private Color wallColor = Color.gray;
	private Color gateColor = new Color(0.8f, 0.4f, 0.05f, 1.0f);
	private void UpdateAgentView(GameObject target=null){
		if (target == null) {
			target = this.target;
		}
		if (agentView != null) {
			if (target != null) {
				if (this.agentViewTexture == null) {
					this.agentViewTexture = new Texture2D(20, 20);
				}
				int[,] img = target.GetComponent<petanim>().GetViewMatrix();
				if (img != null)
				{
						for (int i = 0; i < 20; i++)
						{
							for (int j = 0; j < 20; j++)
							{
								if (img[i, j] == 0)
								{
									agentViewTexture.SetPixel(i, j, skyColor);
								}
								else if (img[i, j] == 6)
								{
									agentViewTexture.SetPixel(i, j, gateColor);
								}
								else if (img[i, j] == 11)
								{
									agentViewTexture.SetPixel(i, j, wallColor);
								}
								else if (img[i, j] == 16)
								{
									agentViewTexture.SetPixel(i, j, Color.red);
								}
								else if (img[i, j] == 21)
								{
									agentViewTexture.SetPixel(i, j, Color.yellow);
								}
								else if (img[i, j] == 1)
								{
									agentViewTexture.SetPixel(i, j, Color.green);
								}
								else
								{
									agentViewTexture.SetPixel(i, j, Color.black);
								}
							}
						}
						agentViewAsImage.texture = agentViewTexture;
						agentViewTexture.Apply();
					}
				}
		}
	}

	void Update()
    {
		
		if (target != null) {
			if (!TopView)
            {
				transform.position = target.transform.position - target.transform.forward * distance;
				GetComponent<Camera>().orthographic = false;
				Vector3 pos = gameObject.transform.position;
				gameObject.transform.position = new Vector3(pos.x, height, pos.y);
				gameObject.transform.LookAt(target.transform);
			}
			UpdateAgentView();
		}

		if (displayTimeToLeft != null) {
			displayTimeToLeft.text = "Time to Left " + agent.Energy.ToString("000.00");
		}
    }
}
