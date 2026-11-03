using System;
using System.Collections.ObjectModel;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 출하 헤더 모델
    /// </summary>
    public class ShipmentModel : ViewModelBase
    {
        private string _shipmentNo;
        private DateTime _shipmentDate = DateTime.Today;
        private string _customerCode;
        private string _customerName;
        private string _orderNo;
        private string _status;
        private string _remark;
        private decimal _totalWeight;
        private decimal _totalAmount;

        public string ShipmentNo
        {
            get => _shipmentNo;
            set => SetProperty(ref _shipmentNo, value);
        }

        public DateTime ShipmentDate
        {
            get => _shipmentDate;
            set => SetProperty(ref _shipmentDate, value);
        }

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

        public string OrderNo
        {
            get => _orderNo;
            set => SetProperty(ref _orderNo, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }

        public decimal TotalWeight
        {
            get => _totalWeight;
            set => SetProperty(ref _totalWeight, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public ObservableCollection<ShipmentDetailModel> Details { get; } = new ObservableCollection<ShipmentDetailModel>();
    }

    /// <summary>
    /// 출하 상세 모델
    /// </summary>
    public class ShipmentDetailModel : ViewModelBase
    {
        private int _seq;
        private string _itemCode;
        private string _itemName;
        private decimal _quantity;
        private decimal _weight;
        private decimal _unitPrice;
        private decimal _amount;
        private string _remark;

        public int Seq
        {
            get => _seq;
            set => SetProperty(ref _seq, value);
        }

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

        public decimal Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public decimal Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }
    }
}
