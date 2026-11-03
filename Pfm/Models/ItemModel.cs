using System;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 품목 모델
    /// </summary>
    public class ItemModel : ViewModelBase
    {
        private string _itemCode;
        private string _itemName;
        private string _itemType;
        private string _category;
        private string _unit;
        private decimal _unitPrice;
        private string _spec;
        private string _remark;
        private bool _isActive = true;

        public string ItemCode
        {
            get => _itemCode;
            set => SetProperty(ref _itemCode, value);
        }

        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        public string ItemType
        {
            get => _itemType;
            set => SetProperty(ref _itemType, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public string Spec
        {
            get => _spec;
            set => SetProperty(ref _spec, value);
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
