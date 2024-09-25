using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public abstract class ClockworkCheck : ClockworkCommand
{
	[HideInInspector]
	public bool? LastResult { get; protected set; }

	public override void Initialize()
	{
		base.Initialize();
		LastResult = null;
	}

	public virtual string GetErrorMessage()
	{
		return "{}";
	}
}
