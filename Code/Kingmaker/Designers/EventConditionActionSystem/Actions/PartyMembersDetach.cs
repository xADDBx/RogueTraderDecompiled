using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("3d947958bd77d10469017478eb6120ed")]
public class PartyMembersDetach : GameAction
{
	[SerializeField]
	private bool m_DetachOnlyListed;

	[ShowIf("m_DetachAllExceptListed")]
	[SerializeField]
	[FormerlySerializedAs("DetachAllExcept")]
	private BlueprintUnitReference[] m_DetachAllExcept = new BlueprintUnitReference[0];

	[ShowIf("m_DetachOnlyListed")]
	[SerializeField]
	private BlueprintUnitReference[] m_DetachAll = new BlueprintUnitReference[0];

	[SerializeField]
	private bool m_RestrictPartySize;

	[SerializeField]
	[ShowIf("m_RestrictPartySize")]
	private int m_PartySize = -1;

	[InfoBox("Если UseRealParty выбрано, будет использована реальная партия, а не та, что на локации. Они могут не совпадать в Capital режиме")]
	[SerializeField]
	private bool UseRealParty;

	[SerializeField]
	private bool DoNotDetachPlayerCharacter;

	public ActionList AfterDetach;

	private bool m_DetachAllExceptListed => !m_DetachOnlyListed;

	public ReferenceArrayProxy<BlueprintUnit> DetachAllExcept
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] detachAllExcept = m_DetachAllExcept;
			return detachAllExcept;
		}
	}

	private ReferenceArrayProxy<BlueprintUnit> DetachAll
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] detachAll = m_DetachAll;
			return detachAll;
		}
	}

	public override string GetCaption()
	{
		if (DetachAllExcept.Length > 0 && m_DetachAllExceptListed)
		{
			return "Split party, detach all except: (" + string.Join(", ", DetachAllExcept.Select((BlueprintUnit u) => u.ToString())) + ")";
		}
		if (m_DetachAll.Length != 0 && m_DetachOnlyListed)
		{
			return "Split party, Detach: (" + string.Join(", ", DetachAll.Select((BlueprintUnit u) => u.ToString())) + ")";
		}
		return "Split party (manually)";
	}

	protected override void RunAction()
	{
		Player p = Game.Instance.Player;
		List<BaseUnitEntity> source = ((!UseRealParty) ? p.Party : p.PartyCharacters.Select((UnitReference u) => u.Entity.ToBaseUnitEntity()).ToTempList());
		if (DoNotDetachPlayerCharacter)
		{
			source = source.Where((BaseUnitEntity u) => u != p.MainCharacter.Entity.ToBaseUnitEntity()).ToTempList();
		}
		if (DetachAllExcept.Length > 0 && m_DetachAllExceptListed)
		{
			foreach (BaseUnitEntity item in source.Where((BaseUnitEntity u) => DetachAllExcept.All((BlueprintUnit b) => u.Blueprint != b)).ToTempList())
			{
				p.DetachPartyMember(item);
			}
			AfterDetach?.Run();
			p.InvalidateCharacterLists();
		}
		else if (m_DetachAll.Length != 0 && m_DetachOnlyListed)
		{
			foreach (BaseUnitEntity item2 in source.Where((BaseUnitEntity u) => DetachAll.Any((BlueprintUnit b) => u.Blueprint == b)).ToTempList())
			{
				p.DetachPartyMember(item2);
			}
			AfterDetach?.Run();
			p.InvalidateCharacterLists();
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
