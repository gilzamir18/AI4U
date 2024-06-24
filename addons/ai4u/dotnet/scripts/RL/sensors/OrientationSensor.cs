using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Godot;

namespace ai4u {


	public enum InfoType {
		ANGLE,
		DIST,
		BOTH
	}	

	public partial class OrientationSensor : Sensor
	{
		///<summary>
		/// The location in the scene for which the angular distance and the Euclidean distance are calculated.
		///</summary>
		[Export]	
		private Node3D target;
		
		
		///<summary>
		/// The location in the scene from which the angular distance and the Euclidean distance are calculated.
		///</summary>
		[Export]
		private Node3D reference = null;
		
		/// <summary>
		/// Maximum distance from the source at which the sensor captures orientation signals. 
		/// Beyond this distance, the signal is zeroed..
		/// </summary>
		[Export]
		private float maxDistance = 1;


		/// <summary>
		/// Determines the type of information that the sensor generates, 
		/// which can be either the orientation between the two reference systems, 
		/// or the Euclidean distance between the systems, or both pieces of information together.
		/// </summary>
		[Export(PropertyHint.Enum, "ANGLE=include only angle, DIST=include only euclidian distance, BOTH = include angle and distance.")]
		private InfoType info = InfoType.BOTH;

		/// <summary>
		/// Determines whether the distance will be normalized. 
		/// The angle between the reference system and the target always varies between -1 and 1. 
		/// However, the distance between the reference system and the target varies between 0 and 1 
		/// if this property is activated, or between 0 and maxDistance if not.
		/// </summary>
		[Export]
		private bool _normalized;

		/// <summary>
		/// The sensor is only activated if there is visibility between the reference system and the target. 
		/// This option significantly increases computational cost, so use it sparingly.
		/// </summary>
		[Export]
		private bool visibilityTest = false;


		/// <summary>
		/// Exclude agent from visibility test.
		/// </summary>
		[Export]
		private bool excludeAgentFromVisibilityTest = true;

		/// <summary>
		/// A vertical offset to prevent the visibility test from failing because the raycasting hits the ground.
		/// </summary>
		[Export]
		private float verticalShiftForVisibility = 0.5f;

		[Export(PropertyHint.LayersAvoidance)]
		private uint visibilityTestMask = uint.MaxValue;

		[Export]
		private bool ignoreVibilityTestForAngle = false;
		[Export]
		private bool ignoreVibilityTestForDist = false;
		

		private HistoryStack<float> history;


		public float[] LastFloatArrayValue => lastFloatArrayValue;

		public float MaxDistance => maxDistance;

		private float[] lastFloatArrayValue;

		private PhysicsDirectSpaceState3D spaceState;

		public InfoType Info => info;

		public override void OnSetup(Agent agent) {
			this.agent = (BasicAgent)agent;

			if (reference == null) {
				reference = this.agent.GetAvatarBody() as Node3D;
			} 

			if (visibilityTest)
			{
				spaceState = (this.agent.GetAvatarBody() as PhysicsBody3D).GetWorld3D().DirectSpaceState;
			}
			if (target == null)
			{
				GD.Print("OrientationSensor error: Mandatory field (target) not provided!");
			}


			type = SensorType.sfloatarray;
			if (info == InfoType.BOTH)
			{
				shape = new int[1]{2*stackedObservations};
			}
			else
			{
				shape = new int[]{1*stackedObservations};
			}

			history = new HistoryStack<float>(shape[0]);
			normalized =  _normalized;
		}

		public override void OnReset(Agent aget)
		{
			history = new HistoryStack<float>(shape[0]);
			lastFloatArrayValue = null;
		}

		public override float[] GetFloatArrayValue()
		{
			if (target == null) {
				GD.Print("OrientationSensor error: target don't specified! Game Object: " + Name);
			}

			Vector3 f = reference.GlobalTransform.Basis.Z;

			Vector3 d = target.GlobalTransform.Origin - reference.GlobalTransform.Origin;
			var dist = d.Length();
			bool testResult = true;

			float distResult = -1;
			float angResult = -2;


            if (visibilityTest)
            {
                if (!(ignoreVibilityTestForAngle && ignoreVibilityTestForDist))
                {

                    var vshift = reference.GlobalTransform.Basis.Y * verticalShiftForVisibility;
                    var query = PhysicsRayQueryParameters3D.Create(reference.GlobalTransform.Origin + vshift, target.GlobalTransform.Origin + vshift, visibilityTestMask);


                    if (excludeAgentFromVisibilityTest)
                    {
                        var rb = agent.GetAvatarBody() as PhysicsBody3D;
                        query.Exclude = new Godot.Collections.Array<Rid> { rb.GetRid() };
                    }

                    var result = this.spaceState.IntersectRay(query);
				
                    if (result != null && result.Count > 0)
                    {
                        Node3D gobj = (Node3D)result["collider"];
                        if (gobj.Visible && gobj != target)
                        {
                            testResult = false;
                        }
                    }
                    else
                    {
                        testResult = false;
                    }
                }
            }


            if (dist < maxDistance)
            {
                if (testResult && (info == InfoType.ANGLE || info == InfoType.BOTH))
                {
					angResult = f.Dot(d.Normalized());
                }
				if (testResult && (info == InfoType.DIST || info == InfoType.BOTH))
				{
					if (normalized)
					{
						distResult = dist / maxDistance;
					}
					else
					{
						distResult = dist;
					}
				}
            }
            else
            {
				distResult = -1;
				angResult = -2;
            }


			history.Push(angResult);
			history.Push(distResult);

			lastFloatArrayValue = history.Values;
			return lastFloatArrayValue;
		}
	}
}
