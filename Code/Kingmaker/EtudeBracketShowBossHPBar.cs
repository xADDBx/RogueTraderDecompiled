using System;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[Serializable]
[TypeId("ef9796d80f0522b49853347f73ed1ccd")]
public class EtudeBracketShowBossHPBar : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	private LocalizedString BossName;

	[SerializeReference]
	public MechanicEntityEvaluator Unit;

	protected override void OnEnter()
	{
		Show();
	}

	protected override void OnExit()
	{
		Hide();
	}

	protected override void OnResume()
	{
		Show();
	}

	private string GetCounterId()
	{
		return base.Fact.Blueprint.AssetGuid + name;
	}

	private void Show()
	{
		if (!Unit.TryGetValue(out var value))
		{
			return;
		}
		if (value == null)
		{
			PFLog.Etudes.Log("Boss unit is null for " + GetType().Name + " in " + base.Fact.Blueprint.name);
			return;
		}
		PartHealth healthPart = value.GetOptional<PartHealth>();
		if (healthPart == null)
		{
			PFLog.Etudes.Log("Boss unit doesn't have PartHealth for " + GetType().Name + " in " + base.Fact.Blueprint.name);
			return;
		}
		EventBus.RaiseEvent(delegate(IBossHPBarHandler h)
		{
			h.ShowBossHPBar(new ShowBossHPBarUIStruct
			{
				Id = GetCounterId(),
				BossName = BossName,
				CurrentHPGetter = () => healthPart.HitPointsLeft,
				MaxHPGetter = () => healthPart.MaxHitPoints
			});
		});
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IBossHPBarHandler h)
		{
			h.HideBossHPBar(GetCounterId());
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
