using System;
using System.Reflection;
using Kingmaker.Utility;
using UnityEngine;

[Serializable]
public class UnityFunc
{
	[SerializeField]
	protected UnityEngine.Object m_Target;

	[SerializeField]
	protected string m_Method;

	[NonSerialized]
	protected MethodInfo m_MethodInfo;

	public UnityEngine.Object Target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public string Method
	{
		get
		{
			return m_Method;
		}
		set
		{
			m_Method = value;
		}
	}

	public bool Validate()
	{
		if (m_Target == null)
		{
			return false;
		}
		if (m_MethodInfo == null || m_MethodInfo.Name != m_Method)
		{
			m_MethodInfo = m_Target.GetType().GetMethod(m_Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
		return m_MethodInfo != null;
	}

	public object Invoke()
	{
		if (!Validate())
		{
			return null;
		}
		if (!(m_MethodInfo != null))
		{
			return null;
		}
		return m_MethodInfo.Invoke(m_Target, null);
	}
}
[IKnowWhatImDoing]
public class UnityFunc<TReturn> : UnityFunc
{
	public new TReturn Invoke()
	{
		return (TReturn)base.Invoke();
	}
}
