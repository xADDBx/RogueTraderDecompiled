using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("4a9d25d64f874754aa85c0f2e819c443")]
public class EtudeBracketForceInitiativeOrder : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator[] m_Order;

	[CanBeNull]
	private static EtudeBracketForceInitiativeOrder s_CurrentInstance => Game.Instance.LoadedAreaState.Settings.CurrentEtudeBracketForceInitiativeOrder;

	public static bool Any => s_CurrentInstance != null;

	[CanBeNull]
	public static IEnumerable<BaseUnitEntity> GetOrderOptional()
	{
		AbstractUnitEntity value;
		return s_CurrentInstance?.m_Order?.Select((AbstractUnitEvaluator i) => (!i.TryGetValue(out value)) ? null : (value as BaseUnitEntity)).NotNull();
	}

	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.SetEtudeBracketForceInitiativeOrder(this);
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.RemoveEtudeBracketForceInitiativeOrder(this);
	}

	protected override void OnResume()
	{
		Game.Instance.LoadedAreaState.Settings.SetEtudeBracketForceInitiativeOrder(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
