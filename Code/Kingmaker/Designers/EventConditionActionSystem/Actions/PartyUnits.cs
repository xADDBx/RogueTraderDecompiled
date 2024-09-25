using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/PartyUnits")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("633639d7e81b7d1499298b189b6f18b3")]
public class PartyUnits : GameAction
{
	[SerializeField]
	private Player.CharactersList m_UnitsList;

	public ActionList Actions;

	public override string GetDescription()
	{
		return "Выполняет экшены для указанного списка членов партии.\nВ экшенах текущего итерируемого члена партии можно получить эвалюатором PartyUnit\nActiveUnits: все члены группы и их петы\nEveryone: все, включая тех, кто сейчас не в партии, разделенных и бывших, в том числе спрятанные\nAllDetachedUnits: все кто в отделенной части партии и их петы\nDetachedPartyCharacters: все кто в отделенной части партии, БЕЗ петовPartyCharacters: все члены группы, БЕЗ петов";
	}

	public override string GetCaption()
	{
		return m_UnitsList switch
		{
			Player.CharactersList.ActiveUnits => "All non-hidden party units", 
			Player.CharactersList.Everyone => "Everyone (even remote and ex-companions)", 
			Player.CharactersList.AllDetachedUnits => "All detached units", 
			Player.CharactersList.DetachedPartyCharacters => "Detached companions (not pets)", 
			Player.CharactersList.PartyCharacters => "Party companions (not pets)", 
			_ => $"Party Units ({m_UnitsList})", 
		};
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.GetCharactersList(m_UnitsList).ToTempList())
		{
			using (ContextData<PartyUnitData>.Request().Setup(item))
			{
				Actions.Run();
			}
		}
	}
}
