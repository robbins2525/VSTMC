#include "$safeitemname$.h"

static void ProcessElement(ElemAgendaEntry const& eeh);

/// <summary>
/// Entry point for the $safeitemname$ command.
/// Builds an agenda from the current native selection set
/// and processes each selected element.
/// </summary>
/// <remarks>
/// Requires that elements are pre-selected using MicroStation's
/// native selection tools before invoking the command.
/// </remarks>
void $safeitemname$::Run()
{
    ElementAgenda agenda;

    StatusInt status = SelectionSetManager::GetManager().BuildAgenda(agenda);
    if (SUCCESS != status || agenda.GetCount() == 0)
    {
        NotificationManager::OutputPrompt(
            L"No elements are currently selected. Use native selection first, then run SELECTIONMULTI."
        );
        return;
    }

    WString summary;
    summary.Sprintf(
        L"Processing %u selected element(s).",
        static_cast<unsigned>(agenda.GetCount())
    );
    NotificationManager::OutputPrompt(summary.c_str());

    for (uint32_t i = 0; i < agenda.GetCount(); ++i)
    {
        ElemAgendaEntry const& eeh = agenda[i];
        if (!eeh.IsValid())
            continue;

        ProcessElement(eeh);;
    }
}

/// <summary>
/// Processes a single element from the selection set.
/// </summary>
/// <param name="eeh">
/// The element agenda entry representing the selected element.
/// </param>
/// <remarks>
/// This is where custom batch logic should be implemented,
/// such as inspecting, modifying, or reporting element data.
/// </remarks>
static void ProcessElement(ElemAgendaEntry const& eeh)
{
    // Replace with your logic - based on current logic you only see last element ID
    WString msg;
    msg.Sprintf(
        L"Processing element ID=%llu",
        static_cast<unsigned long long>(eeh.GetElementId())
    );
    NotificationManager::OutputPrompt(msg.c_str());    
}