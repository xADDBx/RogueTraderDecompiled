using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Posts;

public class PostAttunedAbility : Feature, IHashable
{
	public enum PostAbilityStatus
	{
		Unlearned,
		Normal,
		Attuned
	}

	[JsonProperty]
	public PostType PostType;

	public PostData PostData => (base.Owner as StarshipEntity)?.Blueprint.Posts.Find((PostData x) => x.type == PostType);

	public new BlueprintShipPostExpertise Blueprint => base.Blueprint as BlueprintShipPostExpertise;

	public BlueprintAbility PostDefaultAbility => Blueprint.DefaultPostAbility;

	[JsonProperty]
	public bool IsAttuned { get; private set; }

	public BlueprintAbility PostCurrentAbility()
	{
		if (AbilityStatus() != PostAbilityStatus.Attuned)
		{
			return PostDefaultAbility;
		}
		return Blueprint.ChangedPostAbility;
	}

	private PostAttunedAbility(BlueprintFeature blueprint, BaseUnitEntity owner, MechanicsContext parentContext = null)
		: base(blueprint, owner, parentContext)
	{
	}

	public PostAttunedAbility(PostType postType, BlueprintFeature blueprint, BaseUnitEntity owner, MechanicsContext parentContext = null)
		: base(blueprint, owner, parentContext)
	{
		PostType = postType;
	}

	public PostAttunedAbility(JsonConstructorMark _)
		: base(_)
	{
	}

	public PostAbilityStatus AbilityStatus()
	{
		return PostAbilityStatus.Normal;
	}

	public void Attune()
	{
		IsAttuned = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref PostType);
		bool val2 = IsAttuned;
		result.Append(ref val2);
		return result;
	}
}
