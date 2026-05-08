#pragma suppressREQCmds

#include "$safeprojectname$ids.h"

#include <Mstn\MdlApi\rscdefs.r.h>
#include <Mstn\MdlApi\cmdclass.r.h>

// -----------------------------------------------------------------------------
// Command numbering best practices
//
// Command Tables : 0–9
// Commands       : 10+
//
// This avoids collisions where MicroStation interprets a command number
// as another command table, which can cause recursive key-in levels.
// -----------------------------------------------------------------------------

#define CT_NONE                 0
#define CT_MAIN                 1
#define CT_COMMANDS             2

#define CMD_$safeprojectname_UPPER$_HELLO  10
#define CMD_$safeprojectname_UPPER$_ABOUT  11

CommandTable CT_MAIN =
{
    { 1, CT_COMMANDS, SHOW, REQ, "$safeprojectname_UPPER$" }
};

CommandTable CT_COMMANDS =
{
    { 1, CMD_$safeprojectname_UPPER$_HELLO, INHERIT, NONE, "HELLO" },
    { 2, CMD_$safeprojectname_UPPER$_ABOUT, INHERIT, NONE, "ABOUT" }
};