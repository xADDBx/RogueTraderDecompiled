using System;
using System.Collections.Generic;

namespace Kingmaker.Cheats.Logic;

public class SpamLine : IDisposable
{
	private static readonly Stack<SpamLine> s_Pool = new Stack<SpamLine>();

	private SpamLine()
	{
	}

	public static SpamLine GetFromPool()
	{
		if (s_Pool.TryPop(out var result))
		{
			return result;
		}
		return new SpamLine();
	}

	public static void ReleaseToPool(SpamLine spamLine)
	{
		s_Pool.Push(spamLine);
	}

	public void Call(SpamType spamType, int depth, int data)
	{
		if (depth == 0)
		{
			if (data % 2 == 0)
			{
				LogsSpammer.SpamInternal(spamType, data);
			}
			else
			{
				Call(spamType, depth, data + 1);
			}
			return;
		}
		using SpamLine spamLine = GetFromPool();
		spamLine.Call(spamType, depth - 1, data + depth * 2);
	}

	public void Dispose()
	{
		ReleaseToPool(this);
	}
}
