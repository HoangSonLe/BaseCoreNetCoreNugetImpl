using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Interfaces;
using System.Net;

namespace BaseSourceImpl.Common.ErrorCodes
{
    public class AuthErrorCodes : IErrorCode
    {
        public string Code { get; }
        public string Message { get; }

        private AuthErrorCodes(string code, string message)
        {
            Code = code;
            Message = message;
        }

        // Define your custom error codes
        public static readonly AuthErrorCodes INVALID_CREDENTIALS =
       new AuthErrorCodes("AUTH_001", "Tên đăng nhập hoặc mật khẩu không đúng");

        public static readonly AuthErrorCodes ACCOUNT_LOCKED =
            new AuthErrorCodes("AUTH_002", "Tài khoản bị khóa");

        public static readonly AuthErrorCodes TOKEN_EXPIRED =
            new AuthErrorCodes("AUTH_003", "Token đã hết hạn");

        public static readonly AuthErrorCodes TOKEN_INVALID =
            new AuthErrorCodes("AUTH_004", "Token không hợp lệ");

        public static readonly AuthErrorCodes USER_NOT_FOUND =
            new AuthErrorCodes("AUTH_005", "Người dùng không tồn tại");

        public static readonly AuthErrorCodes UNAUTHORIZED =
            new AuthErrorCodes("AUTH_006", "Không có quyền truy cập");

        public static readonly AuthErrorCodes PASSWORD_WEAK =
            new AuthErrorCodes("AUTH_007", "Mật khẩu quá yếu");

        public static readonly AuthErrorCodes PASSWORD_EXPIRED =
            new AuthErrorCodes("AUTH_008", "Mật khẩu đã hết hạn");

        public static readonly AuthErrorCodes SESSION_EXPIRED =
            new AuthErrorCodes("AUTH_009", "Phiên đăng nhập đã hết hạn");
    }

    public class InvalidCredentialException : BaseApplicationException
    {
        public InvalidCredentialException()
            : base(AuthErrorCodes.INVALID_CREDENTIALS, AuthErrorCodes.INVALID_CREDENTIALS.Message, HttpStatusCode.BadRequest) { }

        public InvalidCredentialException(string message)
            : base(AuthErrorCodes.INVALID_CREDENTIALS, message, HttpStatusCode.BadRequest) { }
    }

    public class AccountLockedException : BaseApplicationException
    {
        public AccountLockedException()
            : base(AuthErrorCodes.ACCOUNT_LOCKED, AuthErrorCodes.ACCOUNT_LOCKED.Message, HttpStatusCode.Forbidden) { }

        public AccountLockedException(string message)
            : base(AuthErrorCodes.ACCOUNT_LOCKED, message, HttpStatusCode.Forbidden) { }
    }

    public class TokenExpiredException : BaseApplicationException
    {
        public TokenExpiredException()
            : base(AuthErrorCodes.TOKEN_EXPIRED, AuthErrorCodes.TOKEN_EXPIRED.Message, HttpStatusCode.Unauthorized) { }

        public TokenExpiredException(string message)
            : base(AuthErrorCodes.TOKEN_EXPIRED, message, HttpStatusCode.Unauthorized) { }
    }

    public class TokenInvalidException : BaseApplicationException
    {
        public TokenInvalidException()
            : base(AuthErrorCodes.TOKEN_INVALID, AuthErrorCodes.TOKEN_INVALID.Message, HttpStatusCode.Unauthorized) { }

        public TokenInvalidException(string message)
            : base(AuthErrorCodes.TOKEN_INVALID, message, HttpStatusCode.Unauthorized) { }
    }

    public class UnauthorizedAccessExceptionEx : BaseApplicationException
    {
        public UnauthorizedAccessExceptionEx()
            : base(AuthErrorCodes.UNAUTHORIZED, AuthErrorCodes.UNAUTHORIZED.Message, HttpStatusCode.Forbidden) { }

        public UnauthorizedAccessExceptionEx(string message)
            : base(AuthErrorCodes.UNAUTHORIZED, message, HttpStatusCode.Forbidden) { }
    }

    public class PasswordWeakException : BaseApplicationException
    {
        public PasswordWeakException()
            : base(AuthErrorCodes.PASSWORD_WEAK, AuthErrorCodes.PASSWORD_WEAK.Message, HttpStatusCode.BadRequest) { }

        public PasswordWeakException(string message)
            : base(AuthErrorCodes.PASSWORD_WEAK, message, HttpStatusCode.BadRequest) { }
    }

    public class PasswordExpiredException : BaseApplicationException
    {
        public PasswordExpiredException()
            : base(AuthErrorCodes.PASSWORD_EXPIRED, AuthErrorCodes.PASSWORD_EXPIRED.Message, HttpStatusCode.Unauthorized) { }

        public PasswordExpiredException(string message)
            : base(AuthErrorCodes.PASSWORD_EXPIRED, message, HttpStatusCode.Unauthorized) { }
    }

    public class SessionExpiredException : BaseApplicationException
    {
        public SessionExpiredException()
            : base(AuthErrorCodes.SESSION_EXPIRED, AuthErrorCodes.SESSION_EXPIRED.Message, HttpStatusCode.Unauthorized) { }

        public SessionExpiredException(string message)
            : base(AuthErrorCodes.SESSION_EXPIRED, message, HttpStatusCode.Unauthorized) { }
    }
}
