using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d8e2c5bb9ff388542b90552f59c8d14a")]
public class ContextActionShowBark : ContextAction
{
	public LocalizedString WhatToBark;

	[HideInInspector]
	public SharedStringAsset WhatToBarkShared;

	public bool ShowWhileUnconscious;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	public override string GetCaption()
	{
		return "Show bark on target unit";
	}

	public override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			return;
		}
		PartLifeState lifeStateOptional = entity.GetLifeStateOptional();
		if (lifeStateOptional == null || lifeStateOptional.IsConscious || ShowWhileUnconscious)
		{
			if (base.Context.MaybeCaster == null)
			{
				PFLog.Default.Error(this, "Caster is missing");
				return;
			}
			LocalizedString localizedString = (WhatToBarkShared ? WhatToBarkShared.String : WhatToBark);
			float duration = (BarkDurationByText ? UIUtility.GetBarkDuration(localizedString) : UIUtility.DefaultBarkTime);
			BarkPlayer.Bark(entity, localizedString, duration, playVoiceOver: false, ContextData<InteractingUnitData>.Current?.Unit);
		}
	}
}
