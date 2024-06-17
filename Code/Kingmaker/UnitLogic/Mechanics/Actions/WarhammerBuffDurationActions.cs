using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("d0b95cbff58b2c9408b4e46a5cfd4c5c")]
public class WarhammerBuffDurationActions : ContextAction
{
	[Serializable]
	public class ActionsInfo
	{
		public int durationStart;

		public int durationEnd;

		public ActionList actions;
	}

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public ActionsInfo[] durationActions;

	public ActionList otherValuesActions;

	public ActionList noBuffActions;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		return $"Perform actions according to buff [{Buff}] duration";
	}

	public override void RunAction()
	{
		Buff buff = base.Target?.Entity.Buffs.GetBuff(Buff);
		if (buff == null)
		{
			noBuffActions.Run();
			return;
		}
		int expirationInRounds = buff.ExpirationInRounds;
		for (int i = 0; i < durationActions.Length; i++)
		{
			ActionsInfo actionsInfo = durationActions[i];
			if (actionsInfo.durationStart >= expirationInRounds && actionsInfo.durationEnd <= expirationInRounds)
			{
				actionsInfo.actions.Run();
				return;
			}
		}
		otherValuesActions.Run();
	}
}
