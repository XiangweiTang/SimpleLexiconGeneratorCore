﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleLexiconGeneratorCore
{
    public static class Sanity
    {
        public static void Requires(bool valid, string message = "Error!")
        {
            if (!valid)
                throw new Exception(message);
        }
    }
}
