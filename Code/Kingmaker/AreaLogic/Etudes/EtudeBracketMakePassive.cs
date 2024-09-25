using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("839317076a554a999fd5def8820dd93d")]
public class EtudeBracketMakePassive : EtudeBracketTrigger, IHashable
{
	public bool OnAllCustomCompanions;

	[HideIf("OnAllCustomCompanions")]
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		if (OnAllCustomCompanions)
		{
			Game.Instance.Player.AllCharacters.ForEach(delegate(BaseUnitEntity c)
			{
				if (c.IsCustomCompanion() && c.IsInGame && !c.IsPet)
				{
					c.Passive.Retain();
				}
			});
		}
		else if ((bool)Unit && Unit.CanEvaluate())
		{
			Unit.GetValue().Passive.Retain();
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	protected override void OnExit()
	{
		if (OnAllCustomCompanions)
		{
			Game.Instance.Player.AllCharacters.ForEach(delegate(BaseUnitEntity c)
			{
				if (c.IsCustomCompanion() && c.IsInGame && !c.IsPet)
				{
					c.Passive.Release();
				}
			});
		}
		else if ((bool)Unit && Unit.CanEvaluate())
		{
			Unit.GetValue().Passive.Release();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
