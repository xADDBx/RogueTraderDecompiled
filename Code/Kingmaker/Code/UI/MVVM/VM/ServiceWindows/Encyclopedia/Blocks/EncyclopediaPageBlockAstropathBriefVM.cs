using Kingmaker.Blueprints.Encyclopedia;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockAstropathBriefVM : EncyclopediaPageBlockVM
{
	public readonly ReactiveProperty<string> MessageLocation = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MessageDate = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MessageSender = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> MessageBody = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsMessageRead = new ReactiveProperty<bool>();

	public EncyclopediaPageBlockAstropathBriefVM(BlueprintEncyclopediaAstropathBriefPage.AstropathBriefBlock block)
		: base(block)
	{
		MessageLocation.Value = block.MessageLocation;
		MessageDate.Value = block.MessageDate;
		MessageSender.Value = block.MessageSender;
		MessageBody.Value = block.MessageBody;
		IsMessageRead.Value = block.IsMessageRead;
	}

	protected override void DisposeImplementation()
	{
	}
}
