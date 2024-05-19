using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;

namespace ai4u;

public partial class AnimatedSpriteManager2D : Node
{

	[Export]
    private Godot.Collections.Array<AnimatedSprite2D> _sprites;

	[Export]
	private string defaultSprite = "IDLE";

	[Export]
	private string defaultAction = "IDLE";

	public AnimatedSprite2D Current
	{
		get
		{
			return _curSprite;
		}
	}
	
	private AnimatedSprite2D _curSprite;
	private string _curSpriteName;

    public override void _Ready()
    {
        base._Ready();
		UpdateCurrentSprite(defaultSprite);
		Play(defaultAction);
    }

    public void UpdateCurrentSprite(string name)
	{
		for (int i = 0; i < _sprites.Count; i++)
		{
			if (name == _sprites[i].Name)
			{
				_curSprite = _sprites[i];
				_curSpriteName = _curSprite.Name;
				_curSprite.Visible = true;
			}
			else
			{
				_sprites[i].Visible = false;
				_sprites[i].Stop();
			}
        }
    }

	public void Play(string action, float customSpeed=1, bool fromEnd=false)
	{
        _curSprite.Play(action, customSpeed, fromEnd);
    }

	public void Stop()
	{
		_curSprite.Stop();
	}
}
