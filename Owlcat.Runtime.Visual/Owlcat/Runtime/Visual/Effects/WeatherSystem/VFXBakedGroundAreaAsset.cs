using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class VFXBakedGroundAreaAsset
{
	[SerializeField]
	private string m_Identifier;

	[SerializeField]
	public Texture2D Texture;

	public string Identifier => m_Identifier;

	public VFXBakedGroundAreaAsset(string identifier, Texture2D texture)
	{
		m_Identifier = identifier;
		Texture = texture;
	}

	public bool Matches(string identifier)
	{
		if (identifier.Equals(m_Identifier))
		{
			return true;
		}
		int result;
		string obj = ((m_Identifier.Length > 4 && m_Identifier[m_Identifier.Length - 3] == '_' && int.TryParse(m_Identifier.Substring(m_Identifier.Length - 2), out result)) ? m_Identifier.Substring(0, m_Identifier.Length - 3) : m_Identifier);
		string value = ((identifier.Length > 4 && identifier[identifier.Length - 3] == '_' && int.TryParse(identifier.Substring(identifier.Length - 2), out result)) ? m_Identifier.Substring(0, identifier.Length - 3) : identifier);
		return obj.Equals(value);
	}
}
