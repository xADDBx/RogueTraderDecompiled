using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.QA.Validation;

public class ValidationError
{
	public const string ACTIVE_METADATA = "ActiveSelf";

	public string GameObject { get; }

	public ErrorLevel Level { get; }

	public string MessageFormat { get; }

	public object[] Params { get; }

	public bool IsShownInHook { get; }

	public string Owner { get; }

	public string Type { get; }

	public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();


	public string Message => GetMessage();

	public ValidationError(ErrorLevel level, string message, string owner, params object[] @params)
	{
		Level = level;
		MessageFormat = message;
		Owner = owner;
		IsShownInHook = true;
		Params = @params?.ToArray();
	}

	public ValidationError(ErrorLevel level, bool isShownInHook, string objectPath, bool active, string message, string owner, params object[] @params)
		: this(level, message, owner, @params)
	{
		IsShownInHook = isShownInHook;
		GameObject = objectPath;
		Metadata["ActiveSelf"] = active.ToString();
	}

	public ValidationError(ErrorLevel level, bool isShownInHook, string objectPath, string type, bool active, string message, string owner, Dictionary<string, string> metadata, params object[] @params)
		: this(level, isShownInHook, objectPath, active, message, owner, @params)
	{
		Type = type;
		Metadata = metadata;
	}

	private string GetMessage()
	{
		try
		{
			if (Params.Length != 0)
			{
				return string.Format(MessageFormat, Params);
			}
			return MessageFormat;
		}
		catch (FormatException arg)
		{
			QALogger.Error($"Message {MessageFormat} trown exeption: {arg}");
			return MessageFormat;
		}
	}
}
