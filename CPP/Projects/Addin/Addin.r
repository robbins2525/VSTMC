/*--------------------------------------------------------------------------------------+
|   $safeitemname$.r
|
+--------------------------------------------------------------------------------------*/

#include <Mstn\MdlApi\rscdefs.r.h>

/*----------------------------------------------------------------------+
| Required resource for a native-code-only application.                 |
-----------------------------------------------------------------------*/
#define DLLAPPID  1

DllMdlApp DLLAPPID =
    {
		L"$safeitemname$", L"$safeitemname$" // taskid, dllName
    }
