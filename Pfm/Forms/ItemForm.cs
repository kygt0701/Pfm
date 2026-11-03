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
    /// 품목 관리 (비닐/필름 규격)
    /// </summary>
    public class ItemForm : BaseForm
    {
        #region 컨트롤
        private GridControl gridItem;
        private GridView gridViewItem;
        private RepositoryItemComboBox cboItemType;
        private RepositoryItemComboBox cboShape;
        private RepositoryItemComboBox cboGrade;
        #endregion

        private DataTable dtItem;

        public ItemForm()
        {
            MenuId = "MST_ITEM";
            FormTitle = "품목 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 600);

            // 그리드
            gridItem = new GridControl();
            gridItem.Dock = DockStyle.Fill;

            gridViewItem = new GridView(gridItem);
            gridItem.MainView = gridViewItem;

            SetupGrid();
            SetupRepositoryItems();

            pnlContent.Controls.Add(gridItem);
        }

        private void SetupRepositoryItems()
        {
            // 품목유형 콤보
            cboItemType = new RepositoryItemComboBox();
            cboItemType.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            LoadComboData(cboItemType, "ITEM_TYPE");

            // 형태 콤보
            cboShape = new RepositoryItemComboBox();
            cboShape.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            LoadComboData(cboShape, "ITEM_SHAPE");

            // 등급 콤보
            cboGrade = new RepositoryItemComboBox();
            cboGrade.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            LoadComboData(cboGrade, "ITEM_GRADE");

            gridItem.RepositoryItems.Add(cboItemType);
            gridItem.RepositoryItems.Add(cboShape);
            gridItem.RepositoryItems.Add(cboGrade);
        }

        private void LoadComboData(RepositoryItemComboBox combo, string groupCode)
        {
            try
            {
                string sql = "SELECT Code, CodeName FROM CommonCode WHERE GroupCode = @GroupCode AND IsActive = 1 ORDER BY SortOrder";
                var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@GroupCode", groupCode));
                foreach (DataRow row in dt.Rows)
                {
                    combo.Items.Add(row["CodeName"].ToString());
                }
            }
            catch { }
        }

        private void SetupGrid()
        {
            gridViewItem.Columns.Clear();

            AddColumn("ItemCode", "품목코드", 80);
            AddColumn("ItemName", "품목명", 120);
            AddColumn("ItemType", "품목유형", 80, cboItemType);
            AddColumn("Shape", "형태", 50, cboShape);
            AddColumn("Grade", "등급", 50, cboGrade);
            AddColumn("Diameter", "직경", 60);
            AddColumn("Thickness", "두께", 60);
            AddColumn("Width", "너비", 60);
            AddColumn("Length", "길이", 60);
            AddColumn("SpecificGravity", "비중", 60);
            AddColumn("UnitWeight", "개당중량", 70);
            AddColumn("UnitPrice", "단가", 80);
            AddColumn("IsActive", "사용", 50);
            AddColumn("Remark", "비고", 150);

            // 그리드 옵션
            gridViewItem.OptionsBehavior.Editable = true;
            gridViewItem.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            gridViewItem.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewItem.OptionsView.ShowAutoFilterRow = true;
            gridViewItem.OptionsView.ShowGroupPanel = false;
            gridViewItem.OptionsView.ShowFooter = true;
            gridViewItem.OptionsFind.AlwaysVisible = true;
            gridViewItem.RowHeight = 25;

            // 숫자 포맷
            gridViewItem.Columns["Diameter"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewItem.Columns["Diameter"].DisplayFormat.FormatString = "N2";
            gridViewItem.Columns["Thickness"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewItem.Columns["Thickness"].DisplayFormat.FormatString = "N4";
            gridViewItem.Columns["UnitPrice"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewItem.Columns["UnitPrice"].DisplayFormat.FormatString = "N0";

            ThemeManager.ApplyGridViewTheme(gridViewItem);
            SetupGridExcelExport(gridItem, gridViewItem, "품목목록");
        }

        private void AddColumn(string fieldName, string caption, int width, RepositoryItemComboBox combo = null)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewItem.Columns.Count;
            if (combo != null)
                col.ColumnEdit = combo;
            gridViewItem.Columns.Add(col);
        }

        protected override void OnSearchClick()
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string sql = @"
                    SELECT ItemCode, ItemName, 
                           (SELECT CodeName FROM CommonCode WHERE GroupCode = 'ITEM_TYPE' AND Code = i.ItemType) AS ItemType,
                           Shape, Grade, Diameter, Thickness, Width, Length,
                           SpecificGravity, UnitWeight, UnitPrice, IsActive, Remark
                    FROM Item i
                    ORDER BY ItemCode";

                dtItem = DbHelper.ExecuteQuery(sql);
                gridItem.DataSource = dtItem;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnNewClick()
        {
            gridViewItem.AddNewRow();
            gridViewItem.SetRowCellValue(GridControl.NewItemRowHandle, "IsActive", true);
            gridViewItem.SetRowCellValue(GridControl.NewItemRowHandle, "SpecificGravity", 0.0092m);
            gridViewItem.FocusedColumn = gridViewItem.Columns["ItemCode"];
        }

        protected override void OnSaveClick()
        {
            try
            {
                gridViewItem.CloseEditor();
                gridViewItem.UpdateCurrentRow();

                int count = 0;
                foreach (DataRow row in dtItem.Rows)
                {
                    if (row.RowState == DataRowState.Added)
                    {
                        // 품목유형 코드 변환
                        string itemTypeCode = GetCodeFromName("ITEM_TYPE", row["ItemType"]?.ToString());

                        string sql = @"
                            INSERT INTO Item (ItemCode, ItemName, ItemType, Shape, Grade, Diameter, Thickness, 
                                Width, Length, SpecificGravity, UnitWeight, UnitPrice, IsActive, Remark, CreatedBy, CreatedAt)
                            VALUES (@ItemCode, @ItemName, @ItemType, @Shape, @Grade, @Diameter, @Thickness,
                                @Width, @Length, @SpecificGravity, @UnitWeight, @UnitPrice, @IsActive, @Remark, @CreatedBy, GETDATE())";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ItemCode", row["ItemCode"] ?? DBNull.Value),
                            new SqlParameter("@ItemName", row["ItemName"] ?? DBNull.Value),
                            new SqlParameter("@ItemType", (object)itemTypeCode ?? DBNull.Value),
                            new SqlParameter("@Shape", row["Shape"] ?? DBNull.Value),
                            new SqlParameter("@Grade", row["Grade"] ?? DBNull.Value),
                            new SqlParameter("@Diameter", row["Diameter"] ?? DBNull.Value),
                            new SqlParameter("@Thickness", row["Thickness"] ?? DBNull.Value),
                            new SqlParameter("@Width", row["Width"] ?? DBNull.Value),
                            new SqlParameter("@Length", row["Length"] ?? DBNull.Value),
                            new SqlParameter("@SpecificGravity", row["SpecificGravity"] ?? 0.0092m),
                            new SqlParameter("@UnitWeight", row["UnitWeight"] ?? DBNull.Value),
                            new SqlParameter("@UnitPrice", row["UnitPrice"] ?? DBNull.Value),
                            new SqlParameter("@IsActive", row["IsActive"] ?? true),
                            new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                            new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM")
                        };

                        DbHelper.ExecuteNonQuery(sql, parameters);
                        count++;
                    }
                    else if (row.RowState == DataRowState.Modified)
                    {
                        string itemTypeCode = GetCodeFromName("ITEM_TYPE", row["ItemType"]?.ToString());

                        string sql = @"
                            UPDATE Item SET ItemName = @ItemName, ItemType = @ItemType, Shape = @Shape, Grade = @Grade,
                                Diameter = @Diameter, Thickness = @Thickness, Width = @Width, Length = @Length,
                                SpecificGravity = @SpecificGravity, UnitWeight = @UnitWeight, UnitPrice = @UnitPrice,
                                IsActive = @IsActive, Remark = @Remark, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                            WHERE ItemCode = @ItemCode";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@ItemCode", row["ItemCode"]),
                            new SqlParameter("@ItemName", row["ItemName"] ?? DBNull.Value),
                            new SqlParameter("@ItemType", (object)itemTypeCode ?? DBNull.Value),
                            new SqlParameter("@Shape", row["Shape"] ?? DBNull.Value),
                            new SqlParameter("@Grade", row["Grade"] ?? DBNull.Value),
                            new SqlParameter("@Diameter", row["Diameter"] ?? DBNull.Value),
                            new SqlParameter("@Thickness", row["Thickness"] ?? DBNull.Value),
                            new SqlParameter("@Width", row["Width"] ?? DBNull.Value),
                            new SqlParameter("@Length", row["Length"] ?? DBNull.Value),
                            new SqlParameter("@SpecificGravity", row["SpecificGravity"] ?? 0.0092m),
                            new SqlParameter("@UnitWeight", row["UnitWeight"] ?? DBNull.Value),
                            new SqlParameter("@UnitPrice", row["UnitPrice"] ?? DBNull.Value),
                            new SqlParameter("@IsActive", row["IsActive"] ?? true),
                            new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                            new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM")
                        };

                        DbHelper.ExecuteNonQuery(sql, parameters);
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

        private string GetCodeFromName(string groupCode, string codeName)
        {
            if (string.IsNullOrEmpty(codeName)) return null;
            try
            {
                string sql = "SELECT Code FROM CommonCode WHERE GroupCode = @GroupCode AND CodeName = @CodeName";
                var result = DbHelper.ExecuteScalar(sql, 
                    new SqlParameter("@GroupCode", groupCode),
                    new SqlParameter("@CodeName", codeName));
                return result?.ToString();
            }
            catch { return null; }
        }

        protected override void OnDeleteClick()
        {
            if (gridViewItem.FocusedRowHandle < 0) return;

            var itemCode = gridViewItem.GetRowCellValue(gridViewItem.FocusedRowHandle, "ItemCode")?.ToString();
            if (string.IsNullOrEmpty(itemCode)) return;

            if (XtraMessageBox.Show($"품목 '{itemCode}'를 삭제하시겠습니까?", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DbHelper.ExecuteNonQuery("DELETE FROM Item WHERE ItemCode = @ItemCode",
                        new SqlParameter("@ItemCode", itemCode));
                    LoadData();
                    XtraMessageBox.Show("삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
