using System;
using System.Collections.Generic;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.Locator;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Signals;

public class SignalService : IService, IDisposable
{
	private readonly struct SignalState
	{
		public static readonly SignalState Empty;

		public readonly NetPlayerGroup PlayerGroup;

		public readonly bool IsSentByLocalPlayer;

		private SignalState(NetPlayerGroup value, bool sentByLocalPlayer)
		{
			PlayerGroup = value;
			IsSentByLocalPlayer = sentByLocalPlayer;
		}

		public SignalState SetSentByLocalPlayer()
		{
			return new SignalState(PlayerGroup, sentByLocalPlayer: true);
		}

		public SignalState Add(NetPlayer player)
		{
			return new SignalState(PlayerGroup.Add(player), IsSentByLocalPlayer);
		}
	}

	public class SignalServiceState : IHashable
	{
		[JsonProperty(PropertyName = "s")]
		public readonly List<uint> Signals = new List<uint>();

		[JsonProperty(PropertyName = "n")]
		public uint SignalKeyCounter;

		public SignalServiceState Refresh(SignalService signalService)
		{
			Signals.Clear();
			Signals.IncreaseCapacity(signalService.m_SignalStates.Count);
			foreach (KeyValuePair<uint, SignalState> signalState in signalService.m_SignalStates)
			{
				Signals.Add(signalState.Key);
			}
			Signals.Sort();
			SignalKeyCounter = signalService.SignalKeyCounter;
			return this;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			List<uint> signals = Signals;
			if (signals != null)
			{
				for (int i = 0; i < signals.Count; i++)
				{
					uint obj = signals[i];
					Hash128 val = UnmanagedHasher<uint>.GetHash128(ref obj);
					result.Append(ref val);
				}
			}
			result.Append(ref SignalKeyCounter);
			return result;
		}
	}

	private static ServiceProxy<SignalService> s_InstanceProxy;

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SignalService");

	private readonly Dictionary<uint, SignalState> m_SignalStates = new Dictionary<uint, SignalState>();

	private readonly SignalServiceState m_State = new SignalServiceState();

	public static SignalService Instance
	{
		get
		{
			if (s_InstanceProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new SignalService());
				s_InstanceProxy = Services.GetProxy<SignalService>();
			}
			return s_InstanceProxy.Instance;
		}
	}

	private static NetPlayerGroup PlayersReadyMask => NetworkingManager.PlayersReadyMask;

	private static GameCommandQueue GameCommandQueue => Game.Instance.GameCommandQueue;

	public int SignalsCount => m_SignalStates.Count;

	public uint SignalKeyCounter { get; private set; }

	ServiceLifetimeType IService.Lifetime => ServiceLifetimeType.GameSession;

	public SignalServiceState State => m_State.Refresh(this);

	void IDisposable.Dispose()
	{
		m_SignalStates.Clear();
		SignalKeyCounter = 0u;
	}

	public SignalWrapper RegisterNext()
	{
		SignalKeyCounter = SignalKeyCounter % uint.MaxValue + 1;
		if (m_SignalStates.ContainsKey(SignalKeyCounter))
		{
			throw new Exception($"Signal {SignalKeyCounter} already registered");
		}
		m_SignalStates.Add(SignalKeyCounter, SignalState.Empty);
		return new SignalWrapper(SignalKeyCounter);
	}

	public bool GetProgress(SignalWrapper signalWrapper, out int current, out int target, out NetPlayerGroup playerGroup)
	{
		if (signalWrapper.IsEmpty || !m_SignalStates.TryGetValue(signalWrapper.Key, out var value))
		{
			current = 0;
			target = 0;
			playerGroup = default(NetPlayerGroup);
			return false;
		}
		NetPlayerGroup playersReadyMask = PlayersReadyMask;
		NetPlayerGroup netPlayerGroup = value.PlayerGroup.Intersection(playersReadyMask);
		current = netPlayerGroup.Count();
		target = Mathf.Max(1, playersReadyMask.Count());
		playerGroup = netPlayerGroup;
		return true;
	}

	private SignalState GetState(uint key)
	{
		if (!m_SignalStates.TryGetValue(key, out var value))
		{
			throw new Exception($"Signal {key} not registered");
		}
		return value;
	}

	public void Write(uint key, NetPlayer player)
	{
		SignalState value = GetState(key).Add(player);
		m_SignalStates[key] = value;
	}

	public void UpdateReadyState(uint key)
	{
		if (m_SignalStates.TryGetValue(key, out var value) && value.PlayerGroup.Contains(PlayersReadyMask))
		{
			m_SignalStates.Remove(key);
		}
	}

	public bool CheckReady(ref SignalWrapper signalWrapper, bool emptyIsOk = false)
	{
		SignalState state;
		return CheckReady(ref signalWrapper, out state, emptyIsOk);
	}

	private bool CheckReady(ref SignalWrapper signalWrapper, out SignalState state, bool emptyIsOk = false)
	{
		if (signalWrapper.IsEmpty)
		{
			if (emptyIsOk)
			{
				state = default(SignalState);
				return true;
			}
			Logger.Error("SignalWrapper is empty!");
		}
		uint key = signalWrapper.Key;
		UpdateReadyState(key);
		if (!m_SignalStates.TryGetValue(key, out state))
		{
			signalWrapper = SignalWrapper.Empty;
			return true;
		}
		return false;
	}

	public bool CheckReadyOrSend(ref SignalWrapper signalWrapper, bool emptyIsOk = false)
	{
		if (CheckReady(ref signalWrapper, out var state, emptyIsOk))
		{
			return true;
		}
		if (state.IsSentByLocalPlayer)
		{
			return false;
		}
		uint key = signalWrapper.Key;
		GameCommandQueue.AddCommand(new SignalGameCommand(key));
		m_SignalStates[key] = state.SetSentByLocalPlayer();
		return false;
	}
}
