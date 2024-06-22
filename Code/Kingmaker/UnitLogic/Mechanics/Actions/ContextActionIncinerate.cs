using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("283d792e26dc4f13b2da25cfd03baeb3")]
public class ContextActionIncinerate : ContextAction
{
	public DOT[] DOTTypes;

	public ContextPropertyName ContextPropertyName;

	[SerializeField]
	private BlueprintProjectileReference m_Projectile;

	public new BlueprintProjectile Projectile => m_Projectile?.Get();

	public override string GetCaption()
	{
		return "Launch projectile from each target with buff and save half their burning damage";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity entity = base.Target.Entity;
		if (maybeCaster == null || entity == null)
		{
			return;
		}
		List<BlueprintBuff> buffs = new List<BlueprintBuff>();
		DOT[] dOTTypes = DOTTypes;
		foreach (DOT type in dOTTypes)
		{
			buffs.Add(ContextActionApplyDOT.SelectBuff(type));
		}
		List<BaseUnitEntity> list = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && p.Facts.List.Any((EntityFact fact) => buffs.Contains(fact.Blueprint))).ToList();
		int count = list.Count;
		base.Context[ContextPropertyName] = 0;
		if (count <= 0 || base.Context.DisableFx)
		{
			return;
		}
		foreach (BaseUnitEntity item in list)
		{
			int num = 0;
			dOTTypes = DOTTypes;
			foreach (DOT type2 in dOTTypes)
			{
				num += DOTLogic.GetBasicDamageOfType(item, type2);
			}
			base.Context[ContextPropertyName] += num / 2;
			new ProjectileLauncher(Projectile, item, entity).Ability(base.AbilityContext?.Ability).Launch();
		}
	}
}
