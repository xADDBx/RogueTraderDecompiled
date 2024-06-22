using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.Visual.VFXGraph;

[AddComponentMenu("VFX/Property Binders/Owlcat Transform Binder")]
[VFXBinder("Transform/Owlcat Transform")]
public class VFXOwlcatTransformBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Transform" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "Transform";

	public Transform Target;

	private ExposedProperty Position;

	private ExposedProperty Angles;

	private ExposedProperty Scale;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = value;
			UpdateSubProperties();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateSubProperties();
	}

	private void OnValidate()
	{
		UpdateSubProperties();
	}

	private void UpdateSubProperties()
	{
		Position = m_Property + "_position";
		Angles = m_Property + "_angles";
		Scale = m_Property + "_scale";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && component.HasVector3(Position) && component.HasVector3(Angles))
		{
			return component.HasVector3(Scale);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetVector3(Position, Target.position);
		component.SetVector3(Angles, Target.eulerAngles);
		component.SetVector3(Scale, Target.localScale);
	}

	public override string ToString()
	{
		return string.Format("Owlcat Transform : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
