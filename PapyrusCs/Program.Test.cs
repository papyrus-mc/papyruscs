using System;
using System.Collections.Generic;
using System.Text;

namespace PapyrusCs
{
    public partial class Program
    {
        private static int RunTestOptions(TestOptions opts)
        {
            if (opts.TestDbRead)
            {
                TestCommands.TestDbRead(opts);
            }
            else if (opts.Decode)
            {
                TestCommands.TestDecode(opts);
            }
            else if (opts.Smallflow)
            {
                TestCommands.TestSmallFlow(opts);
            }

            return 0;
        }
    }
}
