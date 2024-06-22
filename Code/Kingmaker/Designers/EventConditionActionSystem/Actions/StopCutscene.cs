using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Stop cutscene")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("086f25bb4bbcf634289124979aefb433")]
public class StopCutscene : GameAction
{
	private enum UnitCheckType
	{
		Params,
		Controlled
	}

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Cutscene")]
	private CutsceneReference m_Cutscene;

	[SerializeReference]
	public AbstractUnitEvaluator WithUnit;

	[SerializeField]
	[InfoBox("If set to 'Controlled', stops all cutscenes that marked selected unit.\r\nIf set to 'Params' stops all cutscenes that have selected unit as a parameter.\r\n'Controlled' is the old mode, you probably never need it.")]
	[ShowIf("ShowCheck")]
	private UnitCheckType m_CheckType;

	public Cutscene Cutscene => m_Cutscene?.Get();

	private bool ShowCheck => WithUnit != null;

	protected override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			throw new Exception("The cutscene can't be stopped from the preview unit!");
		}
		IEnumerable<CutscenePlayerData> enumerable = Game.Instance.State.Cutscenes.Where((CutscenePlayerData p) => p.Cutscene == Cutscene);
		AbstractUnitEntity ccp = (WithUnit ? WithUnit.GetValue() : null);
		foreach (CutscenePlayerData item in enumerable)
		{
			bool flag = !WithUnit;
			if ((bool)WithUnit)
			{
				flag = m_CheckType switch
				{
					UnitCheckType.Params => item.Parameters.Params.Values.Any(delegate(object v)
					{
						if (v is string text)
						{
							return text == ccp?.UniqueId;
						}
						return v is IEntityRef entityRef && entityRef.Get() == ccp;
					}), 
					UnitCheckType.Controlled => (ccp?.CutsceneControlledUnit?.IsMarkedBy(item)).GetValueOrDefault(), 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			if (flag)
			{
				item.Stop();
			}
		}
	}

	public override string GetCaption()
	{
		return string.Format("Stop scene {0}", Cutscene ? Cutscene.name : "??");
	}
}
