using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.Localization;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowMultipleComponents]
[TypeId("479ebb2441f4430fb4ff044a89ffb9ff")]
public abstract class AddStringToFact : EntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private LocalizedString m_AdditionalString;

	public StringJunctionType JunctionType;

	public virtual string AdditionalString => m_AdditionalString;

	public string NewString(string baseString)
	{
		return JunctionType switch
		{
			StringJunctionType.AfterSpace => baseString + " " + AdditionalString, 
			StringJunctionType.AfterNewString => baseString + "\n" + AdditionalString, 
			StringJunctionType.AfterFirstSquareBrackets => "[" + baseString + "] " + AdditionalString, 
			StringJunctionType.AfterSecondSquareBrackets => baseString + " [" + AdditionalString + "]", 
			StringJunctionType.AfterFirstRoundBrackets => "(" + baseString + ") " + AdditionalString, 
			StringJunctionType.AfterSecondRoundBrackets => baseString + " (" + AdditionalString + ")", 
			StringJunctionType.BeforeSpace => AdditionalString + " " + baseString, 
			StringJunctionType.BeforeNewString => AdditionalString + "\n" + baseString, 
			StringJunctionType.BeforeFirstSquareBrackets => "[" + AdditionalString + "] " + baseString, 
			StringJunctionType.BeforeSecondSquareBrackets => AdditionalString + " [" + baseString + "]", 
			StringJunctionType.BeforeFirstRoundBrackets => "(" + AdditionalString + ") " + baseString, 
			StringJunctionType.BeforeSecondRoundBrackets => AdditionalString + " (" + baseString + ")", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
