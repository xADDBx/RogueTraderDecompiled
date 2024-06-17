using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("f6e13489adfcaeb4fb61519e0aa9c646")]
public class CommandSetCombatMode : CommandBase
{
	private class Data
	{
		public bool OnSwitchedCalled;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool InCombat;

	public bool Continuous;

	public bool WaitForAnimation;

	public CommandSignalData OnSwitched = new CommandSignalData
	{
		Name = "On Switched"
	};

	public override bool IsContinuous => Continuous;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		if (Target.GetValue().View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(InCombat);
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (IsContinuous)
		{
			return false;
		}
		if (!WaitForAnimation)
		{
			return true;
		}
		BaseUnitEntity obj = Target.GetValue() as BaseUnitEntity;
		UnitViewHandsEquipment unitViewHandsEquipment = ((obj == null) ? null : obj.View.Or(null)?.HandsEquipment);
		if (unitViewHandsEquipment != null)
		{
			if (!unitViewHandsEquipment.AreHandsBusyWithAnimation)
			{
				return unitViewHandsEquipment.IsCombatStateConsistent;
			}
			return false;
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		if (Continuous && OnSwitched?.Gate != null && Target.GetValue().AreHandsBusyWithAnimation)
		{
			Data commandData = player.GetCommandData<Data>(this);
			if (!commandData.OnSwitchedCalled)
			{
				commandData.OnSwitchedCalled = true;
				player.SignalGate(OnSwitched.Gate);
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (Continuous && InCombat && Target.GetValue().View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment.SetCombatVisualState(!InCombat);
		}
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (!IsContinuous)
		{
			return base.GetExtraSignals();
		}
		return new CommandSignalData[1] { OnSwitched };
	}

	public override string GetWarning()
	{
		if (!Target || !Target.CanEvaluate())
		{
			return "No unit";
		}
		return null;
	}

	public override string GetCaption()
	{
		if (!Continuous)
		{
			return "Switch <b>combat mode</b> for " + (Target ? Target.GetCaption() : "???");
		}
		return "Set <b>combat mode</b> for " + (Target ? Target.GetCaption() : "???");
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Target || !Target.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
