using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[Serializable]
public class Track : ISerializationCallbackReceiver, IEvaluationErrorHandlingPolicyHolder
{
	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	[SerializeField]
	private List<CommandReference> m_Commands = new List<CommandReference>();

	private List<CommandBase> m_CommandsDereferenced;

	[SerializeField]
	private GateReference m_EndGate;

	[SerializeField]
	private bool m_Repeat;

	public string Comment;

	public bool IsCollapsed;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public List<CommandBase> Commands
	{
		get
		{
			if (m_CommandsDereferenced == null)
			{
				m_CommandsDereferenced = m_Commands.Select((CommandReference r) => r.Get()).ToList();
			}
			return m_CommandsDereferenced;
		}
		set
		{
			m_CommandsDereferenced = value;
		}
	}

	public Gate EndGate
	{
		get
		{
			return m_EndGate.Get();
		}
		set
		{
			m_EndGate = value.ToReference<GateReference>();
		}
	}

	public bool Repeat
	{
		get
		{
			return m_Repeat;
		}
		set
		{
			m_Repeat = value;
		}
	}

	public bool IsContinuous
	{
		get
		{
			if (!m_Repeat)
			{
				List<CommandBase> commands = Commands;
				if (commands != null && commands.Count > 0)
				{
					return Commands.Last().IsContinuous;
				}
				return false;
			}
			return true;
		}
	}

	public void OnBeforeSerialize()
	{
		if (m_CommandsDereferenced != null)
		{
			m_Commands = m_CommandsDereferenced.Select(ElementsReferenceBase.CreateTyped<CommandReference>).ToList();
		}
	}

	public void OnAfterDeserialize()
	{
		m_CommandsDereferenced = null;
	}
}
