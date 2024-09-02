using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Dialog;

public class CombatLogBarkLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventBark>
{
	private const int Capacity = 15;

	private readonly Queue<string> m_PreviousMessages = new Queue<string>(16);

	public void HandleEvent(GameLogEventBark evt)
	{
		AbstractUnitEntity abstractUnitEntity = evt.Actor as AbstractUnitEntity;
		if ((!Game.Instance.TurnController.TurnBasedModeActive || !(Game.Instance.CurrentMode == GameModeType.Default) || abstractUnitEntity == null || abstractUnitEntity.IsInCombat || abstractUnitEntity.IsDead) && !m_PreviousMessages.Contains(evt.Text))
		{
			m_PreviousMessages.Enqueue(evt.Text);
			if (m_PreviousMessages.Count > 15)
			{
				m_PreviousMessages.Dequeue();
			}
			bool flag = false;
			if (!string.IsNullOrEmpty(evt.OverrideName))
			{
				GameLogContext.OverrideName = evt.OverrideName;
				GameLogContext.OverrideNameColor = evt.OverrideNameColor;
				flag = true;
			}
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)abstractUnitEntity;
			GameLogContext.Text = evt.Text;
			AddMessage((GameLogContext.SourceEntity.Value != null || flag) ? new CombatLogMessage(LogThreadBase.Strings.UnitBark.CreateCombatLogMessage(), null, hasTooltip: false) : new CombatLogMessage(LogThreadBase.Strings.ObjectBark.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}

	protected override void DisposeImplementation()
	{
		m_PreviousMessages.Clear();
		base.DisposeImplementation();
	}
}
