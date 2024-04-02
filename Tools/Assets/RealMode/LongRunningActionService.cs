using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace RealMode
{
    public class ActionScope : IDisposable
    {
        public float Completeness { get; set; }
        public string Name { get; set; }

        private readonly Action<ActionScope> _action;
        private bool _disposed;

        public ActionScope(Action<ActionScope> action)
        {
            Name = string.Empty;
            _action = action;
            _disposed = false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _action(this);
            }
        }
    }

    public class LongRunningActionService : MonoBehaviour
    {
        private ConcurrentDictionary<ActionScope, ActionScope> _runningActions = new ConcurrentDictionary<ActionScope, ActionScope>();

        public delegate void ActionsUpdatedEventHandler(LongRunningActionService sender);

        public event ActionsUpdatedEventHandler? OnActionsUpdated;

        private bool _shouldTriggerEvent = false;

        private void Awake()
        {
            _runningActions = new ConcurrentDictionary<ActionScope, ActionScope>();
        }

        public (float completeness, string name)? GetTopRunningAction()
        {
            var scope = _runningActions.FirstOrDefault();
            if (scope.Value != null)
            {
                return (scope.Value.Completeness, scope.Value.Name);
            }
            else
            {
                return null;
            }
        }

        public IDisposable OpenNewLongRunningActionScope(string name)
        {
            var scope = new ActionScope(RemoveLongRunningActionScope)
            {
                Name = name,
                Completeness = 0
            };
            _shouldTriggerEvent = true;
            return scope;
        }

        private void RemoveLongRunningActionScope(ActionScope scope)
        {
            _runningActions.TryRemove(scope, out _);
            _shouldTriggerEvent = true;
        }

        private void Update()
        {
            if (_shouldTriggerEvent)
            {
                _shouldTriggerEvent = false;
                OnActionsUpdated?.Invoke(this);
            }
        }
    }
}