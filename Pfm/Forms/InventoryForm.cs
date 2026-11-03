using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.Utils;
using Pfm.Common;

namespace Pfm.Forms
{
    /// <summary>
    /// 재고 관리 (거래처별)
    /// </summary>
    public class InventoryForm : BaseForm
    {
        #region 컨트롤
        private PanelControl pnlSearch;
        private SearchLookUpEdit slueCustomer;
        private ComboBoxEdit cboItemType;
        private TextEdit txtItemName;
        private SimpleButton btnSearchInner;
        private GridControl gridInventory;
        private GridView gridViewInventory;
        private RepositoryItemSearchLookUpEdit repoCustomer;
        #endregion

        private DataTable dtInventory;
        private DataTable dtCustomer;

        public InventoryForm()
        {
            MenuId = "WRK_INV";
            FormTitle = "재고 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 600);

            // 검색 패널
            pnlSearch = new PanelControl();
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Height = 50;
            pnlSearch.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // 거래처 검색
            var lblCustomer = new LabelControl();
            lblCustomer.Text = "거래처:";
            lblCustomer.Location = new Point(15, 15);
            pnlSearch.Controls.Add(lblCustomer);

            slueCustomer = new SearchLookUpEdit();
            slueCustomer.Location = new Point(65, 12);
            slueCustomer.Size = new Size(200, 22);
            LoadCustomers();
            pnlSearch.Controls.Add(slueCustomer);

            var lblItemType = new LabelControl();
            lblItemType.Text = "품목유형:";
            lblItemType.Location = new Point(285, 15);
            pnlSearch.Controls.Add(lblItemType);

            cboItemType = new ComboBoxEdit();
            cboItemType.Location = new Point(350, 12);
            cboItemType.Size = new Size(120, 22);
            cboItemType.Properties.Items.Add("전체");
            LoadItemTypes();
            cboItemType.SelectedIndex = 0;
            pnlSearch.Controls.Add(cboItemType);

            var lblItemName = new LabelControl();
            lblItemName.Text = "품목명:";
            lblItemName.Location = new Point(490, 15);
            pnlSearch.Controls.Add(lblItemName);

            txtItemName = new TextEdit();
            txtItemName.Location = new Point(540, 12);
            txtItemName.Size = new Size(150, 22);
            pnlSearch.Controls.Add(txtItemName);

            btnSearchInner = new SimpleButton();
            btnSearchInner.Text = "검색";
            btnSearchInner.Location = new Point(710, 10);
            btnSearchInner.Size = new Size(80, 26);
            btnSearchInner.Click += (s, e) => LoadData();
            ThemeManager.ApplySecondaryButton(btnSearchInner);
            pnlSearch.Controls.Add(btnSearchInner);

            // 그리드
            gridInventory = new GridControl();
            gridInventory.Dock = DockStyle.Fill;

            gridViewInventory = new GridView(gridInventory);
            gridInventory.MainView = gridViewInventory;

            SetupGrid();

            pnlContent.Controls.Add(gridInventory);
            pnlContent.Controls.Add(pnlSearch);
        }

        private void LoadCustomers()
        {
            try
            {
                string sql = "SELECT CustomerCode, CustomerName FROM Customer WHERE IsActive = 1 ORDER BY CustomerName";
                dtCustomer = DbHelper.ExecuteQuery(sql);

                slueCustomer.Properties.DataSource = dtCustomer;
                slueCustomer.Properties.ValueMember = "CustomerCode";
                slueCustomer.Properties.DisplayMember = "CustomerName";
                slueCustomer.Properties.NullText = "(전체)";

                var view = slueCustomer.Properties.View;
                view.Columns.Clear();
                view.Columns.AddField("CustomerCode").Caption = "거래처코드";
                view.Columns.AddField("CustomerName").Caption = "거래처명";
                view.Columns["CustomerCode"].Width = 100;
                view.Columns["CustomerName"].Width = 150;
            }
            catch { }
        }

        private void LoadItemTypes()
        {
            try
            {
                string sql = "SELECT CodeName FROM CommonCode WHERE GroupCode = 'ITEM_TYPE' AND IsActive = 1 ORDER BY SortOrder";
                var dt = DbHelper.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    cboItemType.Properties.Items.Add(row["CodeName"].ToString());
                }
            }
            catch { }
        }

