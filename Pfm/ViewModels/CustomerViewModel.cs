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
    /// 거래처 관리 ViewModel
    /// </summary>
    public class CustomerViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _customerList;
        private CustomerModel _selectedCustomer;
        private string _searchKeyword;
        #endregion

        #region 속성
        public DataTable CustomerList
        {
            get => _customerList;
            set => SetProperty(ref _customerList, value);
        }

        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }
        #endregion

        public CustomerViewModel()
        {
            MenuId = "MST_CUST";
            FormTitle = "거래처 관리";
        }

        protected override void OnNew()
        {
            SelectedCustomer = new CustomerModel
            {
                CustomerCode = GenerateCustomerCode(),
                IsActive = true
            };
            StatusMessage = "신규 거래처 입력";
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = @"SELECT CustomerCode, CustomerName, CustomerType, BusinessNo, 
                                      CeoName, Tel, Fax, Email, Address, Remark, IsActive
                               FROM Customer 
                               WHERE (@Keyword IS NULL OR CustomerName LIKE '%' + @Keyword + '%' 
                                      OR CustomerCode LIKE '%' + @Keyword + '%')
                               ORDER BY CustomerName";

                CustomerList = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Keyword", string.IsNullOrEmpty(SearchKeyword) ? (object)DBNull.Value : SearchKeyword));

                EndWork($"조회 완료: {CustomerList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            if (SelectedCustomer == null)
            {
                ServiceLocator.MessageService.ShowWarning("저장할 거래처를 선택하세요.");
                return;
            }

            if (string.IsNullOrEmpty(SelectedCustomer.CustomerName))
            {
                ServiceLocator.MessageService.ShowWarning("거래처명을 입력하세요.");
                return;
            }

            try
            {
                BeginWork("저장 중...");

                // 존재 여부 확인
                string checkSql = "SELECT COUNT(*) FROM Customer WHERE CustomerCode = @CustomerCode";
                var dt = DbHelper.ExecuteQuery(checkSql, new SqlParameter("@CustomerCode", SelectedCustomer.CustomerCode));
                bool exists = Convert.ToInt32(dt.Rows[0][0]) > 0;

                if (exists)
                {
                    // UPDATE
                    string sql = @"UPDATE Customer SET 
                                    CustomerName = @CustomerName, CustomerType = @CustomerType,
                                    BusinessNo = @BusinessNo, CeoName = @CeoName, Tel = @Tel,
                                    Fax = @Fax, Email = @Email, Address = @Address,
                                    Remark = @Remark, IsActive = @IsActive,
                                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE CustomerCode = @CustomerCode";

                    DbHelper.ExecuteNonQuery(sql, GetSaveParameters(false));
                }
                else
                {
                    // INSERT
                    string sql = @"INSERT INTO Customer 
                                    (CustomerCode, CustomerName, CustomerType, BusinessNo, CeoName,
                                     Tel, Fax, Email, Address, Remark, IsActive, CreatedBy, CreatedAt)
                                   VALUES 
                                    (@CustomerCode, @CustomerName, @CustomerType, @BusinessNo, @CeoName,
                                     @Tel, @Fax, @Email, @Address, @Remark, @IsActive, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql, GetSaveParameters(true));
                }

                EndWork("저장 완료");
                ServiceLocator.MessageService.ShowInfo("저장되었습니다.");
                OnSearch();
            }
            catch (Exception ex)
            {
                EndWork("저장 실패");
                ServiceLocator.MessageService.ShowError($"저장 오류: {ex.Message}");
            }
        }

        protected override void OnDelete()
        {
            if (SelectedCustomer == null || string.IsNullOrEmpty(SelectedCustomer.CustomerCode))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 거래처를 선택하세요.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"'{SelectedCustomer.CustomerName}' 거래처를 삭제하시겠습니까?"))
                return;

            try
            {
                BeginWork("삭제 중...");

                string sql = "DELETE FROM Customer WHERE CustomerCode = @CustomerCode";
                DbHelper.ExecuteNonQuery(sql, new SqlParameter("@CustomerCode", SelectedCustomer.CustomerCode));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                SelectedCustomer = null;
                OnSearch();
            }
            catch (Exception ex)
            {
                EndWork("삭제 실패");
                ServiceLocator.MessageService.ShowError($"삭제 오류: {ex.Message}");
            }
        }

        private SqlParameter[] GetSaveParameters(bool isInsert)
        {
            var c = SelectedCustomer;
            return new[]
            {
                new SqlParameter("@CustomerCode", c.CustomerCode ?? (object)DBNull.Value),
                new SqlParameter("@CustomerName", c.CustomerName ?? (object)DBNull.Value),
                new SqlParameter("@CustomerType", c.CustomerType ?? (object)DBNull.Value),
                new SqlParameter("@BusinessNo", c.BusinessNo ?? (object)DBNull.Value),
                new SqlParameter("@CeoName", c.CeoName ?? (object)DBNull.Value),
                new SqlParameter("@Tel", c.Tel ?? (object)DBNull.Value),
                new SqlParameter("@Fax", c.Fax ?? (object)DBNull.Value),
                new SqlParameter("@Email", c.Email ?? (object)DBNull.Value),
                new SqlParameter("@Address", c.Address ?? (object)DBNull.Value),
                new SqlParameter("@Remark", c.Remark ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", c.IsActive),
                new SqlParameter(isInsert ? "@CreatedBy" : "@ModifiedBy", UserSession.UserId ?? "SYSTEM")
            };
        }

        private string GenerateCustomerCode()
        {
            return $"C{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
