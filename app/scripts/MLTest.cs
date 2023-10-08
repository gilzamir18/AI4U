using Godot;
using System;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Collections;

public partial class MLTest : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	private InferenceSession session;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		session = new InferenceSession("model.onnx");
		inference();
	}


	private void inference()
	{
		Tensor<float> t1 =  new DenseTensor<float>(new[] { 1, 7 });;
		Tensor<float> t2 = new DenseTensor<float>(new[]{ 1, 100 });

		var inputs = new List<NamedOnnxValue>(){
			NamedOnnxValue.CreateFromTensor<float>("input", t1),
			NamedOnnxValue.CreateFromTensor<float>("onnx::Flatten_1", t2)
		};


		using (var results = session.Run(inputs))
		{
			foreach(var r in results)
			{
				GD.Print(r.Name + ": ");
				var output = r.AsTensor<float>();
				for (int i = 0; i < output.Length; i++)
				{
					GD.Print(output.GetValue(i) + " ");
				}
			}
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
