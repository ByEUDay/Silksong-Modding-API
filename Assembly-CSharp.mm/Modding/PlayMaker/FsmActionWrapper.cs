#if !NO_FSMUTIL
using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Modding.PlayMakerSupport
{



    public class FsmActionWrapper : FsmStateAction
    {
        private Action<FsmStateAction> _onEnter;
        private Action<FsmStateAction> _onUpdate;
        private Action<FsmStateAction> _onFixedUpdate;
        private Action<FsmStateAction> _onLateUpdate;
        private Action<FsmStateAction> _onExit;

        public FsmActionWrapper()
        {
        }

        public FsmActionWrapper(Action<FsmStateAction> onEnter = null, Action<FsmStateAction> onUpdate = null,
            Action<FsmStateAction> onExit = null)
        {
            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        public override void OnEnter()
        {
            try
            {
                _onEnter?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmActionWrapper.OnEnter: {e}");
            }
        }

        public override void OnUpdate()
        {
            try
            {
                _onUpdate?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmActionWrapper.OnUpdate: {e}");
            }
        }

        public override void OnFixedUpdate()
        {
            try
            {
                _onFixedUpdate?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmActionWrapper.OnFixedUpdate: {e}");
            }
        }

        public override void OnLateUpdate()
        {
            try
            {
                _onLateUpdate?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmActionWrapper.OnLateUpdate: {e}");
            }
        }

        public override void OnExit()
        {
            try
            {
                _onExit?.Invoke(this);
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmActionWrapper.OnExit: {e}");
            }
        }

        public void SetOnEnter(Action<FsmStateAction> action) => _onEnter = action;
        public void SetOnUpdate(Action<FsmStateAction> action) => _onUpdate = action;
        public void SetOnFixedUpdate(Action<FsmStateAction> action) => _onFixedUpdate = action;
        public void SetOnLateUpdate(Action<FsmStateAction> action) => _onLateUpdate = action;
        public void SetOnExit(Action<FsmStateAction> action) => _onExit = action;
    }




    public class FsmConditionalAction : FsmStateAction
    {
        public FsmBool Condition;
        public FsmStateAction TrueAction;
        public FsmStateAction FalseAction;

        public override void OnEnter()
        {
            try
            {
                if (Condition != null && Condition.Value)
                {
                    TrueAction?.OnEnter();
                }
                else
                {
                    FalseAction?.OnEnter();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmConditionalAction.OnEnter: {e}");
            }
        }

        public override void OnUpdate()
        {
            try
            {
                if (Condition != null && Condition.Value)
                {
                    TrueAction?.OnUpdate();
                }
                else
                {
                    FalseAction?.OnUpdate();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmConditionalAction.OnUpdate: {e}");
            }
        }

        public override void OnExit()
        {
            try
            {
                if (Condition != null && Condition.Value)
                {
                    TrueAction?.OnExit();
                }
                else
                {
                    FalseAction?.OnExit();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmConditionalAction.OnExit: {e}");
            }
        }
    }




    public class FsmDelayedAction : FsmStateAction
    {
        public FsmFloat Delay;
        public FsmStateAction Action;
        private float _timer;

        public override void OnEnter()
        {
            _timer = Delay != null ? Delay.Value : 0f;
        }

        public override void OnUpdate()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                try
                {
                    Action?.OnEnter();
                }
                catch (Exception e)
                {
                    Logger.LogError($"FsmDelayedAction.OnUpdate: {e}");
                }
                Finish();
            }
        }
    }




    public class FsmLoopAction : FsmStateAction
    {
        public FsmInt LoopCount;
        public FsmStateAction Action;
        private int _currentLoop;

        public override void OnEnter()
        {
            _currentLoop = 0;
        }

        public override void OnUpdate()
        {
            int maxLoops = LoopCount != null ? LoopCount.Value : 1;
            if (_currentLoop < maxLoops)
            {
                try
                {
                    Action?.OnUpdate();
                }
                catch (Exception e)
                {
                    Logger.LogError($"FsmLoopAction.OnUpdate: {e}");
                }
                _currentLoop++;
            }
            else
            {
                Finish();
            }
        }
    }




    public class FsmWaitForEventAction : FsmStateAction
    {
        public FsmString EventName;
        public FsmFloat Timeout;
        private float _timer;
        private bool _eventReceived;

        public override void OnEnter()
        {
            _timer = Timeout != null ? Timeout.Value : -1f;
            _eventReceived = false;
        }

        public override void OnUpdate()
        {
            if (_eventReceived)
            {
                Finish();
                return;
            }

            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    Finish();
                }
            }
        }

        public void TriggerEvent()
        {
            _eventReceived = true;
        }
    }




    public class FsmRandomAction : FsmStateAction
    {
        public FsmFloat Probability;
        public FsmStateAction TrueAction;
        public FsmStateAction FalseAction;

        public override void OnEnter()
        {
            try
            {
                float prob = Probability != null ? Probability.Value : 0.5f;
                if (UnityEngine.Random.value < prob)
                {
                    TrueAction?.OnEnter();
                }
                else
                {
                    FalseAction?.OnEnter();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"FsmRandomAction.OnEnter: {e}");
            }
        }
    }
}
#endif
