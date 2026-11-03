using System;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 거래처 모델
    /// </summary>
    public class CustomerModel : ViewModelBase
    {
        private string _customerCode;
        private string _customerName;
        private string _customerType;
        private string _businessNo;
        private string _ceoName;
        private string _tel;
        private string _fax;
        private string _email;
        private string _address;
        private string _remark;
        private bool _isActive = true;

        public string CustomerCode
        {
            get => _customerCode;
            set => SetProperty(ref _customerCode, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string CustomerType
        {
            get => _customerType;
            set => SetProperty(ref _customerType, value);
        }

        public string BusinessNo
        {
            get => _businessNo;
            set => SetProperty(ref _businessNo, value);
        }

        public string CeoName
        {
            get => _ceoName;
            set => SetProperty(ref _ceoName, value);
        }

        public string Tel
        {
            get => _tel;
            set => SetProperty(ref _tel, value);
        }

        public string Fax
        {
            get => _fax;
            set => SetProperty(ref _fax, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
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
