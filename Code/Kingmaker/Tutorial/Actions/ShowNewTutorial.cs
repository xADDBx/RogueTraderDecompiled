using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Tutorial.Actions;

[TypeId("1c7b1754db3d4c908b1dbe779c4ed85c")]
public class ShowNewTutorial : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintTutorial.Reference m_Tutorial;

	public TutorialContextDataEvaluator[] Evaluators = new TutorialContextDataEvaluator[0];

	public BlueprintTutorial Tutorial => m_Tutorial.Get().GetTutorial();

	public override string GetCaption()
	{
		return $"Show tutorial {Tutorial}";
	}

	protected override void RunAction()
	{
		using TutorialContext tutorialContext = ContextData<TutorialContext>.Request();
		TutorialContextDataEvaluator[] evaluators = Evaluators;
		foreach (TutorialContextDataEvaluator tutorialContextDataEvaluator in evaluators)
		{
			try
			{
				tutorialContext[tutorialContextDataEvaluator.Key] = CreateContextItem(tutorialContextDataEvaluator.GetValue());
			}
			catch (Exception ex)
			{
				tutorialContext[tutorialContextDataEvaluator.Key] = "<b>" + ex.Message + "</b>";
			}
		}
		Game.Instance.Player.Tutorial.Trigger(Tutorial, null);
	}

	private static TutorialContextItem CreateContextItem(object obj)
	{
		if (obj is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity;
		}
		if (obj is AbilityData abilityData)
		{
			return abilityData;
		}
		if (obj is ItemEntity itemEntity)
		{
			return itemEntity;
		}
		throw new Exception("Can't convert object (" + (obj?.GetType().Name ?? "null") + ") to TutorialContextItem");
	}
}
