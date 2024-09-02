using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("aa7344daa4944044aa1d74b62e3c246c")]
public class SetPlayerActiveWeaponSet : GameAction
{
	[SerializeField]
	[Tooltip("Select weapon set number for a weapon. Valid index 1 or 2!")]
	private int m_WeaponSet;

	public override string GetCaption()
	{
		return $"Switch player weapon set to {m_WeaponSet}";
	}

	protected override void RunAction()
	{
		PartUnitBody partUnitBody = GameHelper.GetPlayerCharacter()?.Body;
		int num = m_WeaponSet - 1;
		if (partUnitBody != null && partUnitBody.CurrentHandEquipmentSetIndex != num)
		{
			partUnitBody.CurrentHandEquipmentSetIndex = num;
		}
	}
}
