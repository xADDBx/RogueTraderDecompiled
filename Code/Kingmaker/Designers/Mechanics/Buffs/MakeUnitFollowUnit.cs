using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("1afd030149be41a438924ef518fe0782")]
public class MakeUnitFollowUnit : UnitFactComponentDelegate, IHashable
{
	public bool AlwaysRun;

	public bool CanBeSlowerThanLeader;

	[Tooltip("If set, unit will follow leader while it is playing cutscene. Example: follow while bark with cutscene")]
	public bool FollowWhileCutscene;

	[Tooltip("Main character if not specified")]
	[SerializeReference]
	public AbstractUnitEvaluator Leader;

	protected override void OnActivateOrPostLoad()
	{
		BaseUnitEntity leader = Game.Instance.Player.MainCharacterEntity;
		if (Leader != null)
		{
			if (!(Leader.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Fact {this}, {Leader} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			leader = baseUnitEntity;
		}
		base.Owner.GetOrCreate<UnitPartFollowUnit>().Init(leader, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartFollowUnit>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
