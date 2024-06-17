using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Settings;

namespace Kingmaker.Utility;

public class ReportPrivacyManager
{
	private Dictionary<string, (bool promo, bool priv)> m_EmailsDict;

	private void InicDictionary()
	{
		if (m_EmailsDict != null)
		{
			return;
		}
		try
		{
			string value = SettingsRoot.Game.Main.PromoEmails.GetValue();
			m_EmailsDict = new Dictionary<string, (bool, bool)>();
			foreach (string item in value.Split(new string[1] { ";;" }, StringSplitOptions.None).ToList())
			{
				try
				{
					if (!string.IsNullOrEmpty(item))
					{
						List<string> list = item.Split(';').ToList();
						(bool, bool) value2 = (list[1] == "t", list[2] == "t");
						m_EmailsDict.Add(list[0], value2);
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
			m_EmailsDict = new Dictionary<string, (bool, bool)>();
		}
	}

	public void ManageClose(string email, bool promo, bool privacy, bool isSend)
	{
		if (string.IsNullOrEmpty(email))
		{
			return;
		}
		InicDictionary();
		if (privacy)
		{
			if (!m_EmailsDict.ContainsKey(email))
			{
				(bool, bool) value = (promo && isSend, true);
				m_EmailsDict.Add(email, value);
				UpdateConfig();
			}
			else if (isSend && promo && !m_EmailsDict[email].promo)
			{
				(bool, bool) value2 = (true, true);
				m_EmailsDict[email] = value2;
				UpdateConfig();
			}
		}
	}

	public (bool promo, bool priv) GetEmailAgreements(string email)
	{
		InicDictionary();
		if (!string.IsNullOrEmpty(email) && m_EmailsDict.ContainsKey(email))
		{
			return m_EmailsDict[email];
		}
		return (promo: false, priv: false);
	}

	private void UpdateConfig()
	{
		try
		{
			SettingsRoot.Game.Main.PromoEmails.SetValueAndConfirm(DictToString());
			SettingsController.Instance.SaveAll();
		}
		catch
		{
		}
	}

	private string DictToString()
	{
		string text = string.Empty;
		try
		{
			foreach (KeyValuePair<string, (bool, bool)> item in m_EmailsDict)
			{
				string text2 = (item.Value.Item2 ? "t" : "f");
				string text3 = ((item.Value.Item2 && item.Value.Item1) ? "t" : "f");
				text = text + item.Key + ";" + text3 + ";" + text2 + ";;";
			}
			return text;
		}
		catch
		{
			return string.Empty;
		}
	}
}
