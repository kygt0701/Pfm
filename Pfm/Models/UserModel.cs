using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 사용자 모델
    /// </summary>
    public class UserModel : ViewModelBase
    {
        private string _userId;
        private string _userName;
        private string _password;
        private string _email;
        private string _tel;
        private string _department;
        private bool _isActive = true;

        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Tel
        {
            get => _tel;
            set => SetProperty(ref _tel, value);
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }

    /// <summary>
    /// 역할 모델
    /// </summary>
    public class RoleModel : ViewModelBase
    {
        private string _roleId;
        private string _roleName;
        private string _description;
        private bool _isActive = true;

        public string RoleId
        {
            get => _roleId;
            set => SetProperty(ref _roleId, value);
        }

        public string RoleName
        {
            get => _roleName;
            set => SetProperty(ref _roleName, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }
}
