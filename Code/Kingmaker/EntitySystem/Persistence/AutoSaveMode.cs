namespace Kingmaker.EntitySystem.Persistence;

public enum AutoSaveMode
{
	None,
	BeforeExit,
	AfterEntry,
	LoadFromSave,
	WhenAreaIsUnloaded
}
