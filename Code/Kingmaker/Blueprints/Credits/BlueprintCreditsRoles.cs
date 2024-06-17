using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Credits;

[TypeId("874067c53f2c19c40b8c863c01b7ec03")]
public class BlueprintCreditsRoles : BlueprintScriptableObject, ICreditsKey
{
	public string CsvName;

	public List<CreditRole> Roles = new List<CreditRole>();

	public List<string> GetKeys()
	{
		return Roles.Select((CreditRole r) => r.KeyRole).ToList();
	}

	public string GetRole(string key)
	{
		string[] array = key.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			string k = array[i].Trim().ToLowerInvariant();
			LocalizedString localizedString = Roles.Find((CreditRole x) => x != null && x.KeyRole.Trim().ToLowerInvariant() == k)?.NameRole;
			k = ((localizedString != null) ? ((string)localizedString) : "");
			text += ((text == "") ? k : ("\n" + k));
		}
		return text;
	}
}
