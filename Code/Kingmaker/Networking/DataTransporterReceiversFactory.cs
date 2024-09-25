using System;
using System.Collections.Generic;

namespace Kingmaker.Networking;

public class DataTransporterReceiversFactory
{
	public readonly struct Args
	{
		public readonly PhotonActorNumber SourcePlayer;

		public readonly PhotonManager Sender;

		public Args(PhotonActorNumber sourcePlayer, PhotonManager sender)
		{
			SourcePlayer = sourcePlayer;
			Sender = sender;
		}
	}

	private readonly Dictionary<byte, Func<Args, DataReceiver>> m_Factories = new Dictionary<byte, Func<Args, DataReceiver>>();

	public bool HasFactory(byte code)
	{
		return m_Factories.ContainsKey(code);
	}

	public bool TryCreate(byte code, Args args, out DataReceiver receiver)
	{
		if (m_Factories.TryGetValue(code, out var value))
		{
			receiver = value(args);
			return true;
		}
		receiver = null;
		return false;
	}

	public void Register(byte code, Func<Args, DataReceiver> factory)
	{
		m_Factories.Add(code, factory);
	}

	public void Unregister(byte code)
	{
		m_Factories.Remove(code);
	}
}
