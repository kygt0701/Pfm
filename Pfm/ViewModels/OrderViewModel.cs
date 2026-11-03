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
    /// 주문 관리 ViewModel
    /// </summary>
    public class OrderViewModel : FormViewModelBase
    {
        #region 필드
        private OrderModel _currentOrder;
        private DataTable _detailList;
        private DataTable _customerList;
        private bool _isNewMode;
        #endregion

        #region 속성
        public OrderModel CurrentOrder
        {
            get => _currentOrder;
            set => SetProperty(ref _currentOrder, value);
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

        public OrderViewModel()
        {
            MenuId = "ORD_ORDER";
            FormTitle = "발주 관리";
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
                string sql = "SELECT CustomerCode, CustomerName, CustomerType FROM Customer WHERE IsActive = 1 ORDER BY CustomerName";
                CustomerList = DbHelper.ExecuteQuery(sql);
            }
            catch { }
        }

        private void InitializeDetailTable()
        {
            DetailList = new DataTable();
            DetailList.Columns.Add("Seq", typeof(int));
            DetailList.Columns.Add("ItemName", typeof(string));
            DetailList.Columns.Add("Shape", typeof(string));
            DetailList.Columns.Add("Grade", typeof(string));
            DetailList.Columns.Add("Diameter", typeof(decimal));
            DetailList.Columns.Add("Thickness", typeof(decimal));
            DetailList.Columns.Add("Width", typeof(decimal));
            DetailList.Columns.Add("Length", typeof(decimal));
            DetailList.Columns.Add("Quantity", typeof(int));
            DetailList.Columns.Add("SpecificGravity", typeof(decimal));
            DetailList.Columns.Add("Division", typeof(string));
            DetailList.Columns.Add("UnitWeight", typeof(decimal));
            DetailList.Columns.Add("TotalWeight", typeof(decimal));
            DetailList.Columns.Add("UnitPrice", typeof(decimal));
            DetailList.Columns.Add("Amount", typeof(decimal));
            DetailList.Columns.Add("Remark", typeof(string));
        }

        protected override void OnNew()
        {
            IsNewMode = true;
            CurrentOrder = new OrderModel
            {
                OrderNo = GenerateOrderNo(),
                OrderDate = DateTime.Today,
                OrderType = "발주",
                Status = "접수"
            };
            InitializeDetailTable();
            StatusMessage = "신규 발주 입력";
        }

        protected override void OnSearch()
        {
            // 발주 목록 조회 팝업 또는 검색 기능
            ServiceLocator.MessageService.ShowInfo("발주번호를 입력하거나 목록에서 선택하세요.");
        }

        public void LoadOrder(string orderNo)
        {
            try
            {
                BeginWork("조회 중...");

                // 헤더 조회
                string headerSql = @"SELECT o.OrderNo, o.OrderDate, o.CustomerCode, c.CustomerName,
                                            o.OrderType, o.Status, o.Remark, o.TotalWeight, o.TotalAmount
                                     FROM [Order] o
                                     LEFT JOIN Customer c ON o.CustomerCode = c.CustomerCode
                                     WHERE o.OrderNo = @OrderNo";

                var headerDt = DbHelper.ExecuteQuery(headerSql, new SqlParameter("@OrderNo", orderNo));
                if (headerDt.Rows.Count == 0)
                {
                    ServiceLocator.MessageService.ShowWarning("발주 정보를 찾을 수 없습니다.");
                    return;
                }

                var row = headerDt.Rows[0];
                CurrentOrder = new OrderModel
                {
                    OrderNo = row["OrderNo"].ToString(),
                    OrderDate = Convert.ToDateTime(row["OrderDate"]),
                    CustomerCode = row["CustomerCode"].ToString(),
                    CustomerName = row["CustomerName"].ToString(),
                    OrderType = row["OrderType"].ToString(),
                    Status = row["Status"].ToString(),
                    Remark = row["Remark"].ToString(),
                    TotalWeight = row["TotalWeight"] != DBNull.Value ? Convert.ToDecimal(row["TotalWeight"]) : 0,
                    TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0
                };

                // 상세 조회
                string detailSql = @"SELECT Seq, ItemName, Shape, Grade, Diameter, Thickness, Width, Length,
                                            Quantity, SpecificGravity, Division, UnitWeight, TotalWeight,
                                            UnitPrice, Amount, Remark
                                     FROM OrderDetail
                                     WHERE OrderNo = @OrderNo
                                     ORDER BY Seq";

                DetailList = DbHelper.ExecuteQuery(detailSql, new SqlParameter("@OrderNo", orderNo));
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
            if (CurrentOrder == null)
            {
                ServiceLocator.MessageService.ShowWarning("저장할 발주 정보가 없습니다.");
                return;
            }

            if (string.IsNullOrEmpty(CurrentOrder.CustomerCode))
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
                    SaveNewOrder();
                }
                else
                {
                    UpdateOrder();
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

        private void SaveNewOrder()
        {
            // 헤더 저장
            string headerSql = @"INSERT INTO [Order] 
                                (OrderNo, OrderDate, CustomerCode, OrderType, Status, Remark, 
                                 TotalWeight, TotalAmount, CreatedBy, CreatedAt)
                                VALUES 
                                (@OrderNo, @OrderDate, @CustomerCode, @OrderType, @Status, @Remark,
                                 @TotalWeight, @TotalAmount, @CreatedBy, GETDATE())";

            DbHelper.ExecuteNonQuery(headerSql,
                new SqlParameter("@OrderNo", CurrentOrder.OrderNo),
                new SqlParameter("@OrderDate", CurrentOrder.OrderDate),
                new SqlParameter("@CustomerCode", CurrentOrder.CustomerCode),
                new SqlParameter("@OrderType", CurrentOrder.OrderType ?? "발주"),
                new SqlParameter("@Status", CurrentOrder.Status ?? "접수"),
                new SqlParameter("@Remark", CurrentOrder.Remark ?? (object)DBNull.Value),
                new SqlParameter("@TotalWeight", CurrentOrder.TotalWeight),
                new SqlParameter("@TotalAmount", CurrentOrder.TotalAmount),
                new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

            // 상세 저장
            SaveDetails();
        }

        private void UpdateOrder()
        {
            // 헤더 수정
            string headerSql = @"UPDATE [Order] SET 
                                OrderDate = @OrderDate, CustomerCode = @CustomerCode,
                                OrderType = @OrderType, Status = @Status, Remark = @Remark,
                                TotalWeight = @TotalWeight, TotalAmount = @TotalAmount,
                                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                WHERE OrderNo = @OrderNo";

            DbHelper.ExecuteNonQuery(headerSql,
                new SqlParameter("@OrderNo", CurrentOrder.OrderNo),
                new SqlParameter("@OrderDate", CurrentOrder.OrderDate),
                new SqlParameter("@CustomerCode", CurrentOrder.CustomerCode),
                new SqlParameter("@OrderType", CurrentOrder.OrderType ?? "발주"),
                new SqlParameter("@Status", CurrentOrder.Status ?? "접수"),
                new SqlParameter("@Remark", CurrentOrder.Remark ?? (object)DBNull.Value),
                new SqlParameter("@TotalWeight", CurrentOrder.TotalWeight),
                new SqlParameter("@TotalAmount", CurrentOrder.TotalAmount),
                new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));

            // 상세 삭제 후 재입력
            DbHelper.ExecuteNonQuery("DELETE FROM OrderDetail WHERE OrderNo = @OrderNo",
                new SqlParameter("@OrderNo", CurrentOrder.OrderNo));

            SaveDetails();
        }

        private void SaveDetails()
        {
            if (DetailList == null) return;

            int seq = 1;
            foreach (DataRow row in DetailList.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                string sql = @"INSERT INTO OrderDetail 
                              (OrderNo, Seq, ItemName, Shape, Grade, Diameter, Thickness, Width, Length,
                               Quantity, SpecificGravity, Division, UnitWeight, TotalWeight, UnitPrice, Amount, Remark)
                              VALUES 
                              (@OrderNo, @Seq, @ItemName, @Shape, @Grade, @Diameter, @Thickness, @Width, @Length,
                               @Quantity, @SpecificGravity, @Division, @UnitWeight, @TotalWeight, @UnitPrice, @Amount, @Remark)";

                DbHelper.ExecuteNonQuery(sql,
                    new SqlParameter("@OrderNo", CurrentOrder.OrderNo),
                    new SqlParameter("@Seq", seq++),
                    new SqlParameter("@ItemName", row["ItemName"] ?? DBNull.Value),
                    new SqlParameter("@Shape", row["Shape"] ?? DBNull.Value),
                    new SqlParameter("@Grade", row["Grade"] ?? DBNull.Value),
                    new SqlParameter("@Diameter", row["Diameter"] ?? 0),
                    new SqlParameter("@Thickness", row["Thickness"] ?? 0),
                    new SqlParameter("@Width", row["Width"] ?? 0),
                    new SqlParameter("@Length", row["Length"] ?? 0),
                    new SqlParameter("@Quantity", row["Quantity"] ?? 0),
                    new SqlParameter("@SpecificGravity", row["SpecificGravity"] ?? 0),
                    new SqlParameter("@Division", row["Division"] ?? DBNull.Value),
                    new SqlParameter("@UnitWeight", row["UnitWeight"] ?? 0),
                    new SqlParameter("@TotalWeight", row["TotalWeight"] ?? 0),
                    new SqlParameter("@UnitPrice", row["UnitPrice"] ?? 0),
                    new SqlParameter("@Amount", row["Amount"] ?? 0),
                    new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value));
            }
        }

        protected override void OnDelete()
        {
            if (CurrentOrder == null || string.IsNullOrEmpty(CurrentOrder.OrderNo))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 발주를 선택하세요.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"발주번호 '{CurrentOrder.OrderNo}'을(를) 삭제하시겠습니까?"))
                return;

            try
            {
                BeginWork("삭제 중...");

                // 상세 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM OrderDetail WHERE OrderNo = @OrderNo",
                    new SqlParameter("@OrderNo", CurrentOrder.OrderNo));

                // 헤더 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM [Order] WHERE OrderNo = @OrderNo",
                    new SqlParameter("@OrderNo", CurrentOrder.OrderNo));

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
            if (DetailList == null || CurrentOrder == null) return;

            decimal totalWeight = 0;
            decimal totalAmount = 0;

            foreach (DataRow row in DetailList.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                totalWeight += row["TotalWeight"] != DBNull.Value ? Convert.ToDecimal(row["TotalWeight"]) : 0;
                totalAmount += row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
            }

            CurrentOrder.TotalWeight = totalWeight;
            CurrentOrder.TotalAmount = totalAmount;
        }

        private string GenerateOrderNo()
        {
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
