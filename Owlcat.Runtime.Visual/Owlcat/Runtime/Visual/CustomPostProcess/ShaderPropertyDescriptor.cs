using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.CustomPostProcess;

[Serializable]
public sealed class ShaderPropertyDescriptor
{
	[NonSerialized]
	private int m_NameId;

	public Shader Shader;

	public int Index;

	public string Name;

	public string Description;

	public ShaderPropertyType Type;

	public ShaderPropertyFlags Flags;

	public string[] Attibutes;

	public float FloatValue;

	public Vector2 RangeLimits;

	public int IntValue;

	public Vector4 VectorValue;

	public Color ColorValue;

	public string DefaultTextureNameValue;

	public Texture TextureValue;

	public ShaderPropertyDescriptor(ShaderPropertyDescriptor property)
	{
		Shader = property.Shader;
		Index = property.Index;
		Name = property.Name;
		Description = property.Description;
		Type = property.Type;
		Flags = property.Flags;
		Attibutes = property.Attibutes;
		FloatValue = property.FloatValue;
		RangeLimits = property.RangeLimits;
		IntValue = property.IntValue;
		VectorValue = property.VectorValue;
		ColorValue = property.ColorValue;
		DefaultTextureNameValue = property.DefaultTextureNameValue;
		TextureValue = property.TextureValue;
		m_NameId = Shader.PropertyToID(Name);
	}

	public ShaderPropertyDescriptor(Shader shader, int index, Dictionary<string, Texture> textureProperties)
	{
		Shader = shader;
		Index = index;
		Name = shader.GetPropertyName(index);
		Description = shader.GetPropertyDescription(index);
		Type = shader.GetPropertyType(index);
		Flags = shader.GetPropertyFlags(index);
		Attibutes = shader.GetPropertyAttributes(index);
		switch (Type)
		{
		case ShaderPropertyType.Color:
			ColorValue = shader.GetPropertyDefaultVectorValue(index);
			break;
		case ShaderPropertyType.Vector:
			VectorValue = shader.GetPropertyDefaultVectorValue(index);
			break;
		case ShaderPropertyType.Float:
			FloatValue = shader.GetPropertyDefaultFloatValue(index);
			break;
		case ShaderPropertyType.Range:
			FloatValue = shader.GetPropertyDefaultFloatValue(index);
			RangeLimits = shader.GetPropertyRangeLimits(index);
			break;
		case ShaderPropertyType.Texture:
			DefaultTextureNameValue = shader.GetPropertyTextureDefaultName(index);
			textureProperties.TryGetValue(Name, out TextureValue);
			break;
		case ShaderPropertyType.Int:
			IntValue = shader.GetPropertyDefaultIntValue(index);
			break;
		}
		m_NameId = Shader.PropertyToID(Name);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Name);
		stringBuilder.AppendLine(Flags.ToString());
		stringBuilder.AppendLine(Type.ToString());
		switch (Type)
		{
		case ShaderPropertyType.Color:
			stringBuilder.AppendLine(VectorValue.ToString());
			break;
		case ShaderPropertyType.Vector:
			stringBuilder.AppendLine(VectorValue.ToString());
			break;
		case ShaderPropertyType.Float:
			stringBuilder.AppendLine(FloatValue.ToString());
			break;
		case ShaderPropertyType.Range:
			stringBuilder.AppendLine(FloatValue.ToString());
			stringBuilder.AppendLine(RangeLimits.ToString());
			break;
		case ShaderPropertyType.Texture:
			stringBuilder.AppendLine(DefaultTextureNameValue.ToString());
			break;
		case ShaderPropertyType.Int:
			stringBuilder.AppendLine(IntValue.ToString());
			break;
		}
		string[] attibutes = Attibutes;
		foreach (string value in attibutes)
		{
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}

	internal void Interp(ShaderPropertyDescriptor from, ShaderPropertyDescriptor to, float t)
	{
		switch (Type)
		{
		case ShaderPropertyType.Color:
			ColorValue = Color.Lerp(from.ColorValue, to.ColorValue, t);
			break;
		case ShaderPropertyType.Vector:
			VectorValue = Vector4.Lerp(from.VectorValue, to.VectorValue, t);
			break;
		case ShaderPropertyType.Float:
			FloatValue = Mathf.Lerp(from.FloatValue, to.FloatValue, t);
			break;
		case ShaderPropertyType.Range:
			FloatValue = Mathf.Lerp(from.FloatValue, to.FloatValue, t);
			break;
		case ShaderPropertyType.Texture:
			if (t > 0f)
			{
				TextureValue = to.TextureValue;
			}
			else
			{
				TextureValue = from.TextureValue;
			}
			break;
		case ShaderPropertyType.Int:
			IntValue = (int)Mathf.Lerp(from.IntValue, to.IntValue, t);
			break;
		}
	}

	internal void SetValue(ShaderPropertyDescriptor value)
	{
		switch (Type)
		{
		case ShaderPropertyType.Color:
			ColorValue = value.ColorValue;
			break;
		case ShaderPropertyType.Vector:
			VectorValue = value.VectorValue;
			break;
		case ShaderPropertyType.Float:
			FloatValue = value.FloatValue;
			break;
		case ShaderPropertyType.Range:
			FloatValue = value.FloatValue;
			break;
		case ShaderPropertyType.Texture:
			TextureValue = value.TextureValue;
			break;
		case ShaderPropertyType.Int:
			IntValue = value.IntValue;
			break;
		}
	}

	internal void UpdateMaterial(Material material)
	{
		switch (Type)
		{
		case ShaderPropertyType.Color:
			material.SetColor(m_NameId, ColorValue);
			break;
		case ShaderPropertyType.Vector:
			material.SetVector(m_NameId, VectorValue);
			break;
		case ShaderPropertyType.Float:
			material.SetFloat(m_NameId, FloatValue);
			break;
		case ShaderPropertyType.Range:
			material.SetFloat(m_NameId, FloatValue);
			break;
		case ShaderPropertyType.Texture:
			material.SetTexture(m_NameId, TextureValue);
			break;
		case ShaderPropertyType.Int:
			material.SetInt(m_NameId, IntValue);
			break;
		}
	}
}
