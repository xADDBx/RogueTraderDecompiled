using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Credits;

[Serializable]
public class CreditPerson
{
	public enum TypeRow
	{
		Person,
		Image,
		Text
	}

	public TypeRow Type;

	public int Order;

	public string Name;

	public LocalizedString Text;

	public Sprite Image;

	public string KeyRole;

	public string KeyTeam;

	public CreditPerson()
	{
	}

	public CreditPerson(string name)
	{
		Name = name;
	}

	public bool Equals(CreditPerson p)
	{
		if (!EqualsWithoutRole(p))
		{
			return false;
		}
		if (string.IsNullOrEmpty(p.KeyRole) != string.IsNullOrEmpty(KeyRole))
		{
			return false;
		}
		if (KeyRole != null && p.KeyRole != null && !p.KeyRole.Equals(KeyRole))
		{
			return false;
		}
		return true;
	}

	public bool EqualsWithoutRole(CreditPerson p)
	{
		if (!p.Name.Equals(Name))
		{
			return false;
		}
		if (string.IsNullOrEmpty(p.KeyTeam) != string.IsNullOrEmpty(KeyTeam))
		{
			return false;
		}
		if (KeyTeam != null && p.KeyTeam != null && !p.KeyTeam.Equals(KeyTeam))
		{
			return false;
		}
		if (string.IsNullOrEmpty(p.Text?.Text) != string.IsNullOrEmpty(Text?.Text))
		{
			return false;
		}
		if (Text?.Text != null && p.Text?.Text != null && !Text.Text.Equals(p.Text?.Text))
		{
			return false;
		}
		return true;
	}
}
