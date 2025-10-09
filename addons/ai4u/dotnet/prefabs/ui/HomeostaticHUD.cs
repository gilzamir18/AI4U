using Godot;
using System;
using ai4u;
using ai4u.ext;
using System.Collections.Generic;


public partial class HomeostaticHUD : Control
{

	[Export]
	private Color checkInColor = new Color(0, 1, 0);

	[Export]
	private Color checkOutColor = new Color(1, 0, 0);

	[Export]
	private Vector2 alertSize = new Vector2(20, 20);

	[Export]
	private float precision = 0.001f;

	[Export]
	private bool test = false;

	private HomeostaticVariable[] variables;
	private GridContainer container;

	private HSlider[] sliders;
	private ColorRect[] alerts;

	public override void _Ready()
	{
		if (test)
		{
			HomeostaticVariable var1 = new HomeostaticVariable();
			var1.name = "Pain";
			var1.rangeMin = 0.0f;
			var1.rangeMax = 1.0f;
			var1.minValue = 0.0f;
			var1.maxValue = 0.1f;
			var1.Value = 0.05f;
			
			HomeostaticVariable var2 = new HomeostaticVariable();
			var2.name = "Satatisfaction";
			var2.rangeMin = 0.0f;
			var2.rangeMax = 1.0f;
			var2.minValue = 0.5f;
			var2.maxValue = 1.0f;
			var2.Value = 0.4f;

			SetupVariables(new HomeostaticVariable[]{var1, var2});
			UpdateSliders();
		}
	}

	public void SetupVariables(HomeostaticVariable[] variables)
	{
		this.variables = variables;

		container = GetNode<GridContainer>("GridContainer");
		sliders = new HSlider[variables.Length];
		alerts = new ColorRect[variables.Length];

		for (int i = 0; i < variables.Length; i++)
		{
			if (!variables[i].Display)
				continue;

			var v = variables[i];

			Label label = new Label();
			label.Text = v.name;
			
			HSlider slider = new HSlider();
			slider.MinValue = v.rangeMin;
			slider.MaxValue = v.rangeMax;
			slider.Step = precision;
			slider.Rounded = false;

			slider.Value = v.Value;
			slider.SizeFlagsHorizontal = SizeFlags.ExpandFill;

			sliders[i] = slider;


			ColorRect alert = new ColorRect();
			alert.Color = checkInColor;
			alert.CustomMinimumSize = alertSize;
			alert.UpdateMinimumSize();
			alerts[i] = alert;

			container.AddChild(label);
			container.AddChild(slider);
			container.AddChild(alert);
		}
	}


	public void UpdateSliders()
	{
		for (int i = 0; i < variables.Length; i++)
		{
			if (!variables[i].Display)
			{
				continue;
			}

			sliders[i].Value = variables[i].Value;
			if (variables[i].Check())
			{
				alerts[i].Color = checkInColor;
			}
			else
			{
				alerts[i].Color = checkOutColor;
			}
			//GD.Print(variables[i].name + " :::: "  + variables[i].Value);
		}
	}
}
