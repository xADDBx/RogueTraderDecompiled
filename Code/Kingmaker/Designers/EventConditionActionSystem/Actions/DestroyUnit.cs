using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("9f0cd598c83b33949802eb2ed789207c")]
public class DestroyUnit : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool FadeOut;

	public override string GetDescription()
	{
		return $"Уничтожает юнита {Target}\n" + $"Плавно исчезнуть юнитом: {FadeOut}\n" + "ВНИМАНИЕ! ОПАСНЫЙ ACTION! УДАЛЯЙТЕ ЮНИТА ТОЛЬКО ЕСЛИ УВЕРЕНЫ, ЧТО ПОТОМ НЕ НУЖНО БУДЕТ ДЕЛАТЬ КОНВЕРТЫ С УЧАСТИЕМ ЭТОГО ЮНИТА.Не используйте на компаньонах и петах.";
	}

	public override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		if (value is BaseUnitEntity baseUnitEntity && value.IsPlayerFaction)
		{
			PFLog.Default.Error($"{name}: trying to destroy {value} who is a companion. Do not do that!");
			baseUnitEntity.GetOptional<UnitPartCompanion>()?.SetState(CompanionState.ExCompanion);
			UIAccess.SelectionManager.UpdateSelectedUnits();
			baseUnitEntity.IsInGame = false;
			EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<ICompanionChangeHandler>)delegate(ICompanionChangeHandler h)
			{
				h.HandleUnrecruit();
			}, isCheckRuntime: true);
		}
		else
		{
			if (FadeOut)
			{
				value.SetViewHandlingOnDisposePolicy(Entity.ViewHandlingOnDisposePolicyType.FadeOutAndDestroy);
			}
			Game.Instance.EntityDestroyer.Destroy(value);
		}
	}

	public override string GetCaption()
	{
		return $"Destroy ({Target})";
	}
}
