using System;

namespace Kingmaker.EntitySystem.Persistence;

public class BlobReadBudget
{
	private const int MAX_TOTAL_MS = 5000;

	private const int PER_CALL_MS = 2000;

	private const int MIN_CALL_MS = 100;

	private int _remainingMs = 5000;

	public static BlobReadBudget Shared { get; } = new BlobReadBudget();


	public void Reset()
	{
		_remainingMs = 5000;
	}

	public int GetNextTimeoutMs()
	{
		if (_remainingMs >= 100)
		{
			return Math.Min(2000, _remainingMs);
		}
		return 100;
	}

	public void Spend(int time)
	{
		_remainingMs -= time;
	}
}
