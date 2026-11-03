using System;
using Pfm.ViewModels.Base;

namespace Pfm.Models
{
    /// <summary>
    /// 미수금 모델
    /// </summary>
    public class ReceivableModel : ViewModelBase
    {
        private string _customerCode;
        private string _customerName;
        private DateTime _transactionDate;
        private string _transactionType;
        private string _documentNo;
        private decimal _salesAmount;
        private decimal _receivedAmount;
        private decimal _balance;
        private string _remark;

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

        public DateTime TransactionDate
        {
            get => _transactionDate;
            set => SetProperty(ref _transactionDate, value);
        }

        public string TransactionType
        {
            get => _transactionType;
            set => SetProperty(ref _transactionType, value);
        }

        public string DocumentNo
        {
            get => _documentNo;
            set => SetProperty(ref _documentNo, value);
        }

        public decimal SalesAmount
        {
            get => _salesAmount;
            set => SetProperty(ref _salesAmount, value);
        }

        public decimal ReceivedAmount
        {
            get => _receivedAmount;
            set => SetProperty(ref _receivedAmount, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public string Remark
        {
            get => _remark;
            set => SetProperty(ref _remark, value);
        }
    }
}
