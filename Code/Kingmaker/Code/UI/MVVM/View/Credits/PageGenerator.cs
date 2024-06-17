using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints.Credits;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

[Serializable]
public class PageGenerator
{
	public struct SearchResult
	{
		public BlueprintCreditsGroup Group;

		public int Page;

		public int Row;
	}

	public float HeightPage = 784f;

	public float HeightHeader = 40f;

	public float HeightDeveloperRow = 45f;

	public float HeightBakerRow = 45f;

	private const int ColumnsCount = 2;

	private const string CompanyTagWrite = "<company>{0}</company>";

	private const string PersonTagWrite = "<person>{0}</person>";

	private const string RoleTagWrite = "<role>{0}</role>";

	private const string HeaderTagWrite = "<header>{0}</header>";

	private const string TextTagWrite = "<text>{0}</text>";

	private const string CompanyTagRead = "<company>(.*)</company>";

	private const string PersonTagRead = "<person>(.*)</person>";

	private const string RoleTagRead = "<role>(.*)</role>";

	private const string HeaderTagRead = "<header>(.*)</header>";

	private const string TextTagRead = "<text>(.*)</text>";

	public static string ReadCompany(string row)
	{
		return Regex.Match(row, "<company>(.*)</company>", RegexOptions.Singleline).Groups[1].Value;
	}

	public static string WriteCompany(string text)
	{
		return $"<company>{text}</company>";
	}

	public static string ReadPerson(string row)
	{
		return Regex.Match(row, "<person>(.*)</person>", RegexOptions.Singleline).Groups[1].Value;
	}

	public static string WritePerson(string text)
	{
		return $"<person>{text}</person>";
	}

	public static string ReadRole(string row)
	{
		return Regex.Match(row, "<role>(.*)</role>", RegexOptions.Singleline).Groups[1].Value;
	}

	public static string WriteRole(string text)
	{
		return $"<role>{text}</role>";
	}

	public static string ReadHeader(string row)
	{
		return Regex.Match(row, "<header>(.*)</header>", RegexOptions.Singleline).Groups[1].Value;
	}

	public static string WriteHeader(string text)
	{
		return $"<header>{text}</header>";
	}

	public static string ReadText(string row)
	{
		return Regex.Match(row, "<text>(.*)</text>", RegexOptions.Singleline).Groups[1].Value;
	}

	public static string WriteText(string text)
	{
		return $"<text>{text}</text>";
	}

	public List<SearchResult> GenerateSearch(BlueprintCreditsGroup group, string filter)
	{
		List<SearchResult> list = new List<SearchResult>();
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		if (group.TeamsData != null && group.OrderTeams.Count > 0)
		{
			foreach (string order in group.OrderTeams)
			{
				CreditTeam team = group.TeamsData.Teams.Find((CreditTeam x) => string.Equals(DeleteAllSpaces(x.KeyTeam), DeleteAllSpaces(order), StringComparison.OrdinalIgnoreCase));
				if (team == null)
				{
					continue;
				}
				List<CreditPerson> list2 = group.Persones.FindAll((CreditPerson x) => string.Equals(DeleteAllSpaces(x.KeyTeam), DeleteAllSpaces(team.KeyTeam), StringComparison.OrdinalIgnoreCase));
				if (list2.Count > 0)
				{
					num += HeightHeader;
					if (num + HeightDeveloperRow * 2f >= HeightPage)
					{
						num = HeightHeader + HeightDeveloperRow;
						num3 = ((!string.IsNullOrEmpty(team.NameTeam)) ? 1 : 0);
						num2++;
					}
					else
					{
						num3 += ((!string.IsNullOrEmpty(team.NameTeam)) ? 1 : 0);
					}
				}
				foreach (CreditPerson item in list2)
				{
					if (string.IsNullOrEmpty(item.Name.Replace("\r", "")))
					{
						continue;
					}
					string[] array = item.KeyRole?.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
					num += HeightDeveloperRow * (float)((array == null || array.Length == 0) ? 1 : array.Length);
					if (!string.IsNullOrEmpty(item.Text?.Text))
					{
						num2++;
						num = 0f;
					}
					else if (num > HeightPage)
					{
						num = HeightHeader + HeightDeveloperRow;
						num2++;
						num3 = ((!string.IsNullOrEmpty(team.NameTeam)) ? 2 : 0);
						if (item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							list.Add(new SearchResult
							{
								Group = group,
								Page = num2,
								Row = 1
							});
						}
					}
					else
					{
						if (item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							list.Add(new SearchResult
							{
								Group = group,
								Page = num2,
								Row = num3
							});
						}
						num3++;
					}
				}
			}
		}
		else
		{
			int count = group.Persones.Count;
			for (int i = 0; i < count; i++)
			{
				CreditPerson creditPerson = group.Persones[i];
				if (i % 2 == 0)
				{
					num += HeightBakerRow;
				}
				if (num >= HeightPage)
				{
					num = HeightBakerRow;
					num2++;
					num3 = 0;
				}
				if (creditPerson.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					list.Add(new SearchResult
					{
						Group = group,
						Page = num2,
						Row = num3
					});
				}
				num3++;
			}
		}
		return list;
	}

