#if !NO_FSMUTIL
using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using Modding.PlayMakerSupport;
using UnityEngine;

namespace Modding.PlayMaker
{



    public class FsmBuilder
    {
        private readonly PlayMakerFSM _fsm;
        private FsmState _currentState;

        private FsmBuilder(PlayMakerFSM fsm)
        {
            _fsm = fsm;
        }

        public static FsmBuilder Create(string name, GameObject owner)
        {
            var fsmComponent = owner.AddComponent<PlayMakerFSM>();
            fsmComponent.FsmName = name;
            return new FsmBuilder(fsmComponent);
        }

        public static FsmBuilder Wrap(PlayMakerFSM fsm)
        {
            return new FsmBuilder(fsm);
        }

        public FsmBuilder AddState(string stateName, Action<FsmState> configure = null)
        {
            var states = _fsm.Fsm.States.ToList();
            var state = new FsmState(_fsm.Fsm)
            {
                Name = stateName
            };
            states.Add(state);
            _fsm.Fsm.States = states.ToArray();
            configure?.Invoke(state);
            _currentState = state;
            return this;
        }

        public FsmBuilder SetInitialState(string stateName)
        {
            _fsm.Fsm.StartState = stateName;
            return this;
        }

        public FsmBuilder InState(string stateName, Action<FsmState> configure)
        {
            var state = _fsm.Fsm.States.FirstOrDefault(s => s.Name == stateName);
            if (state != null)
            {
                configure(state);
                _currentState = state;
            }
            return this;
        }

        public FsmBuilder AddAction(FsmStateAction action)
        {
            if (_currentState != null)
                _currentState.AddAction(action);
            return this;
        }

        public FsmBuilder AddTransition(string fromState, string eventName, string toState)
        {
            var state = _fsm.Fsm.States.FirstOrDefault(s => s.Name == fromState);
            if (state != null)
            {
                var transitions = state.Transitions.ToList();
                transitions.Add(new FsmTransition
                {
                    FsmEvent = FsmEvent.GetFsmEvent(eventName),
                    ToFsmState = _fsm.Fsm.States.FirstOrDefault(s => s.Name == toState),
                    ToState = toState
                });
                state.Transitions = transitions.ToArray();
            }
            return this;
        }

        public FsmBuilder AddGlobalTransition(string eventName, string toState)
        {
            var globalTransitions = _fsm.Fsm.GlobalTransitions?.ToList() ?? new List<FsmTransition>();
            globalTransitions.Add(new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToFsmState = _fsm.Fsm.States.FirstOrDefault(s => s.Name == toState),
                ToState = toState
            });
            _fsm.Fsm.GlobalTransitions = globalTransitions.ToArray();
            return this;
        }

        public FsmBuilder SetVariable<T>(string variableName, T value)
        {
            var variable = FsmQueryUtils.FindVariable(_fsm.Fsm, variableName);
            if (variable != null)
            {
                FsmQueryUtils.SetVariableValue(_fsm.Fsm, variableName, value);
            }
            return this;
        }

        public FsmBuilder AddFloatVariable(string name, float defaultValue = 0f)
        {
            var variables = _fsm.Fsm.Variables.FloatVariables.ToList();
            variables.Add(new FsmFloat { Name = name, Value = defaultValue });
            _fsm.Fsm.Variables.FloatVariables = variables.ToArray();
            return this;
        }

        public FsmBuilder AddIntVariable(string name, int defaultValue = 0)
        {
            var variables = _fsm.Fsm.Variables.IntVariables.ToList();
            variables.Add(new FsmInt { Name = name, Value = defaultValue });
            _fsm.Fsm.Variables.IntVariables = variables.ToArray();
            return this;
        }

        public FsmBuilder AddBoolVariable(string name, bool defaultValue = false)
        {
            var variables = _fsm.Fsm.Variables.BoolVariables.ToList();
            variables.Add(new FsmBool { Name = name, Value = defaultValue });
            _fsm.Fsm.Variables.BoolVariables = variables.ToArray();
            return this;
        }

        public FsmBuilder AddStringVariable(string name, string defaultValue = "")
        {
            var variables = _fsm.Fsm.Variables.StringVariables.ToList();
            variables.Add(new FsmString { Name = name, Value = defaultValue });
            _fsm.Fsm.Variables.StringVariables = variables.ToArray();
            return this;
        }

