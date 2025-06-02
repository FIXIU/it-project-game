using Godot;
using System;

public partial class MovementParticles : Node2D
{
	[Export]
	public CpuParticles2D MyParticles { get; set; }

	public override void _Ready()
	{
		if (MyParticles == null)
		{
			GD.PrintErr("ParticleControllerCPU: MyParticles node not assigned in the inspector!");
		}
		else
		{
			MyParticles.Spread = 30.0f;
			MyParticles.Lifetime = 0.1f;
			MyParticles.LifetimeRandomness = 0.05f;
			MyParticles.ScaleAmountMin = 0.5f;
			MyParticles.ScaleAmountMax = 1.5f;
		}
	}

	public override void _Process(double delta)
	{
		if (MyParticles != null)
		{		
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if (direction != Vector2.Zero)
			{
				MyParticles.Direction = new Vector2(direction.X, -direction.Length() * 100.0f);
				MyParticles.Emitting = true;
				MyParticles.InitialVelocityMax = direction.Length() * 60.0f;
				MyParticles.InitialVelocityMin = direction.Length() * 30.0f;
				TriggerParticles();
			}
			else
			{
				StopParticles();
			}
			if (Input.IsActionJustPressed("ui_select"))
			{
				 if (!MyParticles.OneShot)
				 {
					ToggleEmission();
				 }
			}
		}
		else
		{
			GD.PrintErr("MyParticles is still null in _Process!");
		}
	}

	public void TriggerParticles()
	{
		if (MyParticles == null) return;

		MyParticles.Emitting = true;
	}

	public void StopParticles()
	{
		if (MyParticles == null || MyParticles.OneShot) return;

		MyParticles.Emitting = false;
	}

	public void ToggleEmission()
	{
		 if (MyParticles == null || MyParticles.OneShot) return;

		 MyParticles.Emitting = !MyParticles.Emitting;
		 GD.Print($"Continuous CPU particle emission set to: {MyParticles.Emitting}");
	}

	public void EmitAtPosition(Vector2 worldPosition)
	{
		if (MyParticles == null) return;

		MyParticles.GlobalPosition = worldPosition;

		TriggerParticles();
	}
}
