using NUnitLite;
using System;

namespace NValidate.Tests
{
    class TestRunner
    {
        public static int Main(string[] args)
	    {
	        new AutoRun().Execute(args);
#if DEBUG
            Console.ReadKey();
#endif
            return 0;
        }
    }
}