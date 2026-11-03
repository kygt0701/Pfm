using System;
using System.Collections.ObjectModel;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 주문 헤더 모델
    /// </summary>
    public class OrderModel : ViewModelBase
    {
        private string _orderNo;
        private DateTime _orderDate = DateTime.Today;
        private string _customerCode;
        private string _customerName;
        private string _orderType;
        private string _status;
        private string _remark;
        private decimal _totalWeight;
        private decimal _totalAmount;

        public string OrderNo
        {
            get => _orderNo;
            set => SetProperty(ref _orderNo, value);
        }

        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
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

        public string OrderType
        {
            get => _orderType;
            set => SetProperty(ref _orderType, value);
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

        public ObservableCollection<OrderDetailModel> Details { get; } = new ObservableCollection<OrderDetailModel>();
    }

    /// <summary>
    /// 주문 상세 모델
    /// </summary>
    public class OrderDetailModel : ViewModelBase
    {
        private int _seq;
        private string _itemName;
        private string _shape;
        private string _grade;
        private decimal _diameter;
        private decimal _thickness;
        private decimal _width;
        private decimal _length;
        private int _quantity;
        private decimal _specificGravity;
        private string _division;
        private decimal _unitWeight;
        private decimal _totalWeight;
        private decimal _unitPrice;
        private decimal _amount;
        private string _remark;

        public int Seq
        {
            get => _seq;
            set => SetProperty(ref _seq, value);
        }

        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        public string Shape
        {
            get => _shape;
            set => SetProperty(ref _shape, value);
        }

        public string Grade
        {
            get => _grade;
            set => SetProperty(ref _grade, value);
        }

        public decimal Diameter
        {
            get => _diameter;
            set => SetProperty(ref _diameter, value);
        }

        public decimal Thickness
        {
            get => _thickness;
            set => SetProperty(ref _thickness, value);
        }

        public decimal Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public decimal Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public decimal SpecificGravity
        {
            get => _specificGravity;
            set => SetProperty(ref _specificGravity, value);
        }

        public string Division
        {
            get => _division;
            set => SetProperty(ref _division, value);
        }

        public decimal UnitWeight
        {
            get => _unitWeight;
            set => SetProperty(ref _unitWeight, value);
        }

        public decimal TotalWeight
        {
            get => _totalWeight;
            set => SetProperty(ref _totalWeight, value);
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
