using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("09697d9710264407a9c32060cfe8a2c7")]
[PlayerUpgraderAllowed(false)]
public class CompleteEtude : GameAction, IEtudeReference
{
	[HideIf("Evaluate")]
	public BlueprintEtudeReference Etude;

	[ShowIf("Evaluate")]
	[SerializeReference]
	public BlueprintEvaluator EtudeEvaluator;

	public bool Evaluate;

	public override string GetDescription()
	{
		BlueprintEtude sb = Etude?.Get();
		return string.Format("Завершить этюд {0}", (!Evaluate) ? sb.NameSafe() : (EtudeEvaluator ? EtudeEvaluator.GetCaption() : "??"));
	}

	protected override void RunAction()
	{
		BlueprintEtude bp = ((!Evaluate) ? Etude.Get() : (EtudeEvaluator ? ((BlueprintEtude)EtudeEvaluator.GetValue()) : null));
		Game.Instance.Player.EtudesSystem.MarkEtudeCompleted(bp, "CompleteEtude action " + base.AssetGuid + " in " + base.Owner.name);
	}

	public override string GetCaption()
	{
		BlueprintEtude sb = Etude?.Get();
		return string.Format("Complete etude ({0})", (!Evaluate) ? sb.NameSafe() : (EtudeEvaluator ? EtudeEvaluator.GetCaption() : "??"));
	}

	public EtudeReferenceType GetUsagesFor(BlueprintEtude f)
	{
		if ((Evaluate ? ((BlueprintEtude)EtudeEvaluator.GetValue()) : ((BlueprintEtude)Etude)) != f)
		{
			return EtudeReferenceType.None;
		}
		return EtudeReferenceType.Complete;
	}
}
