using UnityEngine;

namespace Kingmaker.QA.Arbiter;

internal class InstructionSettings
{
	private string m_PlayerPrefsDataString;

	public string Data => m_PlayerPrefsDataString;

	public bool IsEmpty => string.IsNullOrWhiteSpace(m_PlayerPrefsDataString);

	public bool IsSingleInstruction
	{
		get
		{
			if (!IsEmpty)
			{
				return !m_PlayerPrefsDataString.EndsWith(".txt");
			}
			return false;
		}
	}

	public bool IsInstructionList
	{
		get
		{
			if (!IsEmpty)
			{
				return m_PlayerPrefsDataString.EndsWith(".txt");
			}
			return false;
		}
	}

	public InstructionSettings(ArbiterStartupParameters parameters)
	{
		m_PlayerPrefsDataString = PlayerPrefs.GetString("ArbiterInstruction", "");
		if (!string.IsNullOrWhiteSpace(m_PlayerPrefsDataString))
		{
			PlayerPrefs.DeleteKey("ArbiterInstruction");
			PlayerPrefs.Save();
			return;
		}
		m_PlayerPrefsDataString = parameters.ArbiterInstruction ?? string.Empty;
		if (string.IsNullOrWhiteSpace(m_PlayerPrefsDataString))
		{
			string arbiter = parameters.Arbiter;
			m_PlayerPrefsDataString = ((arbiter != null && arbiter.Length > 0) ? arbiter : "arbiter.txt");
		}
	}
}
