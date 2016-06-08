using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryParserGui
{
    using System;

    public class CompilationException : Exception
    {
        public CompilationException()
        {
        }

        public CompilationException(string message)
            : base(message)
        {
        }

        public CompilationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
