using System;

namespace Kingmaker.Utility.BudgetedUpdate;

public class BudgetedLateUpdateRoot : BudgetedUpdateRoot
{
	private static readonly TimeSpan Budget = TimeSpan.FromSeconds(0.5);

	public static BudgetedLateUpdateRoot Instance => AutoSingleton<BudgetedLateUpdateRoot>.Instance;

	public BudgetedLateUpdateRoot()
		: base(Budget)
	{
	}

	private void LateUpdate()
	{
		Invoke();
	}

	protected override void Destroy()
	{
		AutoSingleton<BudgetedLateUpdateRoot>.Destroy();
	}
}
