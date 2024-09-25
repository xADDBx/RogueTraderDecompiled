using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Visual.DayNightCycle;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("30897cdea22c475494ff0e14d456730e")]
public class EtudeBracketOverrideLightConfig : EtudeBracketTrigger, IHashable
{
	[ValidateNotNull]
	public SceneLightConfig.Link LightConfig;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		LightController.Active.OverrideConfig(LightConfig.Load());
	}

	protected override void OnExit()
	{
		LightController.Active.OverrideConfig(null);
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
