namespace Ccs.Ppg.Utility.Exceptions.Exceptions
{
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string? message) : base(message)
        {
        }
        public ResourceNotFoundException() : base()
        {
        }
    }
}
