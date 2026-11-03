using System;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 재고 모델
    /// </summary>
    public class InventoryModel : ViewModelBase
    {
        private string _itemCode;
        private string _itemName;
        private string _warehouseCode;
        private string _warehouseName;
        private string _lotNo;
        private decimal _quantity;
        private string _unit;
        private DateTime _lastUpdated;

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

        public string WarehouseCode
        {
            get => _warehouseCode;
            set => SetProperty(ref _warehouseCode, value);
        }

        public string WarehouseName
        {
            get => _warehouseName;
            set => SetProperty(ref _warehouseName, value);
        }

        public string LotNo
        {
            get => _lotNo;
            set => SetProperty(ref _lotNo, value);
        }

        public decimal Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }
    }
}
