using System;
using System.Diagnostics;

namespace Owlcat.QA.Validation;

public class ValidationProgress
{
	public Action Stepped;

	public Action<ValidationResult> Stopped;

	private string m_Name;

	private int m_ProgressId;

	private int m_TotalSteps;

	private int m_CurrentStep;

	private ValidationResult m_Result;

	private Stopwatch m_Time;

	public bool IsRun => m_Result == ValidationResult.Run;

	public float TimeMs => m_Time.ElapsedMilliseconds;

	internal ValidationProgress(int id, string name, int total = 1)
	{
		m_Name = name;
		m_TotalSteps = total;
		m_Result = ValidationResult.Run;
		m_Time = Stopwatch.StartNew();
	}

	public void SetTotal(int total)
	{
		m_TotalSteps = total;
	}

	public void Step(string name)
	{
		Stepped?.Invoke();
		m_CurrentStep++;
		if (m_TotalSteps <= m_CurrentStep)
		{
			m_TotalSteps = m_CurrentStep + 1;
		}
	}

	public ValidationResult Stop(ValidationResult result = ValidationResult.Success)
	{
		if (result != ValidationResult.Run && IsRun)
		{
			m_Result = result;
			m_Time.Stop();
			if (result == ValidationResult.Success)
			{
				m_CurrentStep = m_TotalSteps;
			}
			Stopped?.Invoke(result);
		}
		return result;
	}
}
