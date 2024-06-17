using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kingmaker.Utility.Fsm;

public class StateMachine<TState, TTrigger> where TState : Enum where TTrigger : Enum
{
	private readonly struct QueuedTrigger
	{
		public readonly TTrigger Trigger;

		public readonly object Payload;

		public QueuedTrigger(TTrigger trigger, object payload)
		{
			Trigger = trigger;
			Payload = payload;
		}
	}

	public class StateConfiguration
	{
		public readonly struct TransitionInfo
		{
			public readonly TState State;

			public readonly string UmlDrawInfo;

			public TransitionInfo(TState state, string umlDrawInfo)
			{
				State = state;
				UmlDrawInfo = umlDrawInfo;
			}
		}

		public readonly TState State;

		public readonly string UmlDrawInfo;

		private Func<object, IStateAsync> m_Factory;

		public Dictionary<TTrigger, TransitionInfo> Transitions { get; } = new Dictionary<TTrigger, TransitionInfo>();


		public List<TTrigger> IgnoreTriggers { get; } = new List<TTrigger>();


		public StateConfiguration(TState state, string umlDrawInfo = null)
		{
			State = state;
			UmlDrawInfo = umlDrawInfo;
		}

		public StateConfiguration Permit(TTrigger trigger, TState newState, string umlDrawInfo = null)
		{
			if (Transitions.ContainsKey(trigger))
			{
				throw new ArgumentException($"Adding duplicate of {trigger}");
			}
			if (IgnoreTriggers.Contains(trigger))
			{
				throw new ArgumentException($"Trigger already ignored {trigger}");
			}
			Transitions.Add(trigger, new TransitionInfo(newState, umlDrawInfo));
			return this;
		}

		public StateConfiguration Ignore(TTrigger trigger)
		{
			if (IgnoreTriggers.Contains(trigger))
			{
				throw new ArgumentException($"Adding duplicate of {trigger}");
			}
			if (Transitions.ContainsKey(trigger))
			{
				throw new ArgumentException($"Trigger already permitted {trigger}");
			}
			IgnoreTriggers.Add(trigger);
			return this;
		}

		public StateConfiguration SetStateFactory(Func<object, IStateAsync> factory)
		{
			if (m_Factory != null)
			{
				throw new ArgumentException("Factory already initialized");
			}
			m_Factory = factory;
			return this;
		}

		public IStateAsync Create([CanBeNull] object payload)
		{
			return m_Factory?.Invoke(payload);
		}
	}

	public interface IStateMachineEventsHandler
	{
		void OnFireTrigger(TTrigger trigger);

		void OnStateChanged(TState oldState, TState newState);

		void OnProcessTrigger(TTrigger trigger, TState currentState, TState nextState);

		void OnFireException(Exception exception);

		void OnUnhandledTransition(TTrigger trigger, TState currentState);

		void OnIgnoreTrigger(TTrigger trigger, TState currentState);
	}

	private class WaitingState
	{
		private struct CallbackData
		{
			public readonly TaskCompletionSource<bool> Tcs;

			public readonly TState State;

			public readonly bool OnlyNextState;

			public CallbackData(TState state, bool onlyNextState)
			{
				Tcs = new TaskCompletionSource<bool>();
				State = state;
				OnlyNextState = onlyNextState;
			}
		}

		private readonly List<CallbackData> m_NewStateCallbacks = new List<CallbackData>();

		public async Task WaitState(TState state, bool onlyNextState, CancellationToken cancellationToken)
		{
			CallbackData item = new CallbackData(state, onlyNextState);
			m_NewStateCallbacks.Add(item);
			await using (cancellationToken.CanBeCanceled ? cancellationToken.Register(OnCancelled, item.Tcs) : default(CancellationTokenRegistration))
			{
				await item.Tcs.Task.ConfigureAwait(continueOnCapturedContext: false);
			}
		}

		public void OnStateChanged(TState newState)
		{
			for (int i = 0; i < m_NewStateCallbacks.Count; i++)
			{
				CallbackData callbackData = m_NewStateCallbacks[i];
				bool flag = false;
				if (callbackData.State.Equals(newState))
				{
					callbackData.Tcs.TrySetResult(result: true);
					flag = true;
				}
				else if (callbackData.OnlyNextState)
				{
					callbackData.Tcs.TrySetException(new CantReachTargetStateException(callbackData.State, newState));
					flag = true;
				}
				if (flag)
				{
					m_NewStateCallbacks.RemoveAt(i);
					i--;
				}
			}
		}

