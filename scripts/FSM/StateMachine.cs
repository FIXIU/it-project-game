using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{
    [Export] public NodePath initialState;

    private Dictionary<string, State> _states;
    private State _currentState;
    
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
        
        _currentState = GetNode<State>(initialState);
        _currentState.Enter();
    }
    
    public override void _Process(double delta)
    {
        _currentState.Update((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState.PhysicsUpdate((float)delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        _currentState.HandleInput(@event);
    }
    
    public void TransitionTo(string stateName)
    {
        if (!_states.ContainsKey(stateName) || _currentState == _states[stateName])
            return;
        
        _currentState.Exit();
        _currentState = _states[stateName];
        _currentState.Enter();
    }
}