	private string DeleteAllSpaces(string text)
	{
		text = text.Trim();
		text = text.Replace(" ", "");
		text = text.ToLower();
		return text;
	}

	public List<string> GeneratePages(BlueprintCreditsGroup group)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		float num = 0f;
		if (group.TeamsData != null && group.OrderTeams.Count > 0)
		{
			foreach (string order in group.OrderTeams)
			{
				CreditTeam team = group.TeamsData.Teams.Find((CreditTeam x) => string.Equals(DeleteAllSpaces(x.KeyTeam), DeleteAllSpaces(order), StringComparison.OrdinalIgnoreCase));
				if (team == null)
				{
					continue;
				}
				List<CreditPerson> list2 = group.Persones.FindAll((CreditPerson x) => string.Equals(DeleteAllSpaces(x.KeyTeam), DeleteAllSpaces(team.KeyTeam), StringComparison.OrdinalIgnoreCase));
				string value = $"<header>{team.NameTeam.Text}</header>";
				if (list2.Count > 0)
				{
					num += HeightHeader;
					if (num + HeightDeveloperRow * 2f >= HeightPage)
					{
						num = HeightHeader + HeightDeveloperRow;
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
						stringBuilder.AppendLine(value);
					}
					else
					{
						stringBuilder.AppendLine(value);
					}
				}
				foreach (CreditPerson item in list2)
				{
					if (item.Text != null && !string.IsNullOrWhiteSpace(item.Text.Text))
					{
						stringBuilder.Clear();
						list.Add($"<text>{item.Text.Text}</text>");
						num = 0f;
					}
					else if (!string.IsNullOrEmpty(item.Name.Replace("\r", "")))
					{
						string[] array = item.KeyRole?.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
						num += HeightDeveloperRow * (float)((array == null || array.Length == 0) ? 1 : array.Length);
						string value2 = string.Format("<person>{0}</person>", item.Name.Replace("\r", "")) + $"<role>{item.KeyRole}</role>";
						if (num > HeightPage)
						{
							num = HeightHeader + HeightDeveloperRow;
							list.Add(stringBuilder.ToString());
							stringBuilder.Clear();
							stringBuilder.AppendLine(value);
							stringBuilder.AppendLine(value2);
						}
						else
						{
							stringBuilder.AppendLine(value2);
						}
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
			}
		}
		else
		{
			int count = group.Persones.Count;
			for (int i = 0; i < count; i++)
			{
				CreditPerson creditPerson = group.Persones[i];
				if (!string.IsNullOrEmpty(creditPerson.Name.Replace("\r", "")))
				{
					if (i % 2 == 0)
					{
						num += HeightBakerRow;
					}
					if (num >= HeightPage)
					{
						num = HeightBakerRow;
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
					stringBuilder.AppendLine(WritePerson(creditPerson.Name.Replace("\r", "")));
				}
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
			}
		}
		return list;
	}
}
