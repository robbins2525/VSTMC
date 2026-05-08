/*--------------------------------------------------------------------------------------+
|   ScanCriteriaHelpers.h
|
+--------------------------------------------------------------------------------------*/
#pragma once

#include <stdexcept>
#include <vector>
#include <initializer_list>

#include <DgnPlatform/ScanCriteria.h>

namespace $rootnamespace$
{
    namespace BD = Bentley::DgnPlatform;

    struct ScanCriteriaHelpers
    {
        /// <summary>
        /// Adds element type filters to an existing scan criteria.
        /// </summary>
        static ScanCriteriaP AddElementTypes(
            ScanCriteriaP criteria,
            std::initializer_list<int> types)
        {
            if (nullptr == criteria)
                throw std::invalid_argument("ScanCriteria pointer cannot be null.");

            if (types.size() == 0)
                throw std::invalid_argument("At least one element type must be provided.");

            for (int type : types)
                criteria->AddSingleElementTypeTest(type);

            return criteria;
        }

        /// <summary>
        /// Adds element type filters to an existing scan criteria.
        /// </summary>
        static ScanCriteriaP AddElementTypes(
            ScanCriteriaP criteria,
            std::vector<int> const& types)
        {
            if (nullptr == criteria)
                throw std::invalid_argument("ScanCriteria pointer cannot be null.");

            if (types.empty())
                throw std::invalid_argument("At least one element type must be provided.");

            for (int type : types)
                criteria->AddSingleElementTypeTest(type);

            return criteria;
        }

        /// <summary>
        /// Creates a new scan criteria for the supplied model.
        /// </summary>
        static ScanCriteriaP CreateForModel(DgnModelRefP modelRef)
        {
            if (nullptr == modelRef)
                throw std::invalid_argument("DgnModelRef pointer cannot be null.");

            ScanCriteriaP criteria = BD::ScanCriteria::Create();

            if (nullptr == criteria)
                throw std::runtime_error("Failed to create ScanCriteria.");

            if (SUCCESS != criteria->SetModelRef(modelRef))
            {
                BD::ScanCriteria::Delete(criteria);
                throw std::runtime_error("Failed to assign model reference to ScanCriteria.");
            }

            return criteria;
        }

        /// <summary>
        /// Releases a scan criteria created by CreateForModel or ScanCriteria::Create.
        /// </summary>
        static void Destroy(ScanCriteriaP criteria)
        {
            if (nullptr != criteria)
                BD::ScanCriteria::Delete(criteria);
        }

        /// <summary>
        /// Scans using the supplied criteria and returns matching element references.
        /// </summary>
        static std::vector<ElementRefP> ScanElements(ScanCriteriaP criteria)
        {
            if (nullptr == criteria)
                throw std::invalid_argument("ScanCriteria pointer cannot be null.");

            std::vector<ElementRefP> result;

            criteria->SetElemRefCallback(&CollectElementRefCallback, &result);

            StatusInt status = criteria->Scan();

            switch (status)
            {
            case SUCCESS:
            case Bentley::DgnPlatform::END_OF_DGN:
                return result;

            default:
            {
                char buffer[128];
                sprintf_s(buffer, "Scan failed. Status=%d", static_cast<int>(status));
                throw std::runtime_error(buffer);
            }
            }
        }

    private:
        static int CollectElementRefCallback(
            ElementRefP elementRef,
            CallbackArgP callbackArg,
            ScanCriteriaP /*scP*/)
        {
            auto* result = reinterpret_cast<std::vector<ElementRefP>*>(callbackArg);

            if (nullptr != result && nullptr != elementRef)
                result->push_back(elementRef);

            return SUCCESS;
        }
    };
}