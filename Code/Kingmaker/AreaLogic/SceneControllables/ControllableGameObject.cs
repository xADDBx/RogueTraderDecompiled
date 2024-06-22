using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
public class ControllableGameObject : ControllableComponent
{
	public enum BoolUnsettable
	{
		None,
		Activate,
		Deactivate
	}

	[FormerlySerializedAs("IsEnableByDefault")]
	[Tooltip("Если нужно, чтобы объект был включен/выключен автоматически на первом старте, используйте Activate/Deactivate. Если нужно оставить его состояние, оставьте None")]
	public BoolUnsettable IsActiveByDefault;

	public override void SetState(ControllableState state)
	{
		base.SetState(state);
		base.gameObject.SetActive(state.Active);
	}

	public override void SetDefaultState()
	{
		if (IsActiveByDefault != 0)
		{
			base.gameObject.SetActive(IsActiveByDefault == BoolUnsettable.Activate);
		}
	}
}
