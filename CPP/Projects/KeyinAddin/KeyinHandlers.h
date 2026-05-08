#pragma once

#include <Bentley\Bentley.h>

namespace Commands
{
    class KeyinHandlers
    {
    public:
        static void Hello(Bentley::WCharCP unparsed);
        static void About(Bentley::WCharCP unparsed);
    };
}