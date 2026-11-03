using System;
using System.Data;
using System.Data.SqlClient;
using Pfm.Common;
using Pfm.Models;
using Pfm.Services;
using Pfm.ViewModels.Base;

namespace Pfm.ViewModels
{
    /// <summary>
    /// 출하 관리 ViewModel
    /// </summary>
    public class ShipmentViewModel : FormViewModelBase
    {
        #region 필드
        private ShipmentModel _currentShipment;
        private DataTable _detailList;
        private DataTable _customerList;
        private bool _isNewMode;
        #endregion

        #region 속성
        public ShipmentModel CurrentShipment
        {
            get => _currentShipment;
            set => SetProperty(ref _currentShipment, value);
        }

        public DataTable DetailList
        {
            get => _detailList;
            set => SetProperty(ref _detailList, value);
        }

        public DataTable CustomerList
        {
            get => _customerList;
            set => SetProperty(ref _customerList, value);
        }

        public bool IsNewMode
        {
            get => _isNewMode;
            set => SetProperty(ref _isNewMode, value);
        }
        #endregion

        public ShipmentViewModel()
        {
            MenuId = "SHP_SHIP";
            FormTitle = "출하 관리";
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadCustomers();
            InitializeDetailTable();
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

        private void InitializeDetailTable()
        {
            DetailList = new DataTable();
            DetailList.Columns.Add("Seq", typeof(int));
            DetailList.Columns.Add("ItemCode", typeof(string));
            DetailList.Columns.Add("ItemName", typeof(string));
            DetailList.Columns.Add("Quantity", typeof(decimal));
            DetailList.Columns.Add("Weight", typeof(decimal));
            DetailList.Columns.Add("UnitPrice", typeof(decimal));
            DetailList.Columns.Add("Amount", typeof(decimal));
            DetailList.Columns.Add("Remark", typeof(string));
        }

        protected override void OnNew()
        {
            IsNewMode = true;
            CurrentShipment = new ShipmentModel
            {
                ShipmentNo = GenerateShipmentNo(),
                ShipmentDate = DateTime.Today,
                Status = "대기"
            };
            InitializeDetailTable();
            StatusMessage = "신규 출하 입력";
        }

        protected override void OnSearch()
        {
            ServiceLocator.MessageService.ShowInfo("출하번호를 입력하거나 목록에서 선택하세요.");
        }

        public void LoadShipment(string shipmentNo)
        {
            try
            {
                BeginWork("조회 중...");

                string headerSql = @"SELECT s.ShipmentNo, s.ShipmentDate, s.CustomerCode, c.CustomerName,
                                            s.OrderNo, s.Status, s.Remark, s.TotalWeight, s.TotalAmount
                                     FROM Shipment s
                                     LEFT JOIN Customer c ON s.CustomerCode = c.CustomerCode
                                     WHERE s.ShipmentNo = @ShipmentNo";

                var headerDt = DbHelper.ExecuteQuery(headerSql, new SqlParameter("@ShipmentNo", shipmentNo));
                if (headerDt.Rows.Count == 0)
                {
                    ServiceLocator.MessageService.ShowWarning("출하 정보를 찾을 수 없습니다.");
                    return;
                }

                var row = headerDt.Rows[0];
                CurrentShipment = new ShipmentModel
                {
                    ShipmentNo = row["ShipmentNo"].ToString(),
                    ShipmentDate = Convert.ToDateTime(row["ShipmentDate"]),
                    CustomerCode = row["CustomerCode"].ToString(),
                    CustomerName = row["CustomerName"].ToString(),
                    OrderNo = row["OrderNo"].ToString(),
                    Status = row["Status"].ToString(),
                    Remark = row["Remark"].ToString(),
                    TotalWeight = row["TotalWeight"] != DBNull.Value ? Convert.ToDecimal(row["TotalWeight"]) : 0,
                    TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0
                };

                string detailSql = @"SELECT Seq, ItemCode, ItemName, Quantity, Weight, UnitPrice, Amount, Remark
                                     FROM ShipmentDetail
                                     WHERE ShipmentNo = @ShipmentNo
                                     ORDER BY Seq";

                DetailList = DbHelper.ExecuteQuery(detailSql, new SqlParameter("@ShipmentNo", shipmentNo));
                IsNewMode = false;

                EndWork("조회 완료");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            if (CurrentShipment == null)
            {
                ServiceLocator.MessageService.ShowWarning("저장할 출하 정보가 없습니다.");
                return;
            }

            if (string.IsNullOrEmpty(CurrentShipment.CustomerCode))
            {
                ServiceLocator.MessageService.ShowWarning("거래처를 선택하세요.");
                return;
            }

            try
            {
                BeginWork("저장 중...");

                CalculateTotals();

                if (IsNewMode)
                {
                    SaveNewShipment();
                }
                else
                {
                    UpdateShipment();
                }

                EndWork("저장 완료");
                ServiceLocator.MessageService.ShowInfo("저장되었습니다.");
                IsNewMode = false;
            }
            catch (Exception ex)
            {
                EndWork("저장 실패");
                ServiceLocator.MessageService.ShowError($"저장 오류: {ex.Message}");
            }
        }

        private void SaveNewShipment()
        {
            string headerSql = @"INSERT INTO Shipment 
                                (ShipmentNo, ShipmentDate, CustomerCode, OrderNo, Status, Remark, 
                                 TotalWeight, TotalAmount, CreatedBy, CreatedAt)
                                VALUES 
                                (@ShipmentNo, @ShipmentDate, @CustomerCode, @OrderNo, @Status, @Remark,
                                 @TotalWeight, @TotalAmount, @CreatedBy, GETDATE())";

            DbHelper.ExecuteNonQuery(headerSql,
                new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo),
                new SqlParameter("@ShipmentDate", CurrentShipment.ShipmentDate),
                new SqlParameter("@CustomerCode", CurrentShipment.CustomerCode),
                new SqlParameter("@OrderNo", CurrentShipment.OrderNo ?? (object)DBNull.Value),
                new SqlParameter("@Status", CurrentShipment.Status ?? "대기"),
                new SqlParameter("@Remark", CurrentShipment.Remark ?? (object)DBNull.Value),
                new SqlParameter("@TotalWeight", CurrentShipment.TotalWeight),
                new SqlParameter("@TotalAmount", CurrentShipment.TotalAmount),
                new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

            SaveDetails();
        }

        private void UpdateShipment()
        {
            string headerSql = @"UPDATE Shipment SET 
                                ShipmentDate = @ShipmentDate, CustomerCode = @CustomerCode,
                                OrderNo = @OrderNo, Status = @Status, Remark = @Remark,
                                TotalWeight = @TotalWeight, TotalAmount = @TotalAmount,
                                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                WHERE ShipmentNo = @ShipmentNo";

            DbHelper.ExecuteNonQuery(headerSql,
                new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo),
                new SqlParameter("@ShipmentDate", CurrentShipment.ShipmentDate),
                new SqlParameter("@CustomerCode", CurrentShipment.CustomerCode),
                new SqlParameter("@OrderNo", CurrentShipment.OrderNo ?? (object)DBNull.Value),
                new SqlParameter("@Status", CurrentShipment.Status ?? "대기"),
                new SqlParameter("@Remark", CurrentShipment.Remark ?? (object)DBNull.Value),
                new SqlParameter("@TotalWeight", CurrentShipment.TotalWeight),
                new SqlParameter("@TotalAmount", CurrentShipment.TotalAmount),
                new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));

            DbHelper.ExecuteNonQuery("DELETE FROM ShipmentDetail WHERE ShipmentNo = @ShipmentNo",
                new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo));

