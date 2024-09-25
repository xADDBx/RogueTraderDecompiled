using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0bafe290985641b9a831acfc037c631f")]
public class PartyMembersDetachEvaluated : GameAction
{
	[InfoBox("If set, action will detach all EXCEPT listed in DetachThese")]
	public bool DetachAllButThese;

	[SerializeReference]
	public AbstractUnitEvaluator[] DetachThese;

	public ActionList AfterDetach;

	public bool m_RestrictPartySize;

	[ShowIf("m_RestrictPartySize")]
	public int m_PartySize = -1;

	public override string GetCaption()
	{
		AbstractUnitEvaluator[] detachThese = DetachThese;
		if (detachThese == null || detachThese.Length == 0)
		{
			return "Split party (manually)";
		}
		return "Split from party (" + string.Join(", ", from u in DetachThese.EmptyIfNull()
			select u?.ToString()) + ")";
	}

	protected override void RunAction()
	{
		Player player = Game.Instance.Player;
		if (DetachThese.Length != 0)
		{
			List<BaseUnitEntity> list = new List<BaseUnitEntity>();
			foreach (AbstractUnitEvaluator item2 in ElementExtendAsObject.Valid(DetachThese))
			{
				if (!(item2.GetValue() is BaseUnitEntity item))
				{
					string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {item2} is not BaseUnitEntity";
					if (!QAModeExceptionReporter.MaybeShowError(message))
					{
						UberDebug.LogError(message);
					}
				}
				else
				{
					list.Add(item);
				}
			}
			BaseUnitEntity[] array = player.Party.ToArray();
			foreach (BaseUnitEntity baseUnitEntity in array)
			{
				if (list.Contains(baseUnitEntity) ^ DetachAllButThese)
				{
					player.DetachPartyMember(baseUnitEntity);
				}
			}
			AfterDetach?.Run();
		}
		else
		{
			int count = (m_RestrictPartySize ? Mathf.Clamp(m_PartySize, 1, 6) : 6);
			EventBus.RaiseEvent(delegate(IDetachUnitsUIHandler h)
			{
				h.HandleDetachUnits(count, AfterDetach);
			});
		}
	}
}
