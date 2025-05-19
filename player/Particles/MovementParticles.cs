using Godot;
using System;

public partial class MovementParticles : Node2D
{
	[Export]
	public CpuParticles2D MyParticles { get; set; }

	public override void _Ready()
	{
		// Ensure the particle node is assigned
		if (MyParticles == null)
		{
			GD.PrintErr("ParticleControllerCPU: MyParticles node not assigned in the inspector!");
			// Optionally, try to find it if not assigned:
			// MyParticles = GetNode<CpuParticles2D>("Path/To/Your/CpuParticles2D");
		}
		else
		{
			MyParticles.Spread = 30.0f;
			MyParticles.Lifetime = 0.1f; // Lifetime of each particle
			MyParticles.LifetimeRandomness = 0.05f; // Randomness in lifetime
			MyParticles.ScaleAmountMin = 0.5f; // Minimum scale of particles
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
			// Optionally print an error so you know it's still not assigned
			GD.PrintErr("MyParticles is still null in _Process!");
		}
	}

	/// <summary>
	/// Triggers the particle emission.
	/// If 'One Shot' is enabled on the CpuParticles2D node, it emits once.
	/// If 'One Shot' is disabled, this starts continuous emission.
	/// </summary>
	public void TriggerParticles()
	{
		if (MyParticles == null) return;

		// For CPUParticles2D, setting Emitting = true handles both cases correctly.
		// Restart() is primarily for GPU particles if you need to ensure it plays
		// from the beginning immediately, but setting Emitting=true works fine here.
		MyParticles.Emitting = true;
	}

	// Stops continuous particle emission. Has no effect if 'One Shot' is true.
	public void StopParticles()
	{
		if (MyParticles == null || MyParticles.OneShot) return;

		MyParticles.Emitting = false;
	}

	// Toggles continuous particle emission on/off. Does nothing if OneShot is true.
	public void ToggleEmission()
	{
		 if (MyParticles == null || MyParticles.OneShot) return;

		 MyParticles.Emitting = !MyParticles.Emitting;
		 GD.Print($"Continuous CPU particle emission set to: {MyParticles.Emitting}");
	}

	// Call this to place the particle emitter somewhere and trigger it.
	// Useful for effects like explosions at a specific point.
	public void EmitAtPosition(Vector2 worldPosition)
	{
		if (MyParticles == null) return;

		// Set the global position of the emitter
		MyParticles.GlobalPosition = worldPosition;

		// Trigger the emission (works for both one-shot and continuous)
		TriggerParticles();
		// Note: If it's a continuous emitter, it will *stay* at this new position
		// unless you move it again or stop emission.
	}
}
