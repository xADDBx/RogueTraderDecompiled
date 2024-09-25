using Kingmaker.Blueprints.Root;

namespace Kingmaker.Controllers.Dialog;

public interface IDialogShowData
{
	string GetText(DialogColors colors);
}