        public FsmBuilder AddGameObjectVariable(string name, GameObject defaultValue = null)
        {
            var variables = _fsm.Fsm.Variables.GameObjectVariables.ToList();
            variables.Add(new FsmGameObject { Name = name, Value = defaultValue });
            _fsm.Fsm.Variables.GameObjectVariables = variables.ToArray();
            return this;
        }

        public FsmBuilder OnEnter(Action action)
        {
            if (_currentState != null)
                _currentState.AddAction(new FsmActionWrapper(onEnter: _ => action()));
            return this;
        }

        public FsmBuilder OnExit(Action action)
        {
            if (_currentState != null)
            {
                var exitAction = new FsmActionWrapper(onExit: _ => action());

            }
            return this;
        }

        public FsmBuilder OnUpdate(Action action)
        {
            if (_currentState != null)
                _currentState.AddAction(new FsmActionWrapper(onUpdate: _ => action()));
            return this;
        }

        public FsmBuilder If(Func<bool> condition, Action<FsmState> trueBranch, Action<FsmState> falseBranch = null)
        {
            if (_currentState != null)
            {
                var condAction = new FsmConditionalAction();
                condAction.Condition = new FsmBool { Value = true };
                condAction.TrueAction = new FsmActionWrapper(onEnter: _ => trueBranch(_currentState));
                condAction.FalseAction = falseBranch != null
                    ? new FsmActionWrapper(onEnter: _ => falseBranch(_currentState))
                    : null;
                _currentState.AddAction(condAction);
            }
            return this;
        }

        public FsmBuilder Wait(float seconds, string finishEvent)
        {
            if (_currentState != null)
            {
                var waitAction = new FsmDelayedAction();
                waitAction.Delay = new FsmFloat { Value = seconds };
                waitAction.Action = new FsmActionWrapper(onEnter: _ => { });
                _currentState.AddAction(waitAction);
            }
            return this;
        }

        public FsmBuilder Loop(int count, Action<FsmState> body)
        {
            if (_currentState != null)
            {
                var loopAction = new FsmLoopAction();
                loopAction.LoopCount = new FsmInt { Value = count };
                loopAction.Action = new FsmActionWrapper(onUpdate: _ => body(_currentState));
                _currentState.AddAction(loopAction);
            }
            return this;
        }

        public FsmBuilder SendEvent(string eventName)
        {
            if (_currentState != null)
            {
                var eventAction = new FsmActionWrapper(onEnter: _ => _fsm.SendEvent(eventName));
                _currentState.AddAction(eventAction);
            }
            return this;
        }

        public FsmBuilder Log(string message)
        {
            if (_currentState != null)
            {
                var logAction = new FsmActionWrapper(onEnter: _ => Logger.Log($"[FSM:{_fsm.FsmName}] {message}"));
                _currentState.AddAction(logAction);
            }
            return this;
        }

        public PlayMakerFSM Build()
        {
            return _fsm;
        }
    }




    public static class FsmStateBuilderExtensions
    {
        public static FsmState InsertActionExt(this FsmState state, int index, FsmStateAction action)
        {
            var actions = state.Actions;
            var newActions = new FsmStateAction[actions.Length + 1];
            Array.Copy(actions, 0, newActions, 0, index);
            newActions[index] = action;
            Array.Copy(actions, index, newActions, index + 1, actions.Length - index);
            state.Actions = newActions;
            return state;
        }

        public static FsmState RemoveActionExt(this FsmState state, int index)
        {
            var actions = state.Actions;
            if (index < 0 || index >= actions.Length) return state;
            var newActions = new FsmStateAction[actions.Length - 1];
            Array.Copy(actions, 0, newActions, 0, index);
            Array.Copy(actions, index + 1, newActions, index, actions.Length - index - 1);
            state.Actions = newActions;
            return state;
        }

        public static FsmState ClearActionsExt(this FsmState state)
        {
            state.Actions = new FsmStateAction[0];
            return state;
        }

        public static FsmState AddTransition(this FsmState state, string eventName, string toState)
        {
            var transitions = state.Transitions;
            var newTransitions = new FsmTransition[transitions.Length + 1];
            Array.Copy(transitions, newTransitions, transitions.Length);
            newTransitions[transitions.Length] = new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToFsmState = state.Fsm.States.FirstOrDefault(s => s.Name == toState),
                ToState = toState
            };
            state.Transitions = newTransitions;
            return state;
        }
    }
}
#endif
