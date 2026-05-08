#pragma once

namespace Platform
{
    class BentleyApi
    {
    public:
        static bool OpenResources();
        static bool RegisterCommands();
        static bool LoadCommandTable();
    };
}