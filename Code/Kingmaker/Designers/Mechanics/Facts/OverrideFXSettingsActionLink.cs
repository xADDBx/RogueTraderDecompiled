using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("2cb1e7cd78634b5c9a3bcf431e14113b")]
public class OverrideFXSettingsActionLink : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private UnitAnimationActionLink OriginalAcitonLink;

	[SerializeField]
	private UnitAnimationActionLink OverrideActionLink;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartFXSettingOverride>().ActionsOverride[OriginalAcitonLink] = OverrideActionLink;
	}

	protected override void OnDeactivate()
	{
		if (base.Owner.GetOrCreate<UnitPartFXSettingOverride>().ActionsOverride[OriginalAcitonLink] == OverrideActionLink)
		{
			base.Owner.GetOrCreate<UnitPartFXSettingOverride>().ActionsOverride.Remove(OriginalAcitonLink);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
