using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("543c77cf38b748cb80f248fa30ee6731")]
public class ReplaceUnitBlueprintForRespec : BlueprintComponent
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Blueprint;

	public BlueprintUnit Blueprint => m_Blueprint;
}
