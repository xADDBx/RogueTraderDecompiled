using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[TypeId("f8b12b0667d7f0449b97e7727dfba247")]
public class VendorEvaluator : GenericEvaluator<PartVendor>
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_VendorEvaluator;

	public override string GetCaption()
	{
		return $"Торговец {m_VendorEvaluator}";
	}

	protected override PartVendor GetValueInternal()
	{
		return m_VendorEvaluator.GetValue().GetOptional<PartVendor>();
	}
}
