using System;
using System.Runtime.Serialization;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
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

	string IHaveCaption.Caption => GetCaption();

	public void InitName()
	{
		name = "$" + GetType().Name + "$" + Uuid.Instance.CreateString();
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
			return GetCaption();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return GetType().Name;
		}
	}

	public abstract string GetCaption();

	public virtual string GetDescription()
	{
		return GetType().Name + ": нет описания";
	}

	public virtual Color GetCaptionColor()
	{
		return Color.white;
	}

	protected void LogError(string error)
	{
		string text = Owner.name + " " + Owner.AssetGuid;
		PFLog.Default.Error(error + ". " + name + " in " + text);
	}
}