        private void SetupGrid()
        {
            gridViewInventory.Columns.Clear();

            // 거래처 콤보 설정
            repoCustomer = new RepositoryItemSearchLookUpEdit();
            repoCustomer.DataSource = dtCustomer;
            repoCustomer.ValueMember = "CustomerCode";
            repoCustomer.DisplayMember = "CustomerName";
            repoCustomer.NullText = "";
            var repoView = repoCustomer.View;
            repoView.Columns.AddField("CustomerCode").Caption = "거래처코드";
            repoView.Columns.AddField("CustomerName").Caption = "거래처명";
            gridInventory.RepositoryItems.Add(repoCustomer);

            AddColumn("CustomerCode", "거래처코드", 90);
            AddColumn("CustomerName", "거래처명", 120);
            AddColumn("ItemCode", "품목코드", 80);
            AddColumn("ItemName", "품목명", 120);
            AddColumn("ItemTypeName", "품목유형", 80);
            AddColumn("Shape", "형태", 50);
            AddColumn("Grade", "등급", 50);
            AddColumn("Diameter", "직경", 60);
            AddColumn("Thickness", "두께", 60);
            AddColumn("Width", "너비", 60);
            AddColumn("Length", "길이", 60);
            AddColumn("Quantity", "재고수량", 80, true);
            AddColumn("TotalWeight", "총중량(kg)", 90, true);
            AddColumn("Location", "위치", 80, true);
            AddColumn("LastUpdated", "최종수정", 130);

            // 그리드 옵션
            gridViewInventory.OptionsBehavior.Editable = true;
            gridViewInventory.OptionsView.ShowGroupPanel = false;
            gridViewInventory.OptionsView.ShowFooter = true;
            gridViewInventory.OptionsView.ColumnAutoWidth = false;
            gridViewInventory.RowHeight = 25;

            // 거래처코드, 거래처명 고정
            gridViewInventory.Columns["CustomerCode"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridViewInventory.Columns["CustomerName"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            // 숫자 포맷
            gridViewInventory.Columns["Quantity"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewInventory.Columns["Quantity"].DisplayFormat.FormatString = "N0";
            gridViewInventory.Columns["TotalWeight"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewInventory.Columns["TotalWeight"].DisplayFormat.FormatString = "N2";

            // 합계
            gridViewInventory.Columns["Quantity"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewInventory.Columns["Quantity"].SummaryItem.DisplayFormat = "합계: {0:N0}";
            gridViewInventory.Columns["TotalWeight"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewInventory.Columns["TotalWeight"].SummaryItem.DisplayFormat = "합계: {0:N2}";

            ThemeManager.ApplyGridViewTheme(gridViewInventory);
            SetupGridExcelExport(gridInventory, gridViewInventory, "재고현황");
        }

        private void AddColumn(string fieldName, string caption, int width, bool editable = false)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewInventory.Columns.Count;
            if (!editable)
            {
                col.OptionsColumn.AllowEdit = false;
                col.OptionsColumn.ReadOnly = true;
            }
            gridViewInventory.Columns.Add(col);
        }

        protected override void OnSearchClick()
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string customerCode = slueCustomer.EditValue?.ToString();
                string itemType = cboItemType.SelectedIndex > 0 ? cboItemType.Text : null;
                string itemName = txtItemName.Text.Trim();

                string sql = @"
                    SELECT inv.CustomerCode, c.CustomerName,
                           inv.ItemCode, i.ItemName, 
                           (SELECT CodeName FROM CommonCode WHERE GroupCode = 'ITEM_TYPE' AND Code = i.ItemType) AS ItemTypeName,
                           i.Shape, i.Grade, i.Diameter, i.Thickness, i.Width, i.Length,
                           ISNULL(inv.Quantity, 0) AS Quantity,
                           ISNULL(inv.TotalWeight, 0) AS TotalWeight,
                           inv.Location, inv.LastUpdated
                    FROM Inventory inv
                    INNER JOIN Item i ON inv.ItemCode = i.ItemCode
                    LEFT JOIN Customer c ON inv.CustomerCode = c.CustomerCode
                    WHERE i.IsActive = 1";

                if (!string.IsNullOrEmpty(customerCode))
                {
                    sql += " AND inv.CustomerCode = @CustomerCode";
                }
                if (!string.IsNullOrEmpty(itemType))
                {
                    sql += " AND (SELECT CodeName FROM CommonCode WHERE GroupCode = 'ITEM_TYPE' AND Code = i.ItemType) = @ItemType";
                }
                if (!string.IsNullOrEmpty(itemName))
                {
                    sql += " AND i.ItemName LIKE '%' + @ItemName + '%'";
                }
                sql += " ORDER BY c.CustomerName, i.ItemCode";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CustomerCode", (object)customerCode ?? DBNull.Value),
                    new SqlParameter("@ItemType", (object)itemType ?? DBNull.Value),
                    new SqlParameter("@ItemName", (object)itemName ?? DBNull.Value)
                };

                dtInventory = DbHelper.ExecuteQuery(sql, parameters);
                gridInventory.DataSource = dtInventory;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnNewClick()
        {
            // 재고 조정 (선택된 품목)
            if (gridViewInventory.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("품목을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string customerCode = gridViewInventory.GetRowCellValue(gridViewInventory.FocusedRowHandle, "CustomerCode")?.ToString();
            string customerName = gridViewInventory.GetRowCellValue(gridViewInventory.FocusedRowHandle, "CustomerName")?.ToString();
            string itemCode = gridViewInventory.GetRowCellValue(gridViewInventory.FocusedRowHandle, "ItemCode")?.ToString();
            string itemName = gridViewInventory.GetRowCellValue(gridViewInventory.FocusedRowHandle, "ItemName")?.ToString();

            var result = XtraInputBox.Show($"[{customerName}] [{itemCode}] {itemName}\n조정 수량을 입력하세요 (+ 입고, - 출고):", "재고 조정", "");
            if (!string.IsNullOrEmpty(result))
            {
                if (decimal.TryParse(result, out decimal adjustQty))
                {
                    AdjustInventory(customerCode, itemCode, adjustQty);
                }
            }
        }

        private void AdjustInventory(string customerCode, string itemCode, decimal adjustQty)
        {
            try
            {
                // 기존 재고 확인
                string sqlCheck = "SELECT COUNT(*) FROM Inventory WHERE CustomerCode = @CustomerCode AND ItemCode = @ItemCode";
                int count = Convert.ToInt32(DbHelper.ExecuteScalar(sqlCheck, 
                    new SqlParameter("@CustomerCode", customerCode),
                    new SqlParameter("@ItemCode", itemCode)));

                if (count == 0)
                {
                    // INSERT
                    string sqlInsert = @"
                        INSERT INTO Inventory (CustomerCode, ItemCode, Quantity, TotalWeight, LastUpdated, CreatedBy, CreatedAt)
                        VALUES (@CustomerCode, @ItemCode, @Quantity, 0, GETDATE(), @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sqlInsert,
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@ItemCode", itemCode),
                        new SqlParameter("@Quantity", adjustQty),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
                else
                {
                    // UPDATE
                    string sqlUpdate = @"
                        UPDATE Inventory SET Quantity = Quantity + @AdjustQty, 
                            LastUpdated = GETDATE(), ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE CustomerCode = @CustomerCode AND ItemCode = @ItemCode";

                    DbHelper.ExecuteNonQuery(sqlUpdate,
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@ItemCode", itemCode),
                        new SqlParameter("@AdjustQty", adjustQty),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
                }

                XtraMessageBox.Show($"재고가 조정되었습니다. ({(adjustQty >= 0 ? "+" : "")}{adjustQty})", 
                    "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"재고 조정 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnSaveClick()
        {
            try
            {
                gridViewInventory.CloseEditor();
                gridViewInventory.UpdateCurrentRow();

                int count = 0;
                foreach (DataRow row in dtInventory.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        string customerCode = row["CustomerCode"]?.ToString();
                        string itemCode = row["ItemCode"].ToString();
                        decimal quantity = Convert.ToDecimal(row["Quantity"]);
                        decimal totalWeight = Convert.ToDecimal(row["TotalWeight"]);
                        string location = row["Location"]?.ToString();

                        // 기존 재고 확인
                        string sqlCheck = "SELECT COUNT(*) FROM Inventory WHERE CustomerCode = @CustomerCode AND ItemCode = @ItemCode";
                        int exists = Convert.ToInt32(DbHelper.ExecuteScalar(sqlCheck, 
                            new SqlParameter("@CustomerCode", customerCode),
                            new SqlParameter("@ItemCode", itemCode)));

                        if (exists == 0)
                        {
                            string sqlInsert = @"
                                INSERT INTO Inventory (CustomerCode, ItemCode, Quantity, TotalWeight, Location, LastUpdated, CreatedBy, CreatedAt)
                                VALUES (@CustomerCode, @ItemCode, @Quantity, @TotalWeight, @Location, GETDATE(), @CreatedBy, GETDATE())";

                            DbHelper.ExecuteNonQuery(sqlInsert,
                                new SqlParameter("@CustomerCode", customerCode),
                                new SqlParameter("@ItemCode", itemCode),
                                new SqlParameter("@Quantity", quantity),
                                new SqlParameter("@TotalWeight", totalWeight),
                                new SqlParameter("@Location", (object)location ?? DBNull.Value),
                                new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                        }
                        else
                        {
                            string sqlUpdate = @"
                                UPDATE Inventory SET Quantity = @Quantity, TotalWeight = @TotalWeight, 
                                    Location = @Location, LastUpdated = GETDATE(), 
                                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                WHERE CustomerCode = @CustomerCode AND ItemCode = @ItemCode";

                            DbHelper.ExecuteNonQuery(sqlUpdate,
                                new SqlParameter("@CustomerCode", customerCode),
                                new SqlParameter("@ItemCode", itemCode),
                                new SqlParameter("@Quantity", quantity),
                                new SqlParameter("@TotalWeight", totalWeight),
                                new SqlParameter("@Location", (object)location ?? DBNull.Value),
                                new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
                        }
                        count++;
                    }
                }

                if (count > 0)
                {
                    XtraMessageBox.Show($"{count}건 저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"저장 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnDeleteClick()
        {
            // 재고 삭제는 지원하지 않음
            XtraMessageBox.Show("재고 데이터는 삭제할 수 없습니다.\n재고 조정(신규 버튼)을 사용하세요.", 
                "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
