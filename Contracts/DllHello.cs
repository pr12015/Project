﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    class DllHello
    { 
        public DllHello() { }

        public void Greet(string message) { Console.WriteLine("Greetings {0}!", message); }
    }
}
