using Godot;
using System;

public partial class PlayerMovement : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -450.0f;
	[Export] private float jumpCutMultiplier = 0.4f;
	[Export] private float minJumpVelocity = -150.0f;
	[Export] private float wallJumpBoostTime = 0.3f;
	[Export] private float wallJumpSpeedMultiplier = 1.5f;
	[Export] private float wallJumpPlayerInfluence = 0.2f;
	[Export] private float coyoteTimeMax = 0.15f;
	[Export] private float wallCoyoteTimeMax = 0.1f;
	private bool isJumping = false;
	private bool wasOnFloor = false;
	private bool wasOnWall = false;
	private float currentWallJumpTime = 0f;
	private float coyoteTimeLeft = 0f;
	private float wallCoyoteTimeLeft = 0f;
	private float wallJumpForce = 0f;
	private int wallJumpDirection = 0;
	private Vector2 lastWallNormal = Vector2.Zero;
	[Export] public StateMachine stateMachine;
	[Export] public Node2D VisualsNode { get; set; }
	[Export] public Node2D CollisionNode { get; set; }
	private Vector2 inputDirection;
	private bool jumpPressed;
	private bool jumpReleased;
	private bool onFloor;
	private bool onWall;
	private Vector2 gravity;
	
	[Export]
	private Timer attackTimer;
	private bool attackPressed;
	
	private bool canAttack = true;

	public override void _PhysicsProcess(double delta)
	{
		float deltaFloat = (float)delta;
		
		
		CacheInputAndPhysicsState();
		if (attackPressed && canAttack)
		{
			GD.Print("Attack pressed");
			PerformAttack();
		}
		
		Vector2 velocity = Velocity;
		
		UpdateTimers(deltaFloat);
		
		UpdateCoyoteTime();
		UpdateWallCoyoteTime();
		
		ApplyGravity(ref velocity, deltaFloat);
		
		HandleWallSlide(ref velocity, deltaFloat);
		
		HandleJumping(ref velocity);
		
		HandleHorizontalMovement(ref velocity);
		
		HandleVisualFlipping();
		
		HandleStateTransitions(velocity);
		
		Velocity = velocity;
		MoveAndSlide();
		
	}
	
	private void CacheInputAndPhysicsState()
	{
		inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		jumpPressed = Input.IsActionJustPressed("ui_accept");
		jumpReleased = Input.IsActionJustReleased("ui_accept");
		attackPressed = Input.IsActionJustPressed("attack");
		
		onFloor = IsOnFloor();
		onWall = IsOnWall();
		gravity = GetGravity();
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
			stateMachine?.TransitionTo("WallSlide");
			
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
		if (attackTimer.TimeLeft != 0) return;
		
		canAttack = false;
		
		// Transition to attack state
		stateMachine?.TransitionTo("Attack");

		attackTimer.Start();
	}
}
