using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("182075e83588f23428cf054eb5f3668f")]
public class StarshipCompanionsOnPostLogic : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IHashable
{
	public int SkillValueToPassBasicCheck;

	public int MicroabilityCooldownWhenNotPassed;

	public int StartingUltimateRounds;

	public int SkillPointsToAddExtraUltimateRound;

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased || !(base.Owner is StarshipEntity { IsInGame: not false } starshipEntity))
		{
			return;
		}
		foreach (Post post in starshipEntity.Hull.Posts)
		{
			if (PostSkillCheckPassed(post))
			{
				continue;
			}
			foreach (Ability item in from ab in post.UnlockedAbilities()
				where ab.Blueprint.AbilityTag != AbilityTag.StarshipUltimateAbility
				select ab)
			{
				starshipEntity.GetAbilityCooldownsOptional()?.StartAutonomousCooldown(item.Blueprint, MicroabilityCooldownWhenNotPassed + 1);
			}
		}
	}

	private int GetPostSkill(Post post)
	{
		return post.CurrentSkillValue;
	}

	private bool PostSkillCheckPassed(Post post)
	{
		if (!post.HasPenalty)
		{
			return GetPostSkill(post) >= SkillValueToPassBasicCheck;
		}
		return false;
	}

	private Post GetPost(StarshipEntity ownerShip, BlueprintAbility abp)
	{
		foreach (Post post in ownerShip.Hull.Posts)
		{
			if ((from ab in post.UnlockedAbilities()
				select ab.Blueprint).Contains(abp))
			{
				return post;
			}
		}
		return null;
	}

	public int AddToAbilityCooldown(StarshipEntity ownerShip, BlueprintAbility abp)
	{
		Post post = GetPost(ownerShip, abp);
		if (post != null)
		{
			if (!PostSkillCheckPassed(post))
			{
				return MicroabilityCooldownWhenNotPassed;
			}
			return 0;
		}
		return 0;
	}

	public bool CanUseUltimate(StarshipEntity ownerShip, BlueprintAbility abp)
	{
		Post post = GetPost(ownerShip, abp);
		if (post != null)
		{
			return PostSkillCheckPassed(post);
		}
		return false;
	}

	public int GetUltimateBuffDuration(StarshipEntity ownerShip, BlueprintAbility abp)
	{
		Post post = GetPost(ownerShip, abp);
		if (post != null)
		{
			int postSkill = GetPostSkill(post);
			return StartingUltimateRounds + postSkill / SkillPointsToAddExtraUltimateRound;
		}
		return StartingUltimateRounds;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
