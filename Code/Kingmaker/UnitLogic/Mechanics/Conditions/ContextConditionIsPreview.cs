using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("6ab3899b2cc347f9ad7ab46d6b3fc355")]
public class ContextConditionIsPreview : ContextCondition
{
	[SerializeField]
	private bool m_IsOnlyChargen;

	protected override string GetConditionCaption()
	{
		return string.Concat($"Check if target is preview, only chargen : {m_IsOnlyChargen}");
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity is BaseUnitEntity { IsPreviewUnit: not false })
		{
			if (m_IsOnlyChargen)
			{
				if (MainMenuUI.IsActive)
				{
					return MainMenuUI.Instance.CharGenContextVM?.CharGenVM != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
