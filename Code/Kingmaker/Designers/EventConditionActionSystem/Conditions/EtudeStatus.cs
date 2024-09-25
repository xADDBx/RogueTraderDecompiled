using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("ea981728db8a5f84888ecba390671a05")]
[PlayerUpgraderAllowed(false)]
public class EtudeStatus : Condition, IEtudeReference
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Etude")]
	private BlueprintEtudeReference m_Etude;

	public bool NotStarted;

	public bool Started;

	public bool Playing;

	public bool CompletionInProgress;

	public bool Completed;

	public BlueprintEtude Etude
	{
		get
		{
			return m_Etude?.Get();
		}
		set
		{
			m_Etude = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintEtudeReference>();
		}
	}

	public override string GetDescription()
	{
		return "NotStarted - этюд никогда не стртовали, Started - этюд стартовали, активирвоали и он еще не закомпличен, Playing - активный в данный момент этюд";
	}

	protected override string GetConditionCaption()
	{
		return string.Format("Etude {0} status is: {1} {2} {3} {4} {5} ", Etude, NotStarted ? "NotStarted;" : "", Started ? "Started;" : "", Playing ? "Playing;" : "", CompletionInProgress ? "CompletionInProgress;" : "", Completed ? "Completed;" : "");
	}

	protected override bool CheckCondition()
	{
		Etude etude = Game.Instance.Player.EtudesSystem.Etudes.Get(Etude);
		if (Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(Etude))
		{
			return NotStarted;
		}
		if (Game.Instance.Player.EtudesSystem.EtudeIsCompleted(Etude))
		{
			return Completed;
		}
		if (etude == null || (!etude.IsPlaying && !etude.CompletionInProgress))
		{
			return Started;
		}
		if (etude.CompletionInProgress)
		{
			if (!CompletionInProgress)
			{
				return Started;
			}
			return true;
		}
		if (etude.IsPlaying)
		{
			if (!Playing)
			{
				return Started;
			}
			return true;
		}
		return false;
	}

	public EtudeReferenceType GetUsagesFor(BlueprintEtude f)
	{
		if (Etude != f)
		{
			return EtudeReferenceType.None;
		}
		return EtudeReferenceType.Check;
	}
}
