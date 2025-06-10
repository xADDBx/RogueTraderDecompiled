using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker;

public class TooltipBrickPetInfo : ITooltipBrick
{
	private PetKeystoneInfoComponent m_petKeystoneInfoComponent;

	private BaseUnitEntity m_PetEntity;

	public TooltipBrickPetInfo(PetKeystoneInfoComponent petKeystoneInfoComponent, BaseUnitEntity pet)
	{
		m_petKeystoneInfoComponent = petKeystoneInfoComponent;
		m_PetEntity = pet;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPetInfoVM(m_petKeystoneInfoComponent, m_PetEntity);
	}
}
