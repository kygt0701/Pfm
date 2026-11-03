using System;
using System.Data;
using System.Data.SqlClient;
using Pfm.Common;
using Pfm.Services;
using Pfm.ViewModels.Base;

namespace Pfm.ViewModels
{
    /// <summary>
    /// 재고 관리 ViewModel
    /// </summary>
    public class InventoryViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _inventoryList;
        private string _searchKeyword;
        private string _selectedWarehouse;
        #endregion

        #region 속성
        public DataTable InventoryList
        {
            get => _inventoryList;
            set => SetProperty(ref _inventoryList, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        public string SelectedWarehouse
        {
            get => _selectedWarehouse;
            set => SetProperty(ref _selectedWarehouse, value);
        }
        #endregion

        public InventoryViewModel()
        {
            MenuId = "INV_STOCK";
            FormTitle = "재고 관리";
        }

        protected override void OnNew()
        {
            // 재고는 직접 신규 생성하지 않음 (입출고로 관리)
            ServiceLocator.MessageService.ShowInfo("재고는 입출고 처리를 통해 관리됩니다.");
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = @"SELECT i.ItemCode, it.ItemName, i.WarehouseCode, w.WarehouseName,
                                      i.LotNo, i.Quantity, it.Unit, i.LastUpdated
                               FROM Inventory i
                               INNER JOIN Item it ON i.ItemCode = it.ItemCode
                               LEFT JOIN Warehouse w ON i.WarehouseCode = w.WarehouseCode
                               WHERE (@Keyword IS NULL OR it.ItemName LIKE '%' + @Keyword + '%' 
                                      OR i.ItemCode LIKE '%' + @Keyword + '%')
                                 AND (@Warehouse IS NULL OR i.WarehouseCode = @Warehouse)
                               ORDER BY it.ItemName, i.WarehouseCode";

                InventoryList = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Keyword", string.IsNullOrEmpty(SearchKeyword) ? (object)DBNull.Value : SearchKeyword),
                    new SqlParameter("@Warehouse", string.IsNullOrEmpty(SelectedWarehouse) ? (object)DBNull.Value : SelectedWarehouse));

                EndWork($"조회 완료: {InventoryList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            // 재고 직접 수정은 제한
            ServiceLocator.MessageService.ShowInfo("재고는 입출고 처리를 통해 관리됩니다.");
        }

        protected override void OnDelete()
        {
            // 재고 직접 삭제는 제한
            ServiceLocator.MessageService.ShowInfo("재고는 입출고 처리를 통해 관리됩니다.");
        }
    }
}
