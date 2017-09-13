using NUnitLite;
using System;

namespace NValidate.Tests
{
    class TestRunner
    {
        public static int Main(string[] args)
	    {
	        return new AutoRun().Execute(args);
            Console.WriteLine("Here");
	    }
    }
}