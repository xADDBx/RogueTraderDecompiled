using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.Utility;

[Serializable]
public class PartyContext : ReportContextBase
{
	public class ReportParameterHelper
	{
		public string PartyMemberName;

		public string ContextType;

		public string ContextName;

		public string Guid;
	}

	private const string KeyPartyMemberName = "Party Member Name";

	private const string KeyLevelName = "Level";

	public List<ContextRow> PartyMembers;

	public List<ContextRow> Pets;

	public List<ContextRow> PartyClasses;

	public List<ContextRow> PartyMemberConditions;

	public List<ContextRow> PartyMemberBuffs;

	public List<ContextRow> AllEquipedItems;

	public List<ContextRow> LastCastedSpells;

	public List<ContextRow> LastUsedItems;

	public PartyContext(List<ReportParameterHelper> itemsHistory, List<ReportParameterHelper> spellsHistory)
	{
		PartyMembers = new List<ContextRow>();
		Pets = new List<ContextRow>();
		PartyClasses = new List<ContextRow>();
		AllEquipedItems = new List<ContextRow>();
		LastCastedSpells = new List<ContextRow>();
		LastUsedItems = new List<ContextRow>();
		PartyMemberBuffs = new List<ContextRow>();
		PartyMemberConditions = new List<ContextRow>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		List<UnitCondition> list = Enum.GetValues(typeof(UnitCondition)).Cast<UnitCondition>().ToList();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
		{
			ContextRow contextRow = new ContextRow();
			BaseUnitEntity baseUnitEntity = partyCharacter.Entity.ToBaseUnitEntity();
			string characterName = baseUnitEntity.Blueprint.CharacterName;
			string text = baseUnitEntity.Blueprint.NameSafe();
			contextRow.Parameters.Add(new ContextParameter("Context", BugContext.InnerContextType.Unit.ToString()));
			contextRow.Parameters.Add(new ContextParameter("Name", text));
			contextRow.Parameters.Add(new ContextParameter("Guid", baseUnitEntity.ViewSettings.PrefabGuid));
			PartyMembers.Add(contextRow);
			if (!string.IsNullOrEmpty(contextRow.GetKey("Guid")) && !string.IsNullOrEmpty(characterName) && !dictionary.ContainsKey(characterName))
			{
				dictionary.Add(characterName, 0);
				dictionary2.Add(characterName, text);
			}
			foreach (ItemEntity item7 in baseUnitEntity.Inventory)
			{
				bool isInStash = item7.IsInStash;
				if (!(item7.Owner != partyCharacter.Entity || isInStash))
				{
					ContextRow item = new ContextRow
					{
						Parameters = 
						{
							new ContextParameter("Context", BugContext.InnerContextType.Item.ToString()),
							new ContextParameter("Party Member Name", text),
							new ContextParameter("Name", item7.Blueprint.NameSafe() ?? ""),
							new ContextParameter("Guid", item7.Blueprint.AssetGuid.ToString())
						}
					};
					AllEquipedItems.Add(item);
				}
			}
			foreach (ClassData @class in baseUnitEntity.Progression.Classes)
			{
				ContextRow item2 = new ContextRow
				{
					Parameters = 
					{
						new ContextParameter("Context", BugContext.InnerContextType.CharacterClass.ToString()),
						new ContextParameter("Party Member Name", text),
						new ContextParameter("Name", @class.CharacterClass.NameSafe()),
						new ContextParameter("Level", @class.Level.ToString()),
						new ContextParameter("Guid", @class.CharacterClass.AssetGuid.ToString())
					}
				};
				PartyClasses.Add(item2);
			}
			foreach (Buff buff in baseUnitEntity.Buffs)
			{
				ContextRow item3 = new ContextRow
				{
					Parameters = 
					{
						new ContextParameter("Context", BugContext.InnerContextType.Buff.ToString()),
						new ContextParameter("Party Member Name", text),
						new ContextParameter("Name", buff.Blueprint.NameSafe() ?? ""),
						new ContextParameter("Guid", buff.Blueprint.AssetGuid.ToString())
					}
				};
				PartyMemberBuffs.Add(item3);
			}
			PartUnitState state = baseUnitEntity.State;
			foreach (UnitCondition item8 in list)
			{
				if (state.HasCondition(item8))
				{
					ContextRow item4 = new ContextRow
					{
						Parameters = 
						{
							new ContextParameter("Context", BugContext.InnerContextType.Condition.ToString()),
							new ContextParameter("Party Member Name", text),
							new ContextParameter("Name", item8.ToString())
						}
					};
					PartyMemberConditions.Add(item4);
				}
			}
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.IsPet)
			{
				ContextRow contextRow2 = new ContextRow();
				string val = partyAndPet.Blueprint.NameSafe();
				contextRow2.Parameters.Add(new ContextParameter("Context", BugContext.InnerContextType.Unit.ToString()));
				contextRow2.Parameters.Add(new ContextParameter("Name", val));
				contextRow2.Parameters.Add(new ContextParameter("Guid", partyAndPet.ViewSettings.PrefabGuid));
				Pets.Add(contextRow2);
			}
		}
		int num = dictionary.Count * 10;
		Dictionary<string, HashSet<string>> dictionary3 = new Dictionary<string, HashSet<string>>();
		foreach (string key in dictionary2.Keys)
		{
			if (!dictionary3.ContainsKey(key))
			{
				dictionary3.Add(key, new HashSet<string>());
			}
		}
		foreach (ReportParameterHelper item9 in itemsHistory)
		{
			if (dictionary.ContainsKey(item9.PartyMemberName) && dictionary[item9.PartyMemberName] < 10 && dictionary3.ContainsKey(item9.PartyMemberName) && dictionary2.ContainsKey(item9.PartyMemberName) && !dictionary3[item9.PartyMemberName].Contains(item9.ContextName))
			{
				dictionary[item9.PartyMemberName] = dictionary[item9.PartyMemberName] + 1;
				dictionary3[item9.PartyMemberName].Add(item9.ContextName);
				ContextRow item5 = new ContextRow
				{
					Parameters = 
					{
						new ContextParameter("Context", BugContext.InnerContextType.Item.ToString()),
						new ContextParameter("Party Member Name", dictionary2[item9.PartyMemberName]),
						new ContextParameter("Name", item9.ContextName),
						new ContextParameter("Guid", item9.Guid)
					}
				};
				LastUsedItems.Add(item5);
				if (LastUsedItems.Count >= num)
				{
					break;
				}
			}
		}
		foreach (string item10 in new List<string>(dictionary.Keys))
		{
			dictionary[item10] = 0;
		}
		dictionary3.Clear();
		foreach (string key2 in dictionary2.Keys)
		{
			if (!dictionary3.ContainsKey(key2))
			{
				dictionary3.Add(key2, new HashSet<string>());
			}
		}
		foreach (ReportParameterHelper item11 in spellsHistory)
		{
			if (dictionary.ContainsKey(item11.PartyMemberName) && dictionary[item11.PartyMemberName] < 10 && dictionary3.ContainsKey(item11.PartyMemberName) && dictionary2.ContainsKey(item11.PartyMemberName) && !dictionary3[item11.PartyMemberName].Contains(item11.ContextName))
			{
				dictionary[item11.PartyMemberName] = dictionary[item11.PartyMemberName] + 1;
				dictionary3[item11.PartyMemberName].Add(item11.ContextName);
				ContextRow item6 = new ContextRow
				{
					Parameters = 
					{
						new ContextParameter("Context", BugContext.InnerContextType.Spell.ToString()),
						new ContextParameter("Party Member Name", dictionary2[item11.PartyMemberName]),
						new ContextParameter("Name", item11.ContextName),
						new ContextParameter("Guid", item11.Guid)
					}
				};
				LastCastedSpells.Add(item6);
				if (LastCastedSpells.Count >= num)
				{
					break;
				}
			}
		}
		base.Contexts.Add("Party Member", PartyMembers);
		base.Contexts.Add("Pets", Pets);
		base.Contexts.Add("Party Member Classes", PartyClasses);
		base.Contexts.Add("Conditions", PartyMemberConditions);
		base.Contexts.Add("Buffs", PartyMemberBuffs);
		base.Contexts.Add("Equipped Items", AllEquipedItems);
		base.Contexts.Add("Last Casted Spells", LastCastedSpells);
		base.Contexts.Add("Last Used Items", LastUsedItems);
	}
}
