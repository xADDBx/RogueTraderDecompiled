using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("9b00608c02075774aad83fbd8cd8261c")]
public class SpotMapObject : GameAction
{
	[SerializeReference]
	public MapObjectEvaluator Target;

	[SerializeReference]
	public AbstractUnitEvaluator Spotter;

	public override string GetCaption()
	{
		if (Spotter == null)
		{
			return $"Main character spot map object {Target}";
		}
		return $"{Spotter} spot map object {Target}";
	}

	protected override void RunAction()
	{
		IAbstractUnitEntity spotter = Game.Instance.Player.MainCharacter.Entity;
		if (Spotter != null)
		{
			if (!(Spotter.GetValue() is BaseUnitEntity baseUnitEntity))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Spotter} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
				return;
			}
			spotter = baseUnitEntity;
		}
		MapObjectEntity value = Target.GetValue();
		if (!value.IsAwarenessCheckPassed)
		{
			value.IsAwarenessCheckPassed = true;
			EventBus.RaiseEvent((IMapObjectEntity)value, (Action<IAwarenessHandler>)delegate(IAwarenessHandler h)
			{
				h.OnEntityNoticed(spotter.ToBaseUnitEntity());
			}, isCheckRuntime: true);
		}
	}
}
