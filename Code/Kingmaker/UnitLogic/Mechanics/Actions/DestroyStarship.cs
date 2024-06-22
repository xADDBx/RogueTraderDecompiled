using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e60b053256f5cf545b5075ee4c00f616")]
public class DestroyStarship : ContextAction
{
	[SerializeField]
	private bool NoLog;

	[SerializeField]
	private bool NoExp;

	public override string GetCaption()
	{
		return "Destroy owner starship";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeOwner = base.Context.MaybeOwner;
		StarshipEntity starship = maybeOwner as StarshipEntity;
		if (starship != null)
		{
			if (NoLog)
			{
				starship.GetOrCreate<Kill.SilentDeathUnitPart>();
			}
			if (NoExp)
			{
				starship.GiveExperienceOnDeath = false;
			}
			starship.LifeState.MarkedForDeath = true;
			Game.Instance.CoroutinesController.InvokeInTime(delegate
			{
				Game.Instance.EntityDestroyer.Destroy(starship);
			}, TimeSpan.FromSeconds(1.0));
		}
	}
}
