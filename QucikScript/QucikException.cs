using System;

namespace QucikScript
{
    public class QucikException : Exception
    {
        public QucikException(string value) : base(value) { }
    }
}