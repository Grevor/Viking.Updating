using System;

namespace Viking.Updating
{
    public class UpdateGraphException : Exception
    {
        public UpdateGraphException(string message) : base(FormattableString.Invariant($"Exception while updating: {message}")) { }
        public UpdateGraphException(string message, Exception e) : base(FormattableString.Invariant($"Exception while updating: {message}"), e) { }
    }
}
