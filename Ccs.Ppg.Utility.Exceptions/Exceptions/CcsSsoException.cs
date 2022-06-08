
namespace Ccs.Ppg.Utility.Exceptions.Exceptions
{
    public class CcsSsoException : Exception
    {
        public CcsSsoException()
                : base()
        {
        }

        public CcsSsoException(string errorCode)
            : base(errorCode)
        {
        }
    }
}
