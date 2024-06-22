using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ShowBark")]
[AllowMultipleComponents]
[TypeId("e164ef6758f918a4abcc3889472a2a3c")]
public class ShowBark : GameAction
{
	public LocalizedString WhatToBark;

	[HideInInspector]
	public SharedStringAsset WhatToBarkShared;

	[Tooltip("Bark duration depends on text length")]
	public bool BarkDurationByText;

	[FormerlySerializedAs("Target")]
	[SerializeReference]
	public AbstractUnitEvaluator TargetUnit;

	[SerializeReference]
	public MapObjectEvaluator TargetMapObject;

	[Tooltip("Allow set exact playback time")]
	public bool OverrideBarkDuration;

	[Tooltip("Exact playback time")]
	[ShowIf("OverrideBarkDuration")]
	public float BarkDuration;

	private Entity Target
	{
		get
		{
			if (TargetUnit == null)
			{
				if (TargetMapObject == null)
				{
					return null;
				}
				return TargetMapObject.GetValue();
			}
			return TargetUnit.GetValue();
		}
	}

	protected override void RunAction()
	{
		if (TargetUnit == null || TargetUnit.GetValue().LifeState.IsConscious)
		{
			Entity target = Target;
			LocalizedString localizedString = (WhatToBarkShared ? WhatToBarkShared.String : WhatToBark);
			float duration = UIUtility.DefaultBarkTime;
			if (BarkDurationByText)
			{
				duration = UIUtility.GetBarkDuration(localizedString);
			}
			if (OverrideBarkDuration)
			{
				duration = BarkDuration;
			}
			BarkPlayer.Bark(target, localizedString, duration, BarkDurationByText, ContextData<InteractingUnitData>.Current?.Unit);
		}
	}

	public override string GetCaption()
	{
		Element arg = ((TargetUnit != null) ? ((MechanicEntityEvaluator)TargetUnit) : ((MechanicEntityEvaluator)TargetMapObject));
		return $"Show Bark (on {arg})";
	}
}
