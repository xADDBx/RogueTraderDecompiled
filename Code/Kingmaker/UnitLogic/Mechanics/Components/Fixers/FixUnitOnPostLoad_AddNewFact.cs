using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Components.Fixers;

[TypeId("d83f2b14f040488dafc8fb080b11c186")]
public class FixUnitOnPostLoad_AddNewFact : FixUnitOnPostLoad
{
	[SerializeField]
	[FormerlySerializedAs("NewFact")]
	private BlueprintUnitFactReference m_NewFact;

	public BlueprintUnitFact NewFact => m_NewFact?.Get();

	public override void OnPostLoad(BaseUnitEntity unit)
	{
		if (!unit.Facts.Contains(NewFact))
		{
			EntityFact entityFact = unit.AddFact(NewFact);
			if (base.OwnerBlueprint != null)
			{
				entityFact.AddSource(base.OwnerBlueprint);
			}
		}
	}
}
