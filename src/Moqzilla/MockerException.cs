using System;
using System.Collections.Generic;
using System.Text;

namespace Moqzilla
{
    public class MockerException : Exception
    {
        internal MockerException(MockerExceptionType exceptionType)
            : base(GetMessageFromExceptionType(exceptionType))
        {
        }

        private static string GetMessageFromExceptionType(MockerExceptionType exceptionType)
        {
            switch (exceptionType)
            {
                case MockerExceptionType.NoValidConstructors:
                    return "Mocker could not find constructors that consist entirely of interfaces.";
                default:
                    return "Mocker encountered an error.";
            }
        }
    }
}
