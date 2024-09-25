namespace Kingmaker.Controllers;

public abstract class BaseGameTimeController : IGameTimeController
{
	protected float AdditionalTimeMultiplier = 1f;

	private GameTimeState m_SavedState;

	public GameTimeState TimeState { get; private set; }

	public void SetState(GameTimeState state)
	{
		if (TimeState != state)
		{
			TimeState = state;
			AdditionalTimeMultiplier = GetMultiplierByState(state);
			OnTimeStateChanged();
		}
	}

	protected abstract void OnTimeStateChanged();

	protected void SaveState()
	{
		m_SavedState = TimeState;
	}

	protected void ResumeState()
	{
		SetState(m_SavedState);
	}

	private static float GetMultiplierByState(GameTimeState state)
	{
		return state switch
		{
			GameTimeState.Normal => 1f, 
			GameTimeState.Fast => 2f, 
			GameTimeState.Paused => 0f, 
			_ => 1f, 
		};
	}
}
