using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetSoundState")]
[AllowMultipleComponents]
[TypeId("b416216a964543c4e89e50cd51b22cf7")]
public class SetSoundState : GameAction
{
	[SerializeField]
	private AkStateReference m_State;

	protected override void RunAction()
	{
		m_State.Set();
	}

	public override string GetCaption()
	{
		if (m_State == null)
		{
			return "Sound State (unknown)";
		}
		return $"Sound State ({m_State.Group}\\{m_State.Value})";
	}
}
