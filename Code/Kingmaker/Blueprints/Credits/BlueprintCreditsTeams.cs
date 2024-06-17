using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Credits;

[TypeId("5fc8a9955abbdc44eb3373b66f738b24")]
public class BlueprintCreditsTeams : BlueprintScriptableObject, ICreditsKey
{
	public string CsvName;

	public List<CreditTeam> Teams = new List<CreditTeam>();

	public List<string> GetKeys()
	{
		return Teams.Select((CreditTeam t) => t.KeyTeam).ToList();
	}

	public string GetTeam(string key)
	{
		LocalizedString localizedString = Teams.Find((CreditTeam x) => x != null && x.KeyTeam.Trim().ToLowerInvariant() == key.Trim().ToLowerInvariant())?.NameTeam;
		if (localizedString == null)
		{
			return "";
		}
		return localizedString;
	}
}
