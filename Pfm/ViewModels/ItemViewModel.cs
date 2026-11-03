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
    /// 품목 관리 ViewModel
    /// </summary>
    public class ItemViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _itemList;
        private ItemModel _selectedItem;
        private string _searchKeyword;
        #endregion

        #region 속성
        public DataTable ItemList
        {
            get => _itemList;
            set => SetProperty(ref _itemList, value);
        }

        public ItemModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }
        #endregion

        public ItemViewModel()
        {
            MenuId = "MST_ITEM";
            FormTitle = "품목 관리";
        }

        protected override void OnNew()
        {
            SelectedItem = new ItemModel
            {
                ItemCode = GenerateItemCode(),
                IsActive = true
            };
            StatusMessage = "신규 품목 입력";
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = @"SELECT ItemCode, ItemName, ItemType, Category, Unit, 
                                      UnitPrice, Spec, Remark, IsActive
                               FROM Item 
                               WHERE (@Keyword IS NULL OR ItemName LIKE '%' + @Keyword + '%' 
                                      OR ItemCode LIKE '%' + @Keyword + '%')
                               ORDER BY ItemName";

                ItemList = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Keyword", string.IsNullOrEmpty(SearchKeyword) ? (object)DBNull.Value : SearchKeyword));

                EndWork($"조회 완료: {ItemList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            if (SelectedItem == null)
            {
                ServiceLocator.MessageService.ShowWarning("저장할 품목을 선택하세요.");
                return;
            }

            if (string.IsNullOrEmpty(SelectedItem.ItemName))
            {
                ServiceLocator.MessageService.ShowWarning("품목명을 입력하세요.");
                return;
            }

            try
            {
                BeginWork("저장 중...");

                string checkSql = "SELECT COUNT(*) FROM Item WHERE ItemCode = @ItemCode";
                var dt = DbHelper.ExecuteQuery(checkSql, new SqlParameter("@ItemCode", SelectedItem.ItemCode));
                bool exists = Convert.ToInt32(dt.Rows[0][0]) > 0;

                if (exists)
                {
                    string sql = @"UPDATE Item SET 
                                    ItemName = @ItemName, ItemType = @ItemType, Category = @Category,
                                    Unit = @Unit, UnitPrice = @UnitPrice, Spec = @Spec,
                                    Remark = @Remark, IsActive = @IsActive,
                                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE ItemCode = @ItemCode";

                    DbHelper.ExecuteNonQuery(sql, GetSaveParameters(false));
                }
                else
                {
                    string sql = @"INSERT INTO Item 
                                    (ItemCode, ItemName, ItemType, Category, Unit, UnitPrice, 
                                     Spec, Remark, IsActive, CreatedBy, CreatedAt)
                                   VALUES 
                                    (@ItemCode, @ItemName, @ItemType, @Category, @Unit, @UnitPrice,
                                     @Spec, @Remark, @IsActive, @CreatedBy, GETDATE())";

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
            if (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.ItemCode))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 품목을 선택하세요.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"'{SelectedItem.ItemName}' 품목을 삭제하시겠습니까?"))
                return;

            try
            {
                BeginWork("삭제 중...");

                string sql = "DELETE FROM Item WHERE ItemCode = @ItemCode";
                DbHelper.ExecuteNonQuery(sql, new SqlParameter("@ItemCode", SelectedItem.ItemCode));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                SelectedItem = null;
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
            var i = SelectedItem;
            return new[]
            {
                new SqlParameter("@ItemCode", i.ItemCode ?? (object)DBNull.Value),
                new SqlParameter("@ItemName", i.ItemName ?? (object)DBNull.Value),
                new SqlParameter("@ItemType", i.ItemType ?? (object)DBNull.Value),
                new SqlParameter("@Category", i.Category ?? (object)DBNull.Value),
                new SqlParameter("@Unit", i.Unit ?? (object)DBNull.Value),
                new SqlParameter("@UnitPrice", i.UnitPrice),
                new SqlParameter("@Spec", i.Spec ?? (object)DBNull.Value),
                new SqlParameter("@Remark", i.Remark ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", i.IsActive),
                new SqlParameter(isInsert ? "@CreatedBy" : "@ModifiedBy", UserSession.UserId ?? "SYSTEM")
            };
        }

        private string GenerateItemCode()
        {
            return $"I{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
