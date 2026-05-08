/*--------------------------------------------------------------------------------------+
|   $safeitemname$.cs
|
+--------------------------------------------------------------------------------------*/

#region System Namespaces
using System;
using System.Collections.Generic;
#endregion

#region Bentley Namespaces
using BDPN = Bentley.DgnPlatformNET;
using BG = Bentley.GeometryNET;
using BM = Bentley.MstnPlatformNET;
#endregion

namespace $rootnamespace$
{
	class $safeitemrootname$(string unparsed = "")
	{
        // Example initialization parameter used by some Bentley add-ins
        private readonly string _unparsed = unparsed;
        public string Unparsed => _unparsed;
}
}

