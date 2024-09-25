namespace Kingmaker.Utility;

public class PerFrameVar<T>
{
	private T m_Value;

	private int m_Frame = -1;

	public bool UpToDate => m_Frame == Game.Instance.RealTimeController.CurrentSystemStepIndex;

	public T Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
			m_Frame = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		}
	}
}