		private void OnCancelled(object tcs)
		{
			if (!(tcs is TaskCompletionSource<bool> taskCompletionSource))
			{
				return;
			}
			for (int i = 0; i < m_NewStateCallbacks.Count; i++)
			{
				if (m_NewStateCallbacks[i].Tcs == taskCompletionSource)
				{
					m_NewStateCallbacks.RemoveAt(i);
					break;
				}
			}
			taskCompletionSource.TrySetCanceled();
		}
	}

	private const bool RetainContext = false;

	private readonly Dictionary<TState, StateConfiguration> m_StateConfigurations = new Dictionary<TState, StateConfiguration>();

	private bool m_Started;

	private bool m_InTransition;

	private readonly Queue<QueuedTrigger> m_TriggersQueue = new Queue<QueuedTrigger>();

	private readonly WaitingState m_WaitingState = new WaitingState();

	private IStateMachineEventsHandler m_EventsHandler;

	public TState CurrentState { get; private set; }

	private IStateAsync State { get; set; }

	private StateConfiguration CurrentStateConfiguration => m_StateConfigurations[CurrentState];

	public StateConfiguration Configure(TState state, string umlDrawInfo = null)
	{
		if (m_Started)
		{
			throw new InvalidOperationException("Configure not possible after Start");
		}
		if (m_StateConfigurations.ContainsKey(state))
		{
			throw new ArgumentException($"Adding duplicate of {state}");
		}
		StateConfiguration stateConfiguration = new StateConfiguration(state, umlDrawInfo);
		m_StateConfigurations.Add(state, stateConfiguration);
		return stateConfiguration;
	}

	public Task Start(TState state, object payload = null)
	{
		if (m_Started)
		{
			throw new InvalidOperationException("Already started");
		}
		m_Started = true;
		return SetState(state, payload);
	}

	public async void Fire(TTrigger trigger, object payload = null)
	{
		try
		{
			await FireAsync(trigger, payload).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception)
		{
			m_EventsHandler.OnFireException(exception);
		}
	}

	private Task FireAsync(TTrigger trigger, object payload = null)
	{
		if (!m_Started)
		{
			throw new InvalidOperationException("Not started yet");
		}
		m_EventsHandler?.OnFireTrigger(trigger);
		m_TriggersQueue.Enqueue(new QueuedTrigger(trigger, payload));
		return ProcessQueue();
	}

	public void SetEventsHandler(IStateMachineEventsHandler handler)
	{
		m_EventsHandler = handler;
	}

	private async Task ProcessQueue()
	{
		if (m_InTransition)
		{
			return;
		}
		try
		{
			m_InTransition = true;
			while (m_TriggersQueue.Count != 0)
			{
				QueuedTrigger queuedTrigger = m_TriggersQueue.Dequeue();
				if (CurrentStateConfiguration.IgnoreTriggers.Contains(queuedTrigger.Trigger))
				{
					m_EventsHandler.OnIgnoreTrigger(queuedTrigger.Trigger, CurrentState);
					continue;
				}
				if (!CurrentStateConfiguration.Transitions.TryGetValue(queuedTrigger.Trigger, out var value))
				{
					m_EventsHandler.OnUnhandledTransition(queuedTrigger.Trigger, CurrentState);
					continue;
				}
				m_EventsHandler.OnProcessTrigger(queuedTrigger.Trigger, CurrentState, value.State);
				await SetState(value.State, queuedTrigger.Payload).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			m_InTransition = false;
		}
	}

	public Task WaitState(TState state, CancellationToken cancellationToken = default(CancellationToken))
	{
		return WaitState(state, onlyNextState: false, cancellationToken);
	}

	public Task WaitNextState(TState nextState, CancellationToken cancellationToken = default(CancellationToken))
	{
		return WaitState(nextState, onlyNextState: true, cancellationToken);
	}

	private Task WaitState(TState nextState, bool onlyNextState, CancellationToken cancellationToken)
	{
		if (CurrentState.Equals(nextState))
		{
			return Task.CompletedTask;
		}
		if (cancellationToken.IsCancellationRequested)
		{
			throw new OperationCanceledException(cancellationToken);
		}
		return m_WaitingState.WaitState(nextState, onlyNextState, cancellationToken);
	}

	private async Task SetState(TState newState, object payload)
	{
		if (State != null)
		{
			await State.OnExit().ConfigureAwait(continueOnCapturedContext: false);
		}
		if (!m_StateConfigurations.TryGetValue(newState, out var value))
		{
			throw new InvalidOperationException($"Has no configuration for state {newState}");
		}
		TState currentState = CurrentState;
		CurrentState = newState;
		m_EventsHandler.OnStateChanged(currentState, newState);
		m_WaitingState.OnStateChanged(newState);
		State = value.Create(payload);
		if (State != null)
		{
			await State.OnEnter().ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
