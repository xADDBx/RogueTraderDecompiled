using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("6ab8dd2cc3ba43e8b76882f981bf2b99")]
[PlayerUpgraderAllowed(false)]
public class StartEtude : GameAction, IEtudeReference
{
	[HideIf("Evaluate")]
	public BlueprintEtudeReference Etude;

	[ShowIf("Evaluate")]
	[SerializeReference]
	public BlueprintEvaluator EtudeEvaluator;

	public bool Evaluate;

	[InfoBox("Basically, etude started here will be actually started by EtudeSystemController in its next Tick method. Sometimes there is a need to start an etude immediately. Please, do not overuse!")]
	public bool StartImmediately;

	public override string GetDescription()
	{
		BlueprintEtude sb = Etude?.Get();
		return string.Format("Стартует этюд {0})", (!Evaluate) ? sb.NameSafe() : (EtudeEvaluator ? EtudeEvaluator.GetCaption() : "??"));
	}

	public override void RunAction()
	{
		BlueprintEtude bp = ((!Evaluate) ? Etude.Get() : (EtudeEvaluator ? ((BlueprintEtude)EtudeEvaluator.GetValue()) : null));
		if (StartImmediately)
		{
			Game.Instance.Player.EtudesSystem.StartEtudeImmediately(bp);
		}
		else
		{
			Game.Instance.Player.EtudesSystem.StartEtude(bp);
		}
	}

	public override string GetCaption()
	{
		BlueprintEtude sb = Etude?.Get();
		return string.Format("Start etude ({0})", (!Evaluate) ? sb.NameSafe() : (EtudeEvaluator ? EtudeEvaluator.GetCaption() : "??"));
	}

	public EtudeReferenceType GetUsagesFor(BlueprintEtude f)
	{
		if ((Evaluate ? ((BlueprintEtude)EtudeEvaluator.GetValue()) : ((BlueprintEtude)Etude)) != f)
		{
			return EtudeReferenceType.None;
		}
		return EtudeReferenceType.Start;
	}
}
