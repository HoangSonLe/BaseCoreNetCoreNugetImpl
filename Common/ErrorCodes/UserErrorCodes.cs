using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Interfaces;
using System.Net;

namespace BaseSourceImpl.Common.ErrorCodes
{
    public class UserErrorCodes : IErrorCode
    {
        public string Code { get; }
        public string Message { get; }

        private UserErrorCodes(string code, string message)
        {
            Code = code;
            Message = message;
        }

        // Define your custom error codes
        public static readonly UserErrorCodes USER_NOT_FOUND =
            new UserErrorCodes("USER_001", "Người dùng không tồn tại");

        public static readonly UserErrorCodes USER_DUPLICATE =
             new UserErrorCodes("USER_002", "Người dùng đã tồn tại");
    }
    public class UserDuplicateException : BaseApplicationException
    {
        public UserDuplicateException()
             : base(UserErrorCodes.USER_DUPLICATE, UserErrorCodes.USER_DUPLICATE.Message, HttpStatusCode.Conflict)
        {
        }

        public UserDuplicateException(string message)
                : base(UserErrorCodes.USER_DUPLICATE, message, HttpStatusCode.Conflict)
        {
        }
    }
    public class UserNotFoundException : BaseApplicationException
    {
        public UserNotFoundException()
             : base(UserErrorCodes.USER_NOT_FOUND, UserErrorCodes.USER_NOT_FOUND.Message, HttpStatusCode.NotFound)
        {
        }

        public UserNotFoundException(string message)
                : base(UserErrorCodes.USER_NOT_FOUND, message, HttpStatusCode.NotFound)
        {
        }
    }


}
