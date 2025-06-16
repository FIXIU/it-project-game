using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{
    [Export] public NodePath initialState;
    private Dictionary<string, State> _states;
    private State _currentState;
    public State CurrentState => _currentState;
    public override void _Ready()
    {
        _states = new Dictionary<string, State>();
        foreach (Node node in GetChildren())
        {
            GD.Print(node.Name);
            if (node is State state)
            {
                GD.Print(state.Name + " is a State");
                _states[node.Name] = state;
                state.fsm = this;
                GD.Print("Trying to ready " + state.Name);
                state.Ready();
                GD.Print("Trying to exit " + state.Name);
                state.Exit();
                GD.Print("Exitted " + state.Name);
                GD.Print(state.Name);
            }
        }
        if (initialState != null && GetNode(initialState) is State initialNodeState)
        {
            _currentState = initialNodeState;
            _currentState.Enter();
        }
        else
        {
            GD.PrintErr($"Initial state not set or not found for StateMachine: {GetPath()}");
        }

        foreach (var state in _states)
        {
            GD.Print(state);
        }
    }
    public override void _Process(double delta)
    {
        if (_currentState != null)
        {
            // Debug: Print current state periodically (every 60 frames ≈ 1 second)
            if (Engine.GetProcessFrames() % 60 == 0)
            {
                GD.Print($"StateMachine: Current state is '{_currentState.Name}', calling Update()");
            }
            _currentState.Update(delta);
        }
        else
        {
            if (Engine.GetProcessFrames() % 60 == 0)
            {
                GD.PrintErr("StateMachine: No current state!");
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (_currentState != null)
        {
            _currentState.PhysicsUpdate((float)delta);
        }
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (_currentState != null)
        {
            _currentState.HandleInput(@event);
        }
    }
    public void TransitionTo(string stateName)
    {
        
        if (!_states.ContainsKey(stateName))
        {
            GD.PrintErr($"StateMachine: State '{stateName}' not found!");
            return;
        }
        
        if (_currentState == _states[stateName])
        {
            return;
        }
        
        if (_currentState != null)
        {
            _currentState.Exit();
        }
        
        _currentState = _states[stateName];
        _currentState.Enter();
    }
    
    public void ListAllStates()
    {
        GD.Print("Current States in StateMachine:");
        foreach (var state in _states)
        {
            GD.Print(state);
        }
    }
}

