#include "$safeitemname$.h"
#include "ScanCriteriaHelpers.h"
#include <Mstn/ISessionMgr.h>
#include <DgnPlatform/ScanCriteria.h>

namespace $rootnamespace$
{
	int $safeitemname$()
	{
		using namespace Bentley::DgnPlatform;

		DgnModelRefP modelRef = ISessionMgr::GetActiveDgnModelP();

		if (nullptr == modelRef)
			throw std::runtime_error("No active model");

		ScanCriteriaP criteria = ScanCriteriaHelpers::CreateForModel(modelRef);

		if (nullptr == criteria)
			throw std::runtime_error("Failed to create ScanCriteria");

		criteria->SetDrawnElements();

		ScanCriteriaHelpers::AddElementTypes(criteria, {
			LINE_ELM,
			LINE_STRING_ELM,
			SHAPE_ELM
			});

		auto elements = ScanCriteriaHelpers::ScanElements(criteria);

		int count = static_cast<int>(elements.size());

		ScanCriteriaHelpers::Destroy(criteria);

		return count;
	}
}
