using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayNghien.Infrastructures.CommonMessage
{
    public static class AuthResponseMessage
    {
        public const string ERR_MSG_UserNotFound = "Thông tin đăng nhập không chính xác";
        public const string ERR_MSG_UserNotConFirmed = "Tài khoản chưa được kích hoạt";
        public const string ERR_MSG_UserExisted = "Email này đã tồn tại";
        public const string ERR_MSG_EmailIsNullOrEmpty = "Phải nhập email";
        public const string INFO_MSG_UserCreated = "Tạo tài khoản thành công, vui xòng chờ admin kích hoạt";
        public const string INFO_MSG_UserDeleted = "Đã xóa tài khoản thành công ";
        public const string INFO_MSG_UserLogout = "Da Dang Xuat";
    }
}
