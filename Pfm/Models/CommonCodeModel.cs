using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 공통코드 그룹 모델
    /// </summary>
    public class CodeGroupModel : ViewModelBase
    {
        private string _groupCode;
        private string _groupName;
        private string _description;
        private bool _isActive = true;

        public string GroupCode
        {
            get => _groupCode;
            set => SetProperty(ref _groupCode, value);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
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

    /// <summary>
    /// 공통코드 상세 모델
    /// </summary>
    public class CodeDetailModel : ViewModelBase
    {
        private string _groupCode;
        private string _code;
        private string _codeName;
        private int _sortOrder;
        private string _remark;
        private bool _isActive = true;

        public string GroupCode
        {
            get => _groupCode;
            set => SetProperty(ref _groupCode, value);
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public string CodeName
        {
            get => _codeName;
            set => SetProperty(ref _codeName, value);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
        }

        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
    }
}
