using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA;

public class BotInstructionIndex<TInstruction> where TInstruction : BlueprintScriptableObject
{
	private string m_IndexFile;

	private Dictionary<string, BlueprintReference<TInstruction>> m_Index;

	public IEnumerable<string> Instructions
	{
		get
		{
			CheckInit();
			return m_Index.Keys;
		}
	}

	public BotInstructionIndex(string indexFile)
	{
		m_IndexFile = indexFile;
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
			GetLogChannel().Error("Instruction not found: " + name);
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
		if (string.IsNullOrEmpty(m_IndexFile))
		{
			GetLogChannel().Error("Failed to load Instruction Index: Index file name is not set!");
			return;
		}
		string path = Path.Combine(ApplicationPaths.streamingAssetsPath, m_IndexFile);
		if (File.Exists(path))
		{
			try
			{
				using StreamReader streamReader = new StreamReader(path);
				GetLogChannel().Log("Loading Instruction Index.");
				m_Index = JsonConvert.DeserializeObject<Dictionary<string, BlueprintReference<TInstruction>>>(streamReader.ReadToEnd());
				CheckDeletedInstructions();
				return;
			}
			catch (Exception ex)
			{
				GetLogChannel().Error("Failed to load Instruction Index");
				GetLogChannel().Exception(ex);
				return;
			}
		}
		GetLogChannel().Log("Created empty Instruction Index.");
		m_Index = new Dictionary<string, BlueprintReference<TInstruction>>();
		Dump();
	}

	public void Dump()
	{
		try
		{
			CheckInit();
			using StreamWriter streamWriter = new StreamWriter(Path.Combine(ApplicationPaths.streamingAssetsPath, m_IndexFile));
			streamWriter.Write(JsonConvert.SerializeObject(m_Index, Formatting.Indented));
		}
		catch (Exception arg)
		{
			GetLogChannel().Error($"Failed to save instruction index: \n{arg}");
		}
	}

	public void Register(TInstruction instruction)
	{
		try
		{
			CheckInit();
			if (m_Index.TryGetValue(instruction.name, out var value))
			{
				if (instruction == value.Get())
				{
					return;
				}
				GetLogChannel().Log("Instruction [" + instruction.name + "] changed guid!");
			}
			List<KeyValuePair<string, BlueprintReference<TInstruction>>> list = m_Index.Where((KeyValuePair<string, BlueprintReference<TInstruction>> x) => x.Value.Get() == instruction).ToList();
			if (list.Count > 1)
			{
				GetLogChannel().Error("Found too many duplicates in index!");
			}
			else if (list.Count == 1)
			{
				GetLogChannel().Log("Renaming instruction [" + list[0].Key + " -> " + instruction.name + "]");
				m_Index.Remove(list[0].Key);
			}
			else
			{
				GetLogChannel().Log("Registering instruction [" + instruction.name + "]");
			}
			m_Index[instruction.name] = instruction.ToReference<BlueprintReference<TInstruction>>();
			Dump();
		}
		catch (Exception arg)
		{
			GetLogChannel().Error($"Failed to register instruction: \n{arg}");
		}
	}

	public void RegenerateIndex()
	{
	}
}
