namespace Kingmaker.Sound.Base;

public interface IRegistrationCallback
{
	void OnAfterRegister();

	void OnBeforeUnregister();
}
