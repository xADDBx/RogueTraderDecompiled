using System;
using Faithlife.Utility;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Utility.GuidUtility;

public class Uuid
{
	private static readonly byte[] NameBytes = new byte[16];

	private readonly Guid m_SeedGuid;

	private readonly Kingmaker.Utility.StatefulRandom.StatefulRandom m_Random;

	private const string GlobalGuidString = "27bb4190-9b93-4cc5-a460-2bfd9db5802e";

	public static readonly Uuid Instance = new Uuid("GlobalUuid", new Guid("27bb4190-9b93-4cc5-a460-2bfd9db5802e"));

	public Kingmaker.Utility.StatefulRandom.StatefulRandom Random => m_Random;

	public RandState State
	{
		get
		{
			return m_Random.State;
		}
		set
		{
			m_Random.State = value;
		}
	}

	private Uuid()
	{
	}

	public Uuid(string name, Guid startGuid)
	{
		m_SeedGuid = startGuid;
		m_Random = new Kingmaker.Utility.StatefulRandom.StatefulRandom(name);
	}

	public void Seed(uint seed)
	{
		m_Random.Seed(seed);
	}

	public Guid CreateGuid()
	{
		int i = 0;
		for (int num = NameBytes.Length; i < num; i++)
		{
			NameBytes[i] = (byte)m_Random.Range(0, 256);
		}
		return Faithlife.Utility.GuidUtility.Create(m_SeedGuid, NameBytes);
	}

	public string CreateString()
	{
		return CreateGuid().ToString("N");
	}
}
