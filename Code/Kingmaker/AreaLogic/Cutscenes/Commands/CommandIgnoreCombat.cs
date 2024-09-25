using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("733a21023bf2444db3f696a212b80ee6")]
public class CommandIgnoreCombat : CommandBase
{
	private class Data
	{
		public UnitReference Unit;

		[CanBeNull]
		public string OriginalGroupId;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool KeepWeaponsOut;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (!(Unit.GetValue() is BaseUnitEntity unit))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		Data commandData = player.GetCommandData<Data>(this);
		UnitReference unitReference = (commandData.Unit = unit.FromBaseUnitEntity());
		if (!(unitReference == null))
		{
			if (KeepWeaponsOut)
			{
				unitReference.Entity.ToBaseUnitEntity().View.HandsEquipment.SetCombatVisualState(inCombat: true);
			}
			if (commandData.Unit.Entity.ToBaseUnitEntity().CombatGroup.Count > 1)
			{
				commandData.OriginalGroupId = commandData.Unit.Entity.ToBaseUnitEntity().CombatGroup.Id;
				commandData.Unit.Entity.ToBaseUnitEntity().CombatGroup.Id = Uuid.Instance.CreateString();
			}
			commandData.Unit.Entity.ToBaseUnitEntity().Features.IsUntargetable.Retain();
			commandData.Unit.Entity.ToBaseUnitEntity().Features.IsIgnoredByCombat.Retain();
			commandData.Unit.Entity.ToBaseUnitEntity().Passive.Retain();
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!(commandData.Unit == null))
		{
			if (!commandData.OriginalGroupId.IsNullOrEmpty())
			{
				commandData.Unit.Entity.ToBaseUnitEntity().CombatGroup.Id = commandData.OriginalGroupId;
			}
			commandData.Unit.Entity.ToBaseUnitEntity().Features.IsUntargetable.Release();
			commandData.Unit.Entity.ToBaseUnitEntity().Features.IsIgnoredByCombat.Release();
			commandData.Unit.Entity.ToBaseUnitEntity().Passive.Release();
			if (KeepWeaponsOut && commandData.Unit != null)
			{
				commandData.Unit.Entity.ToBaseUnitEntity().View.HandsEquipment.SetCombatVisualState(inCombat: false);
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return $"{Unit} ignore combat";
	}
}
