#include "BentleyApi.h"
#include "commands\KeyinHandlers.h"
#include "diagnostics\Logger.h"
#include "$safeprojectname$cmd.h"

#include <Mstn\MdlApi\mdlapi.h>

namespace
{
    RscFileHandle s_rscFile = 0;

    extern "C" void cmd_Hello(WCharCP unparsed)
    {
        Commands::KeyinHandlers::Hello(unparsed);
    }

    extern "C" void cmd_About(WCharCP unparsed)
    {
        Commands::KeyinHandlers::About(unparsed);
    }

    static MdlCommandNumber s_commandNumbers[] =
    {
        { (CmdHandler)cmd_Hello, CMD_$safeprojectname_UPPER$_HELLO },
        { (CmdHandler)cmd_About, CMD_$safeprojectname_UPPER$_ABOUT },        
        0
    };
}

namespace Platform
{
    bool BentleyApi::OpenResources()
    {
        mdlResource_openFile(&s_rscFile, nullptr, 0);
        return true;
    }

    bool BentleyApi::RegisterCommands()
    {
        mdlSystem_registerCommandNumbers(s_commandNumbers);
        return true;
    }

    bool BentleyApi::LoadCommandTable()
    {
        mdlParse_loadCommandTable(nullptr);
        return true;
    }
}