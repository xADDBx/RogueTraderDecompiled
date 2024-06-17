using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints;
using UnityEngine;

namespace Kingmaker.BarkBanters;

[AllowedOn(typeof(BlueprintBarkBanter))]
[TypeId("829809919e5d4c5da7a345c8931a52a8")]
public class AstropathBriefComponent : BlueprintComponent
{
	[SerializeField]
	private BlueprintAstropathBrief.Reference m_AstropathBrief;

	public BlueprintAstropathBrief AstropathBrief => m_AstropathBrief?.Get();
}
