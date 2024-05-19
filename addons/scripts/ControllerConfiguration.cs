using Godot;
using System;

namespace ai4u;

public partial class ControllerConfiguration : Node
{
		[Export]
		public int skipFrame = 4;
		[Export]
		public bool repeatAction = true;
}
