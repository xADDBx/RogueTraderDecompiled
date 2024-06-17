using Kingmaker.ElementsSystem.ContextData;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class CheckedPositionData : ContextData<CheckedPositionData>
{
	public Vector3 Position { get; private set; }

	public CheckedPositionData Setup(Vector3 position)
	{
		Position = position;
		return this;
	}

	protected override void Reset()
	{
		Position = default(Vector3);
	}
}
