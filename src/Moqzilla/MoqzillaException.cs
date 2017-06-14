using System;
using System.Collections.Generic;
using System.Text;

namespace Moqzilla
{
    public class MoqzillaException : Exception
    {
        internal MoqzillaException(MoqzillaExceptionType exceptionType)
            : base(GetMessageFromExceptionType(exceptionType))
        {
        }

        private static string GetMessageFromExceptionType(MoqzillaExceptionType exceptionType)
        {
            switch (exceptionType)
            {
                case MoqzillaExceptionType.NoValidConstructors:
                    return "Moqzilla could not find constructors that consist entirely of interfaces.";
                default:
                    return "Moqzilla encountered an error.";
            }
        }
    }
}
