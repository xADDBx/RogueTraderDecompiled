using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Utility.GuidUtility;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class Element : ICanBeLogContext, IHaveCaption, IHaveDescription, IElementConvertable
{
	[HideInInspector]
	public string name;

	public static LogChannel LogChannel = LogChannelFactory.GetOrCreate("Elements");

	public SimpleBlueprint Owner { get; set; }

	public string AssetGuid
	{
		get
		{
			string text = name;
			int num = text.IndexOf("$", 1, StringComparison.Ordinal);
			if (num < 0 || num >= text.Length - 1)
			{
				return "";
			}
			return text.Substring(num + 1, text.Length - num - 1);
		}
	}

	public string AssetGuidShort
	{
		get
		{
			string text = name;
			int num = text.IndexOf("$", 1, StringComparison.Ordinal);
			if (num < 0 || num >= text.Length - 1 - 4)
			{
				return "";
			}
			return text.Substring(num + 1, 4);
		}
	}

	string IHaveDescription.Description => GetDescription();

	string IHaveCaption.Caption => GetCaption(useLineBreaks: false);

	[CanBeNull]
	private static ElementsDebugger Debugger => ContextData<ElementsDebugger>.Current;

	public void InitName()
	{
		name = "$" + GetType().Name + "$" + Uuid.Instance.CreateString();
	}

	public static string GenerateName(string typeName)
	{
		return "$" + typeName + "$" + Guid.NewGuid().ToString("N");
	}

	public static implicit operator bool(Element o)
	{
		return o != null;
	}

	public static Element CreateInstance(Type t)
	{
		Element obj = (Element)Activator.CreateInstance(t);
		obj.name = "$" + t.Name + "$" + Uuid.Instance.CreateString();
		return obj;
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		Json.BlueprintBeingRead.Data.AddToElementsList(this);
	}

	public sealed override string ToString()
	{
		try
		{
			return GetCaption(useLineBreaks: false);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return GetType().Name;
		}
	}

	public virtual string GetCaption(bool useLineBreaks)
	{
		return GetCaption();
	}

	public abstract string GetCaption();

	public virtual string GetDescription()
	{
		return $"{GetType().Name}: {GetCaption()}";
	}

	public virtual Color GetCaptionColor()
	{
		return Color.white;
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	private static void Log(LogSeverity severity, object context, string messageFormat, params object[] @params)
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Log(LogChannel, context, severity, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogInfo(object context, string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Message, context, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogInfo(string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Message, Debugger?.Element, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogWarning(object context, string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Warning, context, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogWarning(string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Warning, Debugger?.Element, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogError(object context, string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Error, context, messageFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public static void LogError(string messageFormat, params object[] @params)
	{
		Log(LogSeverity.Error, Debugger?.Element, messageFormat, @params);
	}

	[StackTraceIgnore]
	public static void LogException(Exception exception)
	{
		Element source = Debugger?.Element ?? (exception as ElementLogicException)?.Element;
		Owlcat.Runtime.Core.Logging.Logger.Instance.Log(LogChannel, source, LogSeverity.Error, exception, null, null);
	}
}
