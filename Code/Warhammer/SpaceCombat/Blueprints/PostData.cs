using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Warhammer.SpaceCombat.Blueprints;

[Serializable]
public class PostData
{
	public PostType type;

	public StatType AssociatedSkill;

	public Sprite PostSprite;

	public Sprite PostSpriteHolographic;

	[CanBeNull]
	[SerializeReference]
	public AbstractUnitEvaluator DefaultUnit;

	[SerializeField]
	private List<BlueprintAbilityReference> m_DefaultAbilities;

	public IEnumerable<BlueprintAbility> DefaultAbilities => (m_DefaultAbilities?.Dereference()).EmptyIfNull();
}
