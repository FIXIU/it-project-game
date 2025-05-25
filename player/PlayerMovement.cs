using Godot;
using System;

public partial class PlayerMovement : CharacterBody2D
{
	public const float Speed = 200.0f;
	public const float JumpVelocity = -400.0f;
	
	// Variable jump
	private float jumpCutMultiplier = 0.4f; // Button release jump cutoff
	private bool isJumping = false;
	private float minJumpVelocity = -150.0f; // Minimum jump height
	
	// Wall jump boost
	private float wallJumpBoostTime = 0.3f;
	private float currentWallJumpTime = 0f;
	private float wallJumpForce = 0f;
	private int wallJumpDirection = 0;

	// Coyote time
	private float coyoteTimeMax = 0.15f;
	private float coyoteTimeLeft = 0f;
	private bool wasOnFloor = false;
	
	// Wall coyote time parameters
	private float wallCoyoteTimeMax = 0.1f; // 100ms of wall coyote time
	private float wallCoyoteTimeLeft = 0f;
	private bool wasOnWall = false;
	private Vector2 lastWallNormal = Vector2.Zero;

	[Export] public StateMachine stateMachine;
	[Export] public Node2D VisualsNode { get; set; }
	[Export] public Node2D CollisionNode { get; set; }

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float deltaFloat = (float)delta;

		// Update wall jump timer
		if (currentWallJumpTime > 0)
		{
			currentWallJumpTime -= deltaFloat;
			if (currentWallJumpTime <= 0)
			{
				// Reset when timer ends
				currentWallJumpTime = 0;
				wallJumpForce = 0;
				wallJumpDirection = 0;
			}
		}

		// Update floor coyote time
		if (IsOnFloor())
		{
			// Reset coyote time when on floor
			coyoteTimeLeft = coyoteTimeMax;
			wasOnFloor = true;
			isJumping = false; // Reset jumping state when landing
		}
		else if (wasOnFloor) 
		{
			// Started falling, begin coyote time countdown
			wasOnFloor = false;
		}
		else if (coyoteTimeLeft > 0)
		{
			// In the air and coyote time is active
			coyoteTimeLeft -= deltaFloat;
		}
		
		// Update wall coyote time
		if (IsOnWall() && !IsOnFloor())
		{
			// Reset wall coyote time when on wall
			wallCoyoteTimeLeft = wallCoyoteTimeMax;
			lastWallNormal = GetWallNormal().Normalized();
			wasOnWall = true;
		}
		else if (wasOnWall)
		{
			// Just left the wall, begin wall coyote time countdown
			wasOnWall = false;
		}
		else if (wallCoyoteTimeLeft > 0)
		{
			// In the air and wall coyote time is active
			wallCoyoteTimeLeft -= deltaFloat;
		}

		// Add the gravity.
		if (!IsOnFloor() && !IsOnWall())
		{
			velocity += GetGravity() * deltaFloat;
		}
		
		// Handle wall mechanics

		// 		Wall slide
		if (IsOnWall() && !IsOnFloor())
		{
			velocity += GetGravity()/2 * deltaFloat;
		}

		// 		Wall jump (with wall coyote time)
		bool canWallJump = (IsOnWall() || wallCoyoteTimeLeft > 0) && !IsOnFloor();
		if (canWallJump && Input.IsActionJustPressed("ui_accept"))
		{
			GD.Print("Attempting Wall Jump"); // DEBUG
			// Get wall normal - either current or from coyote time
			Vector2 wallNormal = IsOnWall() ? GetWallNormal().Normalized() : lastWallNormal;
			
			// Set vertical jump velocity immediately
			velocity.Y = JumpVelocity;
			isJumping = true;
			
			// Store wall jump parameters for horizontal movement
			wallJumpDirection = (int)Mathf.Sign(wallNormal.X);
			wallJumpForce = Speed * 1.5f;
			currentWallJumpTime = wallJumpBoostTime;
			
			// Consume wall coyote time
			wallCoyoteTimeLeft = 0;
			
			GD.Print($"Wall Jump: Direction = {wallJumpDirection}, Force = {wallJumpForce}");
			if (stateMachine != null)
			{
				GD.Print("Transitioning to Jump state from Wall Jump"); // DEBUG
				stateMachine.TransitionTo("Jump"); // Or a specific WallJump state
			}
			else
			{
				GD.Print("StateMachine is null in Wall Jump"); // DEBUG
			}
		}

