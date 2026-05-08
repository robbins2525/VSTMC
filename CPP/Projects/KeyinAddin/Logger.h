#pragma once

#include <Bentley\Bentley.h>

namespace Diagnostics
{
    class Logger
    {
    public:
        static void Debug(Bentley::WCharCP message);
        static void Info(Bentley::WCharCP title, Bentley::WCharCP message);
        static void Warning(Bentley::WCharCP title, Bentley::WCharCP message);
        static void Error(Bentley::WCharCP title, Bentley::WCharCP message);
    };
}