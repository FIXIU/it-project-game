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
            if (node is State state)
            {
                _states[node.Name] = state;
                state.fsm = this;
                state.Ready();
                state.Exit(); // reset all states
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
    }

    public override void _Process(double delta)
    {
        if (_currentState != null)
        {
            _currentState.Update((float)delta);
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
        if (!_states.ContainsKey(stateName) || _currentState == _states[stateName])
            return;

        if (_currentState != null)
        {
            _currentState.Exit();
        }
        _currentState = _states[stateName];
        _currentState.Enter();
    }
}