namespace Owlcat.Runtime.Core.Registry;

public class RegisteredObjectBase
{
	public bool Enabled { get; private set; }

	public void Enable()
	{
		Repository.Instance.GetRegistry(GetType()).Register(this);
		Enabled = true;
		OnEnabled();
	}

	public void Disable()
	{
		try
		{
			OnDisabled();
		}
		finally
		{
			Repository.Instance.GetRegistry(GetType()).Delete(this);
			Enabled = false;
		}
	}

	protected virtual void OnEnabled()
	{
	}

	protected virtual void OnDisabled()
	{
	}
}
