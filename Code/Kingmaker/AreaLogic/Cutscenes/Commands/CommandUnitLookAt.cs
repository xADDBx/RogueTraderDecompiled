using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("ccbd07b7ffc946647a6a30d5628d1836")]
public class CommandUnitLookAt : CommandBase
{
	private class Data
	{
		public float InitialOrientation;

		public bool Finished;

		public double LastTime;

		public bool Signalled;

		public bool Freeze;
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private PositionEvaluator m_Position;

	[SerializeField]
	private float m_AngularSpeed;

	[SerializeField]
	private bool m_Continuous;

	[SerializeField]
	[ConditionalShow("m_Continuous")]
	private bool m_RestoreOrientation;

	[SerializeField]
	[ConditionalShow("m_Continuous")]
	private bool m_FreezeAfterTurn = true;

	[SerializeField]
	private CommandSignalData m_OnTurned = new CommandSignalData
	{
		Name = "OnTurned"
	};

	public override bool IsContinuous => m_Continuous;

	private void LookAt(CutscenePlayerData player, bool firstTick = false)
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		if (!(value is LightweightUnitEntity))
		{
			PartUnitState stateOptional = value.GetStateOptional();
			if (stateOptional == null || !stateOptional.CanRotate)
			{
				player.GetCommandData<Data>(this).Finished = !m_Continuous;
				return;
			}
		}
		if (!firstTick && Mathf.Approximately(value.DesiredOrientation, value.Orientation))
		{
			Data commandData = player.GetCommandData<Data>(this);
			if (!m_Continuous)
			{
				commandData.Finished = true;
				return;
			}
			if (m_FreezeAfterTurn)
			{
				commandData.Freeze = true;
				return;
			}
		}
		value.LookAt(m_Position.GetValue());
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity value = m_Unit.GetValue();
		commandData.InitialOrientation = value.DesiredOrientation;
		if (m_AngularSpeed == 0f || skipping)
		{
			LookAt(player, firstTick: true);
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		if (m_RestoreOrientation)
		{
			Data commandData = player.GetCommandData<Data>(this);
			AbstractUnitEntity value = m_Unit.GetValue();
			if (value != null)
			{
				value.DesiredOrientation = commandData.InitialOrientation;
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		Vector3 value2 = m_Position.GetValue();
		float num = value?.GetLookAtAngle(value2) ?? 0f;
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.Freeze)
		{
			if (m_AngularSpeed == 0f)
			{
				LookAt(player);
			}
			else if (value != null)
			{
				double num2 = time - commandData.LastTime;
				float orientation = Mathf.MoveTowardsAngle(value.Orientation, num, (float)num2 * m_AngularSpeed);
				value.SetOrientation(orientation);
				commandData.LastTime = time;
			}
		}
		if (!IsContinuous)
		{
			if (value != null && value.IsInGame)
			{
				PartUnitState stateOptional = value.GetStateOptional();
				if (stateOptional == null || stateOptional.CanRotate)
				{
					goto IL_00b5;
				}
			}
			commandData.Finished = true;
			return;
		}
		goto IL_00b5;
		IL_00b5:
		if (value != null && (commandData.Freeze || Mathf.Approximately(value.Orientation, num)))
		{
			if (!m_Continuous)
			{
				commandData.Finished = true;
			}
			if (m_FreezeAfterTurn)
			{
				commandData.Freeze = true;
			}
			if ((bool)m_OnTurned.Gate && !commandData.Signalled)
			{
				commandData.Signalled = true;
				player.SignalGate(m_OnTurned.Gate);
			}
		}
		if (time > 20.0)
		{
			player.GetCommandData<Data>(this).Finished = true;
		}
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		if (IsContinuous)
		{
			return new CommandSignalData[1] { m_OnTurned };
		}
		return base.GetExtraSignals();
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		LookAt(player);
	}

	public override string GetCaption()
	{
		return m_Unit?.ToString() + " <b>look at</b> " + m_Position;
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!m_Unit || !m_Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}
}
