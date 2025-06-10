using Godot;

public partial class PlayerMovement : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float JumpVelocity = -300.0f;
	[Export] public float wallJumpSpeedMultiplier = 1.5f;
	[Export] public float wallJumpBoostTime = 0.3f;
	[Export] public float wallJumpPlayerInfluence = 0.6f;
	[Export] public float jumpCutMultiplier = 0.5f;
	[Export] public float minJumpVelocity = -50.0f;
	[Export] public float coyoteTimeMax = 0.1f;
	[Export] public float wallCoyoteTimeMax = 0.1f;

	[Export] public Node2D VisualsNode;
	[Export] public Node2D CollisionNode;

	private Vector2 gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsVector2();

	[Export] private StateMachine stateMachine;
	[Export] private Timer attackTimer;

	// Input variables
	private Vector2 inputDirection;
	private bool jumpPressed;
	private bool jumpReleased;
	private bool attackPressed;

	// State variables
	private bool onFloor;
	private bool onWall;
	private bool isJumping;
	private bool canAttack = true;

	// Jump mechanics
	private float coyoteTimeLeft;
	private float wallCoyoteTimeLeft;
	private bool wasOnFloor;
	private bool wasOnWall;
	private Vector2 lastWallNormal;

	// Wall jump mechanics
	private float currentWallJumpTime;
	private float wallJumpForce;
	private int wallJumpDirection;

	public override void _Ready()
	{
		// Connect attack timer timeout signal - THIS IS THE KEY FIX!
		if (attackTimer != null)
		{
			attackTimer.Timeout += OnAttackTimerTimeout;
			attackTimer.OneShot = true;
			attackTimer.WaitTime = 1.0f; // 1 second cooldown - adjust as needed
		}

		// Initialize state
		coyoteTimeLeft = 0;
		wallCoyoteTimeLeft = 0;
		currentWallJumpTime = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		UpdatePhysicsState();

		Vector2 velocity = Velocity;

		UpdateTimers((float)delta);
		UpdateCoyoteTime();
		UpdateWallCoyoteTime();

		ApplyGravity(ref velocity, (float)delta);
		HandleWallSlide(ref velocity, (float)delta);
		HandleJumping(ref velocity);
		HandleHorizontalMovement(ref velocity);
		HandleAttackInput(); // Handle attack input
		HandleVisualFlipping();
		HandleStateTransitions(velocity);

		Velocity = velocity;
		MoveAndSlide();
	}

	private void GetInput()
	{
		inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		jumpPressed = Input.IsActionJustPressed("jump");
		jumpReleased = Input.IsActionJustReleased("jump");
		attackPressed = Input.IsActionJustPressed("attack");
	}

	private void UpdatePhysicsState()
	{
		onFloor = IsOnFloor();
		onWall = IsOnWall();
	}

	private void HandleAttackInput()
	{
		if (attackPressed && canAttack)
		{
			PerformAttack();
		}
	}

	private void UpdateTimers(float delta)
	{
		if (currentWallJumpTime > 0)
		{
			currentWallJumpTime = Mathf.Max(0, currentWallJumpTime - delta);
			if (currentWallJumpTime <= 0)
			{
				ResetWallJumpState();
			}
		}

		if (coyoteTimeLeft > 0)
			coyoteTimeLeft = Mathf.Max(0, coyoteTimeLeft - delta);

		if (wallCoyoteTimeLeft > 0)
			wallCoyoteTimeLeft = Mathf.Max(0, wallCoyoteTimeLeft - delta);
	}

	private void UpdateCoyoteTime()
	{
		if (onFloor)
		{
			coyoteTimeLeft = coyoteTimeMax;
			wasOnFloor = true;
			isJumping = false;
		}
		else if (wasOnFloor)
		{
			wasOnFloor = false;
		}
	}

	private void UpdateWallCoyoteTime()
	{
		if (onWall && !onFloor)
		{
			wallCoyoteTimeLeft = wallCoyoteTimeMax;
			lastWallNormal = GetWallNormal().Normalized();
			wasOnWall = true;
		}
		else if (wasOnWall)
		{
			wasOnWall = false;
		}
	}

	private void ApplyGravity(ref Vector2 velocity, float delta)
	{
		if (!onFloor && !onWall)
		{
			velocity += gravity * delta;
		}
	}

	private void HandleWallSlide(ref Vector2 velocity, float delta)
	{
		if (onWall && !onFloor)
		{
			velocity += gravity * 0.5f * delta;

			// Don't change state if attacking
			if (stateMachine?.CurrentState.Name != "Attack")
			{
				stateMachine?.TransitionTo("WallSlide");
			}

			if (inputDirection.X != 0)
			{
				FlipVisuals(inputDirection.X);
				FlipCollision(inputDirection.X);
			}
		}
	}

	private void HandleJumping(ref Vector2 velocity)
	{
		if (CanWallJump() && jumpPressed)
		{
			PerformWallJump(ref velocity);
		}
		else if (CanRegularJump() && jumpPressed)
		{
			PerformRegularJump(ref velocity);
		}

		if (isJumping && jumpReleased && velocity.Y < minJumpVelocity)
		{
			velocity.Y *= jumpCutMultiplier;
		}
	}

	private bool CanWallJump()
	{
		return (onWall || wallCoyoteTimeLeft > 0) && !onFloor;
	}

	private bool CanRegularJump()
	{
		return (onFloor || coyoteTimeLeft > 0) && !CanWallJump();
	}

	private void PerformWallJump(ref Vector2 velocity)
	{
		Vector2 wallNormal = onWall ? GetWallNormal().Normalized() : lastWallNormal;

		velocity.Y = JumpVelocity;
		isJumping = true;

		wallJumpDirection = (int)Mathf.Sign(wallNormal.X);
		wallJumpForce = Speed * wallJumpSpeedMultiplier;
		currentWallJumpTime = wallJumpBoostTime;

		wallCoyoteTimeLeft = 0;

		stateMachine?.TransitionTo("Jump");
	}

	private void PerformRegularJump(ref Vector2 velocity)
	{
		velocity.Y = JumpVelocity;
		coyoteTimeLeft = 0;
		isJumping = true;

		stateMachine?.TransitionTo("Jump");
	}

	private void HandleHorizontalMovement(ref Vector2 velocity)
	{
		// Don't process horizontal movement during attack
		if (stateMachine?.CurrentState.Name == "Attack")
			return;

		if (currentWallJumpTime > 0)
		{
			ApplyWallJumpMovement(ref velocity);
		}
		else if (inputDirection.X != 0)
		{
			velocity.X = inputDirection.X * Speed;

			if (onFloor && !isJumping)
			{
				stateMachine?.TransitionTo("Run");
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, Speed);

			if (onFloor && !isJumping)
			{
				stateMachine?.TransitionTo("Idle");
			}
		}
	}

	private void ApplyWallJumpMovement(ref Vector2 velocity)
	{
		float wallJumpInfluence = currentWallJumpTime / wallJumpBoostTime;
		float playerInfluence = wallJumpPlayerInfluence + (1.0f - wallJumpPlayerInfluence) * (1.0f - wallJumpInfluence);

		velocity.X = (wallJumpDirection * wallJumpForce * wallJumpInfluence) + 
					 (inputDirection.X * Speed * playerInfluence);
	}

	private void HandleVisualFlipping()
	{
		if (inputDirection.X != 0)
		{
			FlipVisuals(inputDirection.X);
			FlipCollision(inputDirection.X);
		}
	}

	private void FlipVisuals(float direction)
	{
		if (VisualsNode == null) return;

		float absScaleX = Mathf.Abs(VisualsNode.Scale.X);
		if (absScaleX == 0) absScaleX = 1.0f;

		float newScaleX = direction < 0 ? -absScaleX : absScaleX;
		VisualsNode.Scale = new Vector2(newScaleX, VisualsNode.Scale.Y);
	}

	private void FlipCollision(float direction)
	{
		if (CollisionNode == null) return;

		float absPosX = Mathf.Abs(CollisionNode.Position.X);
		if (absPosX == 0) absPosX = 1.0f;

		float newPosX = direction < 0 ? absPosX : -absPosX;
		CollisionNode.Position = new Vector2(newPosX, CollisionNode.Position.Y);
	}

	private void HandleStateTransitions(Vector2 velocity)
	{
		// Don't change states if we're currently attacking - THIS IS CRUCIAL!
		if (stateMachine?.CurrentState.Name == "Attack")
			return;

		if (!onFloor && velocity.Y > 0 && !onWall)
		{
			stateMachine?.TransitionTo("Fall");
		}
	}

	private void ResetWallJumpState()
	{
		wallJumpForce = 0;
		wallJumpDirection = 0;
	}

	private void PerformAttack()
	{
		if (!canAttack) return;

		canAttack = false;

		// Transition to attack state
		stateMachine?.TransitionTo("Attack");

		// Start the cooldown timer
		attackTimer.Start();

		GD.Print("Attack performed - cooldown started");
	}

	// THIS IS THE KEY METHOD THAT WAS MISSING!
	private void OnAttackTimerTimeout()
	{
		canAttack = true;
		GD.Print("Attack cooldown finished - can attack again");
	}
}