		// Handle Jump (with coyote time)
		bool canJump = IsOnFloor() || coyoteTimeLeft > 0;
		if (Input.IsActionJustPressed("ui_accept") && canJump && !canWallJump) // Ensure not wall jumping
		{
			GD.Print("Attempting Regular Jump"); // DEBUG
			velocity.Y = JumpVelocity;
			coyoteTimeLeft = 0; // Used the coyote time
			isJumping = true;
			if (stateMachine != null)
			{
				GD.Print("Transitioning to Jump state from Regular Jump"); // DEBUG
				stateMachine.TransitionTo("Jump");
			}
			else
			{
				GD.Print("StateMachine is null in Regular Jump"); // DEBUG
			}
		}
		
		// Variable jump height - cut jump short when button is released
		if (isJumping && Input.IsActionJustReleased("ui_accept") && velocity.Y < minJumpVelocity)
		{
			velocity.Y *= jumpCutMultiplier; // Cut the jump short
		}

		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		// Handle visual flipping based on input direction
		if (VisualsNode != null) // Ensure VisualsNode is assigned in the editor
		{
			if (direction.X != 0) // Only flip if there's horizontal input
			{
				float newScaleX = Mathf.Abs(VisualsNode.Scale.X);
				// If scale was somehow 0, default to 1 to avoid issues.
				if (newScaleX == 0) newScaleX = 1.0f;

				if (direction.X < 0)
				{
					VisualsNode.Scale = new Vector2(-newScaleX, VisualsNode.Scale.Y);
				}
				else // direction.X > 0
				{
					VisualsNode.Scale = new Vector2(newScaleX, VisualsNode.Scale.Y);
				}
			}
		}
		
		if (CollisionNode != null) // Ensure VisualsNode is assigned in the editor
		{
			if (direction.X != 0) // Only flip if there's horizontal input
			{
				float newPosX = Mathf.Abs(CollisionNode.Position.X);
				// If scale was somehow 0, default to 1 to avoid issues.
				if (newPosX == 0) newPosX = 1.0f;

				if (direction.X < 0)
				{
					CollisionNode.Position = new Vector2(newPosX, CollisionNode.Position.Y);
				}
				else // direction.X > 0
				{
					CollisionNode.Position = new Vector2(-newPosX, CollisionNode.Position.Y);
				}
			}
		}
		
		// Calculate horizontal velocity with wall jump influence
		if (currentWallJumpTime > 0)
		{
			// Calculate how much influence the wall jump still has (1.0 to 0.0)
			float wallJumpInfluence = currentWallJumpTime / wallJumpBoostTime;
			
			// Blend wall jump force with player input
			float playerInfluence = 1.0f - wallJumpInfluence * 0.8f; // Allow some player control even at start
			
			// Apply both forces
			velocity.X = (wallJumpDirection * wallJumpForce * wallJumpInfluence) + 
						(direction.X * Speed * playerInfluence);
		}
		else if (direction.X != 0)
		{
			velocity.X = direction.X * Speed;
			if (IsOnFloor() && !isJumping) // Modified condition: only transition to Run if on floor AND not jumping
			{
				stateMachine?.TransitionTo("Run");
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			if (IsOnFloor() && !isJumping) // Ensure not in the middle of a jump action
			{
				stateMachine?.TransitionTo("Idle");
			}
		}

		// State transitions based on vertical movement
		if (!IsOnFloor())
		{
			if (velocity.Y > 0 && !IsOnWall()) // Falling and not on a wall
			{
				stateMachine?.TransitionTo("Fall");
			}
			// Jump state is handled at the point of jump action
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
