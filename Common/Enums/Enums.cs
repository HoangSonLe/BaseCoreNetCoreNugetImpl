using System.ComponentModel;

namespace BaseSourceImpl.Common.Enums
{
    public enum ETypeAccount
    {
        ADMIN,
        DEV
    }
    public enum EGender
    {
        [Description("Nam")]
        MALE,
        [Description("Nữ")]
        FEMALE,
        [Description("Khác")]
        OTHER
    }
}
