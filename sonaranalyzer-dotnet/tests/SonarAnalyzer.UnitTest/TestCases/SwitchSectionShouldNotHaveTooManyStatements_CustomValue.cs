﻿using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program()
        {
            switch (myVariable)
            {
                case 0:
                    break;
                case 1: // Noncompliant {{Reduce this 'switch/case' number of lines from 6 to at most 1, for example by extracting code into a method.}}
//              ^^^^^^^
                    Console.WriteLine("1");
                    Console.WriteLine("2");
                    Console.WriteLine("3");
                    Console.WriteLine("4");
                    Console.WriteLine("5");
                    break;
                default:
                    break;
            }
        }
    }
}
