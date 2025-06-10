using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA;

public class BotInstructionIndex<TInstruction> where TInstruction : BlueprintScriptableObject
{
	private Dictionary<string, BlueprintReference<TInstruction>> m_Index;

	public IEnumerable<string> Instructions
	{
		get
		{
			CheckInit();
			return m_Index.Keys;
		}
	}

	protected virtual LogChannel GetLogChannel()
	{
		return LogChannel.Default;
	}

	private void CheckInit()
	{
		if (m_Index == null)
		{
			Init();
		}
	}

	public TInstruction GetInstruction(string name)
	{
		CheckInit();
		if (!m_Index.TryGetValue(name, out var value))
		{
			GetLogChannel().Error("Instruction not found: {0}", name);
		}
		return value;
	}

	private void CheckDeletedInstructions()
	{
		foreach (KeyValuePair<string, BlueprintReference<TInstruction>> item in m_Index.Where((KeyValuePair<string, BlueprintReference<TInstruction>> x) => x.Value == null).ToList())
		{
			m_Index.Remove(item.Key);
		}
	}

	public void Init()
	{
		m_Index = new Dictionary<string, BlueprintReference<TInstruction>>();
		foreach (TInstruction item in Utilities.GetScriptableObjects<TInstruction>().ToList())
		{
			if (m_Index.ContainsKey(item.name))
			{
				m_Index[item.name] = item.ToReference<BlueprintReference<TInstruction>>();
			}
			else
			{
				m_Index.Add(item.name, item.ToReference<BlueprintReference<TInstruction>>());
			}
		}
		GetLogChannel().Log("Initialize index with {0} items", m_Index.Count);
	}

	public void Register(TInstruction instruction)
	{
		try
		{
			CheckInit();
			if (m_Index.ContainsKey(instruction.name))
			{
				m_Index[instruction.name] = instruction.ToReference<BlueprintReference<TInstruction>>();
			}
			else
			{
				m_Index.Add(instruction.name, instruction.ToReference<BlueprintReference<TInstruction>>());
			}
		}
		catch (Exception arg)
		{
			GetLogChannel().Error($"Failed to register instruction: \n{arg}");
		}
	}
}