            SaveDetails();
        }

        private void SaveDetails()
        {
            if (DetailList == null) return;

            int seq = 1;
            foreach (DataRow row in DetailList.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                string sql = @"INSERT INTO ShipmentDetail 
                              (ShipmentNo, Seq, ItemCode, ItemName, Quantity, Weight, UnitPrice, Amount, Remark)
                              VALUES 
                              (@ShipmentNo, @Seq, @ItemCode, @ItemName, @Quantity, @Weight, @UnitPrice, @Amount, @Remark)";

                DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo),
                    new SqlParameter("@Seq", seq++),
                    new SqlParameter("@ItemCode", row["ItemCode"] ?? DBNull.Value),
                    new SqlParameter("@ItemName", row["ItemName"] ?? DBNull.Value),
                    new SqlParameter("@Quantity", row["Quantity"] ?? 0),
                    new SqlParameter("@Weight", row["Weight"] ?? 0),
                    new SqlParameter("@UnitPrice", row["UnitPrice"] ?? 0),
                    new SqlParameter("@Amount", row["Amount"] ?? 0),
                    new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value));
            }
        }

        protected override void OnDelete()
        {
            if (CurrentShipment == null || string.IsNullOrEmpty(CurrentShipment.ShipmentNo))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 출하를 선택하세요.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"출하번호 '{CurrentShipment.ShipmentNo}'을(를) 삭제하시겠습니까?"))
                return;

            try
            {
                BeginWork("삭제 중...");

                DbHelper.ExecuteNonQuery("DELETE FROM ShipmentDetail WHERE ShipmentNo = @ShipmentNo",
                    new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo));

                DbHelper.ExecuteNonQuery("DELETE FROM Shipment WHERE ShipmentNo = @ShipmentNo",
                    new SqlParameter("@ShipmentNo", CurrentShipment.ShipmentNo));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                OnNew();
            }
            catch (Exception ex)
            {
                EndWork("삭제 실패");
                ServiceLocator.MessageService.ShowError($"삭제 오류: {ex.Message}");
            }
        }

        public void CalculateTotals()
        {
            if (DetailList == null || CurrentShipment == null) return;

            decimal totalWeight = 0;
            decimal totalAmount = 0;

            foreach (DataRow row in DetailList.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                totalWeight += row["Weight"] != DBNull.Value ? Convert.ToDecimal(row["Weight"]) : 0;
                totalAmount += row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
            }

            CurrentShipment.TotalWeight = totalWeight;
            CurrentShipment.TotalAmount = totalAmount;
        }

        private string GenerateShipmentNo()
        {
            return $"SHP{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
