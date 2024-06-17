using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class LevelUpPlanUnitHolder : BaseUnitPart, IHashable
{
	[CanBeNull]
	private BaseUnitEntity m_PlanUnit;

	[CanBeNull]
	public BaseUnitEntity RequestPlan()
	{
		if (m_PlanUnit == null)
		{
			using (ContextData<GameLogDisabled>.Request())
			{
				using (ContextData<AddClassLevels.DoNotCreatePlan>.Request())
				{
					m_PlanUnit = base.Owner.CreatePreview(createView: false);
				}
			}
		}
		return m_PlanUnit;
	}

	protected override void OnDetach()
	{
		m_PlanUnit?.Dispose();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
