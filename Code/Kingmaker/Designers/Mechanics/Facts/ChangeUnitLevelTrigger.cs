using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("789dd01d78384d1cb23fb6502108e6f2")]
public class ChangeUnitLevelTrigger : UnitFactComponentDelegate, IChangeUnitLevel, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int AppliedCharacterLevel;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AppliedCharacterLevel);
			return result;
		}
	}

	public ActionList ActionOnChangeUnitLevel;

	protected override void OnActivate()
	{
		base.OnActivate();
		RequestSavableData<SavableData>().AppliedCharacterLevel = base.Owner.Progression.CharacterLevel;
	}

	void IChangeUnitLevel.HandleChangeUnitLevel()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (!base.Owner.IsPreview && base.Owner == baseUnitEntity)
		{
			SavableData savableData = RequestSavableData<SavableData>();
			if (savableData.AppliedCharacterLevel != base.Owner.Progression.CharacterLevel)
			{
				savableData.AppliedCharacterLevel = base.Owner.Progression.CharacterLevel;
				base.Fact.RunActionInContext(ActionOnChangeUnitLevel, base.Owner.ToITargetWrapper());
			}
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
