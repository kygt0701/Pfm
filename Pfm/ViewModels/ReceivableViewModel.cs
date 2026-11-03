using System;
using System.Data;
using System.Data.SqlClient;
using Pfm.Common;
using Pfm.Services;
using Pfm.ViewModels.Base;

namespace Pfm.ViewModels
{
    /// <summary>
    /// 미수금 관리 ViewModel
    /// </summary>
    public class ReceivableViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _receivableList;
        private DataTable _customerList;
        private string _selectedCustomerCode;
        private DateTime _fromDate;
        private DateTime _toDate;
        private decimal _totalSales;
        private decimal _totalReceived;
        private decimal _totalBalance;
        #endregion

        #region 속성
        public DataTable ReceivableList
        {
            get => _receivableList;
            set => SetProperty(ref _receivableList, value);
        }

        public DataTable CustomerList
        {
            get => _customerList;
            set => SetProperty(ref _customerList, value);
        }

        public string SelectedCustomerCode
        {
            get => _selectedCustomerCode;
            set => SetProperty(ref _selectedCustomerCode, value);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public decimal TotalSales
        {
            get => _totalSales;
            set => SetProperty(ref _totalSales, value);
        }

        public decimal TotalReceived
        {
            get => _totalReceived;
            set => SetProperty(ref _totalReceived, value);
        }

        public decimal TotalBalance
        {
            get => _totalBalance;
            set => SetProperty(ref _totalBalance, value);
        }
        #endregion

        public ReceivableViewModel()
        {
            MenuId = "ACC_RECV";
            FormTitle = "미수금 관리";
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                string sql = "SELECT CustomerCode, CustomerName FROM Customer WHERE IsActive = 1 ORDER BY CustomerName";
                CustomerList = DbHelper.ExecuteQuery(sql);
            }
            catch { }
        }

        protected override void OnNew()
        {
            // 미수금은 직접 신규 생성하지 않음
            ServiceLocator.MessageService.ShowInfo("미수금은 출하/수금 처리를 통해 관리됩니다.");
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = @"SELECT r.CustomerCode, c.CustomerName, r.TransactionDate, r.TransactionType,
                                      r.DocumentNo, r.SalesAmount, r.ReceivedAmount, r.Balance, r.Remark
                               FROM Receivable r
                               INNER JOIN Customer c ON r.CustomerCode = c.CustomerCode
                               WHERE r.TransactionDate BETWEEN @FromDate AND @ToDate
                                 AND (@CustomerCode IS NULL OR r.CustomerCode = @CustomerCode)
                               ORDER BY r.TransactionDate DESC, r.CustomerCode";

                ReceivableList = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@FromDate", FromDate),
                    new SqlParameter("@ToDate", ToDate),
                    new SqlParameter("@CustomerCode", string.IsNullOrEmpty(SelectedCustomerCode) ? (object)DBNull.Value : SelectedCustomerCode));

                CalculateTotals();

                EndWork($"조회 완료: {ReceivableList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        private void CalculateTotals()
        {
            TotalSales = 0;
            TotalReceived = 0;
            TotalBalance = 0;

            if (ReceivableList == null) return;

            foreach (DataRow row in ReceivableList.Rows)
            {
                TotalSales += row["SalesAmount"] != DBNull.Value ? Convert.ToDecimal(row["SalesAmount"]) : 0;
                TotalReceived += row["ReceivedAmount"] != DBNull.Value ? Convert.ToDecimal(row["ReceivedAmount"]) : 0;
                TotalBalance += row["Balance"] != DBNull.Value ? Convert.ToDecimal(row["Balance"]) : 0;
            }
        }

        protected override void OnSave()
        {
            // 미수금 직접 수정은 제한
            ServiceLocator.MessageService.ShowInfo("미수금은 출하/수금 처리를 통해 관리됩니다.");
        }

        protected override void OnDelete()
        {
            // 미수금 직접 삭제는 제한
            ServiceLocator.MessageService.ShowInfo("미수금은 출하/수금 처리를 통해 관리됩니다.");
        }
    }
}
