using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Cheats.Exceptions;

namespace Core.Cheats;

public class HelpCommands
{
	private readonly CheatDatabase m_Database;

	[Cheat]
	public static string Help_New(string cmdName = null)
	{
		return CheatsManagerHolder.System.Help.Help(cmdName);
	}

	public HelpCommands(CheatDatabase database)
	{
		m_Database = database;
	}

	private static void GetSignatureBasic(CheatMethodInfo info, StringBuilder sb)
	{
		sb.AppendFormat("{0} {1} ({2})", info.ReturnType, info.Name, string.Join(", ", info.Parameters.Select((CheatParameter v) => (!v.HasDefaultValue) ? (v.Type + " " + v.Name) : (v.Type + " " + v.Name + " = default"))));
	}

	private string Help(string cmdName = null)
	{
		if (string.IsNullOrWhiteSpace(cmdName))
		{
			return ListCommands();
		}
		if (m_Database.CommandsByName.TryGetValue(cmdName, out var value))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Command {0}:", cmdName).AppendLine();
			if (value is CheatMethodInfoInternal cheatMethodInfoInternal)
			{
				stringBuilder.Append(cheatMethodInfoInternal.MethodSignature).AppendLine();
			}
			else
			{
				GetSignatureBasic(value, stringBuilder);
				stringBuilder.AppendLine();
			}
			if (!string.IsNullOrWhiteSpace(value.Description))
			{
				stringBuilder.AppendLine("Description:");
				stringBuilder.AppendLine(value.Description);
			}
			if (!string.IsNullOrWhiteSpace(value.ExampleUsage))
			{
				stringBuilder.AppendLine("Example:");
				stringBuilder.AppendLine(value.ExampleUsage);
			}
			return stringBuilder.ToString();
		}
		if (m_Database.PropertiesByName.TryGetValue(cmdName, out var value2))
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.AppendFormat("Variable {0}:", cmdName).AppendLine();
			stringBuilder2.AppendFormat("{0} {1}", value2.Type, value2.Name);
			if (!string.IsNullOrWhiteSpace(value2.Description))
			{
				stringBuilder2.AppendLine("Description:");
				stringBuilder2.AppendLine(value2.Description);
			}
			if (!string.IsNullOrWhiteSpace(value2.ExampleUsage))
			{
				stringBuilder2.AppendLine("Example:");
				stringBuilder2.AppendLine(value2.ExampleUsage);
			}
			return stringBuilder2.ToString();
		}
		throw new CommandNotFoundException(cmdName);
	}

	private string ListCommands()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_Database.CommandsByName.Any())
		{
			stringBuilder.AppendLine("Known commands: ");
		}
		foreach (KeyValuePair<string, CheatMethodInfo> item in m_Database.CommandsByName)
		{
			if (item.Value is CheatMethodInfoInternal cheatMethodInfoInternal)
			{
				stringBuilder.AppendFormat("{0}: {1}", item.Key, cheatMethodInfoInternal.MethodSignature).AppendLine();
				continue;
			}
			stringBuilder.AppendFormat("{0}: ", item.Key);
			GetSignatureBasic(item.Value, stringBuilder);
			stringBuilder.AppendLine();
		}
		if (m_Database.PropertiesByName.Any())
		{
			stringBuilder.AppendLine("Known variables: ");
		}
		foreach (KeyValuePair<string, CheatPropertyInfo> item2 in m_Database.PropertiesByName)
		{
			stringBuilder.AppendFormat("{0}: {1} {2}", item2.Key, item2.Value.Type, item2.Value.Name).AppendLine();
		}
		if (m_Database.ExternalCommands.Any())
		{
			stringBuilder.AppendLine("Known externals: ");
		}
		string[] externalCommands = m_Database.ExternalCommands;
		foreach (string value in externalCommands)
		{
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}
}
