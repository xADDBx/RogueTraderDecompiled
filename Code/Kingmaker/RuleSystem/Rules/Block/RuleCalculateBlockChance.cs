using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules.Block;

public class RuleCalculateBlockChance : RulebookOptionalTargetEvent<UnitEntity, MechanicEntity>
{
	public readonly ValueModifiersManager BlockValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager BlockValueMultipliers = new ValueModifiersManager();

	public const int BaseBlockChance = 0;

	private int m_shieldBlockChance;

	[CanBeNull]
	public AbilityData Ability { get; }

	public bool IsAutoBlock { get; private set; }

	[CanBeNull]
	public MechanicEntity MaybeAttacker => base.MaybeTarget;

	[NotNull]
	public UnitEntity Defender => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public NotImplementedException Target
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public int Result { get; private set; }

	public int ShieldBlockChance => m_shieldBlockChance;

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in BlockValueModifiers.List)
			{
				yield return item;
			}
			foreach (Modifier item2 in BlockValueMultipliers.List)
			{
				yield return item2;
			}
		}
	}

	public int RawResult { get; private set; }

	public RuleCalculateBlockChance([NotNull] UnitEntity defender, int blockChance, [CanBeNull] MechanicEntity attacker = null, [CanBeNull] AbilityData ability = null, bool isAutoBlock = false)
		: base(defender, attacker)
	{
		Ability = ability;
		IsAutoBlock = isAutoBlock;
		m_shieldBlockChance = blockChance;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		RawResult = ShieldBlockChance + BlockValueModifiers.Value;
		if (!BlockValueMultipliers.Empty)
		{
			RawResult *= BlockValueMultipliers.Value;
		}
		Result = Math.Clamp(RawResult, 0, 95);
		SpecialOverrideWithFeatures();
	}

	private void SpecialOverrideWithFeatures()
	{
		if (MaybeAttacker != null && (bool)MaybeAttacker.Features.AutoHit)
		{
			RawResult = 0;
			Result = 0;
		}
		else if ((bool)Defender.Features.AutoBlock || IsAutoBlock)
		{
			IsAutoBlock = true;
			RawResult = 100;
			Result = 100;
		}
	}
}
