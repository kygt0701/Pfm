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
    /// 출하 관리
    /// </summary>
    public class ShipmentForm : BaseForm
    {
        #region 컨트롤
        private SplitContainerControl splitContainer;

        // 헤더 영역
        private PanelControl pnlHeader;
        private TextEdit txtShipmentNo;
        private DateEdit deShipmentDate;
        private SearchLookUpEdit slueCustomer;
        private ComboBoxEdit cboStatus;
        private TextEdit txtRemark;
        private LabelControl lblTotalQty;
        private LabelControl lblTotalWeight;
        private LabelControl lblTotalAmount;

        // 상세 그리드
        private GridControl gridDetail;
        private GridView gridViewDetail;
        private RepositoryItemSearchLookUpEdit repoItem;

        // 목록 그리드
        private GridControl gridList;
        private GridView gridViewList;
        #endregion

        private DataTable dtList;
        private DataTable dtDetail;
        private DataTable dtCustomer;
        private DataTable dtItem;
        private bool isNewMode = false;

        public ShipmentForm()
        {
            MenuId = "WRK_SHP";
            FormTitle = "출하 관리";
            InitializeComponents();
            LoadMasterData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1300, 700);

            // 스플릿 컨테이너 (좌: 목록, 우: 상세)
            splitContainer = new SplitContainerControl();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterPosition = 350;
            splitContainer.FixedPanel = SplitFixedPanel.Panel1;

            // 좌측: 출하 목록
            SetupListGrid();
            splitContainer.Panel1.Controls.Add(gridList);

            // 우측: 출하 상세
            var pnlRight = new PanelControl();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            SetupHeaderPanel();
            SetupDetailGrid();

            pnlRight.Controls.Add(gridDetail);
            pnlRight.Controls.Add(pnlHeader);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlContent.Controls.Add(splitContainer);
        }

        private void SetupListGrid()
        {
            gridList = new GridControl();
            gridList.Dock = DockStyle.Fill;
            gridViewList = new GridView(gridList);
            gridList.MainView = gridViewList;

            gridViewList.Columns.Clear();
            AddListColumn("ShipmentNo", "출하번호", 100);
            AddListColumn("ShipmentDate", "출하일자", 90);
            AddListColumn("CustomerName", "거래처", 120);
            AddListColumn("TotalAmount", "금액", 100);
            AddListColumn("Status", "상태", 70);

            gridViewList.OptionsBehavior.Editable = false;
            gridViewList.OptionsView.ShowGroupPanel = false;
            gridViewList.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridViewList.FocusedRowChanged += GridViewList_FocusedRowChanged;

            gridViewList.Columns["TotalAmount"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewList.Columns["TotalAmount"].DisplayFormat.FormatString = "#,##0";

            ThemeManager.ApplyGridViewTheme(gridViewList);
            SetupGridExcelExport(gridList, gridViewList, "출하목록");
        }

        private void AddListColumn(string fieldName, string caption, int width)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewList.Columns.Count;
            gridViewList.Columns.Add(col);
        }

        private void SetupHeaderPanel()
        {
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 120;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            int y1 = 15, y2 = 45, y3 = 75;

            // 출하번호
            pnlHeader.Controls.Add(CreateLabel("출하번호:", 15, y1));
            txtShipmentNo = new TextEdit();
            txtShipmentNo.Location = new Point(80, y1 - 3);
            txtShipmentNo.Size = new Size(120, 22);
            txtShipmentNo.Properties.ReadOnly = true;
            pnlHeader.Controls.Add(txtShipmentNo);

            // 출하일자
            pnlHeader.Controls.Add(CreateLabel("출하일자:", 220, y1));
            deShipmentDate = new DateEdit();
            deShipmentDate.Location = new Point(285, y1 - 3);
            deShipmentDate.Size = new Size(110, 22);
            deShipmentDate.EditValue = DateTime.Today;
            pnlHeader.Controls.Add(deShipmentDate);

            // 상태
            pnlHeader.Controls.Add(CreateLabel("상태:", 415, y1));
            cboStatus = new ComboBoxEdit();
            cboStatus.Location = new Point(455, y1 - 3);
            cboStatus.Size = new Size(100, 22);
            cboStatus.Properties.Items.AddRange(new[] { "작성", "출하완료", "수금완료" });
            cboStatus.SelectedIndex = 0;
            pnlHeader.Controls.Add(cboStatus);

            // 거래처
            pnlHeader.Controls.Add(CreateLabel("거래처:", 15, y2));
            slueCustomer = new SearchLookUpEdit();
            slueCustomer.Location = new Point(80, y2 - 3);
            slueCustomer.Size = new Size(250, 22);
            slueCustomer.EditValueChanged += SlueCustomer_EditValueChanged;
            pnlHeader.Controls.Add(slueCustomer);

            // 비고
            pnlHeader.Controls.Add(CreateLabel("비고:", 350, y2));
            txtRemark = new TextEdit();
            txtRemark.Location = new Point(390, y2 - 3);
            txtRemark.Size = new Size(300, 22);
            pnlHeader.Controls.Add(txtRemark);

            // 합계 표시
            pnlHeader.Controls.Add(CreateLabel("총수량:", 15, y3));
            lblTotalQty = new LabelControl();
            lblTotalQty.Text = "0";
            lblTotalQty.Location = new Point(70, y3);
            lblTotalQty.AutoSizeMode = LabelAutoSizeMode.None;
            lblTotalQty.Size = new Size(80, 20);
            lblTotalQty.Appearance.Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            pnlHeader.Controls.Add(lblTotalQty);

            pnlHeader.Controls.Add(CreateLabel("총중량:", 170, y3));
            lblTotalWeight = new LabelControl();
            lblTotalWeight.Text = "0.00 kg";
            lblTotalWeight.Location = new Point(225, y3);
            lblTotalWeight.AutoSizeMode = LabelAutoSizeMode.None;
            lblTotalWeight.Size = new Size(100, 20);
            lblTotalWeight.Appearance.Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            pnlHeader.Controls.Add(lblTotalWeight);

            pnlHeader.Controls.Add(CreateLabel("총금액:", 350, y3));
            lblTotalAmount = new LabelControl();
            lblTotalAmount.Text = "0 원";
            lblTotalAmount.Location = new Point(405, y3);
            lblTotalAmount.AutoSizeMode = LabelAutoSizeMode.None;
            lblTotalAmount.Size = new Size(150, 20);
            lblTotalAmount.Appearance.Font = new Font("맑은 고딕", 10, FontStyle.Bold);
            lblTotalAmount.Appearance.ForeColor = AppColors.Primary;
            pnlHeader.Controls.Add(lblTotalAmount);
        }

        private LabelControl CreateLabel(string text, int x, int y)
        {
            var lbl = new LabelControl();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            return lbl;
        }

        private void SetupDetailGrid()
        {
            gridDetail = new GridControl();
            gridDetail.Dock = DockStyle.Fill;
            gridViewDetail = new GridView(gridDetail);
            gridDetail.MainView = gridViewDetail;

            gridViewDetail.Columns.Clear();

            // 품목 검색 콤보
            repoItem = new RepositoryItemSearchLookUpEdit();
            repoItem.NullText = "";
            gridDetail.RepositoryItems.Add(repoItem);

            AddDetailColumn("ItemCode", "품목코드", 90, repoItem);
            AddDetailColumn("ItemName", "품목명", 150);
            AddDetailColumn("Spec", "규격", 150);
            AddDetailColumn("Qty", "수량", 70);
            AddDetailColumn("UnitWeight", "개당중량", 80);
            AddDetailColumn("TotalWeight", "총중량", 80);
            AddDetailColumn("UnitPrice", "단가", 80);
            AddDetailColumn("Amount", "금액", 100);
            AddDetailColumn("Remark", "비고", 120);

            gridViewDetail.OptionsBehavior.Editable = true;
            gridViewDetail.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            gridViewDetail.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewDetail.OptionsView.ShowGroupPanel = false;
            gridViewDetail.RowHeight = 25;

            // 숫자 포맷
            gridViewDetail.Columns["Qty"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["Qty"].DisplayFormat.FormatString = "#,##0";
            gridViewDetail.Columns["UnitWeight"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["UnitWeight"].DisplayFormat.FormatString = "#,##0.0000";
            gridViewDetail.Columns["TotalWeight"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["TotalWeight"].DisplayFormat.FormatString = "#,##0.00";
            gridViewDetail.Columns["UnitPrice"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["UnitPrice"].DisplayFormat.FormatString = "#,##0";
            gridViewDetail.Columns["Amount"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["Amount"].DisplayFormat.FormatString = "#,##0";

            // 읽기전용 컬럼
            gridViewDetail.Columns["ItemName"].OptionsColumn.AllowEdit = false;
            gridViewDetail.Columns["Spec"].OptionsColumn.AllowEdit = false;
            gridViewDetail.Columns["TotalWeight"].OptionsColumn.AllowEdit = false;
            gridViewDetail.Columns["Amount"].OptionsColumn.AllowEdit = false;

            gridViewDetail.CellValueChanged += GridViewDetail_CellValueChanged;

            ThemeManager.ApplyGridViewTheme(gridViewDetail);
            SetupGridExcelExport(gridDetail, gridViewDetail, "출하상세");
        }

        private void AddDetailColumn(string fieldName, string caption, int width, RepositoryItemSearchLookUpEdit repo = null)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewDetail.Columns.Count;
            if (repo != null) col.ColumnEdit = repo;
            gridViewDetail.Columns.Add(col);
        }

        private void LoadMasterData()
        {
            // 거래처 로드
            try
            {
                string sql = "SELECT CustomerCode, CustomerName FROM Customer WHERE IsActive = 1 ORDER BY CustomerName";
                dtCustomer = DbHelper.ExecuteQuery(sql);

                slueCustomer.Properties.DataSource = dtCustomer;
                slueCustomer.Properties.ValueMember = "CustomerCode";
                slueCustomer.Properties.DisplayMember = "CustomerName";
                slueCustomer.Properties.NullText = "";

                var view = slueCustomer.Properties.View;
                view.Columns.Clear();
                view.Columns.AddField("CustomerCode").Caption = "거래처코드";
                view.Columns.AddField("CustomerName").Caption = "거래처명";
            }
            catch { }

            // 품목 로드
            try
            {
                string sql = @"SELECT ItemCode, ItemName, 
                    CONCAT(Shape, ' ', Diameter, 'x', Thickness, 'x', Width, 'x', Length) AS Spec,
                    UnitWeight
                    FROM Item WHERE IsActive = 1 ORDER BY ItemName";
                dtItem = DbHelper.ExecuteQuery(sql);

                repoItem.DataSource = dtItem;
                repoItem.ValueMember = "ItemCode";
                repoItem.DisplayMember = "ItemCode";

                var view = repoItem.View;
                view.Columns.Clear();
                view.Columns.AddField("ItemCode").Caption = "품목코드";
                view.Columns.AddField("ItemName").Caption = "품목명";
                view.Columns.AddField("Spec").Caption = "규격";
            }
            catch { }
        }

        private void SlueCustomer_EditValueChanged(object sender, EventArgs e)
        {
            // 거래처 변경 시 품목 목록 갱신 (거래처별 재고 있는 품목)
        }

        private void GridViewList_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                string shipmentNo = gridViewList.GetRowCellValue(e.FocusedRowHandle, "ShipmentNo")?.ToString();
                LoadShipmentDetail(shipmentNo);
                isNewMode = false;
            }
        }

        private void GridViewDetail_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "ItemCode")
            {
                // 품목 선택 시 품목명, 규격, 개당중량 자동 입력
                string itemCode = e.Value?.ToString();
                if (!string.IsNullOrEmpty(itemCode))
                {
                    var rows = dtItem.Select($"ItemCode = '{itemCode}'");
                    if (rows.Length > 0)
                    {
                        gridViewDetail.SetRowCellValue(e.RowHandle, "ItemName", rows[0]["ItemName"]);
                        gridViewDetail.SetRowCellValue(e.RowHandle, "Spec", rows[0]["Spec"]);
                        gridViewDetail.SetRowCellValue(e.RowHandle, "UnitWeight", rows[0]["UnitWeight"]);
                    }
                }
            }
            else if (e.Column.FieldName == "Qty" || e.Column.FieldName == "UnitWeight" || e.Column.FieldName == "UnitPrice")
            {
                CalculateRow(e.RowHandle);
            }

            CalculateTotals();
        }

        private void CalculateRow(int rowHandle)
        {
            decimal qty = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "Qty") ?? 0);
            decimal unitWeight = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "UnitWeight") ?? 0);
            decimal unitPrice = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "UnitPrice") ?? 0);

            decimal totalWeight = qty * unitWeight;
            decimal amount = totalWeight * unitPrice;

            gridViewDetail.SetRowCellValue(rowHandle, "TotalWeight", totalWeight);
            gridViewDetail.SetRowCellValue(rowHandle, "Amount", amount);
        }

        private void CalculateTotals()
        {
            decimal totalQty = 0, totalWeight = 0, totalAmount = 0;

            for (int i = 0; i < gridViewDetail.RowCount; i++)
            {
                if (gridViewDetail.IsValidRowHandle(i))
                {
                    totalQty += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Qty") ?? 0);
                    totalWeight += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? 0);
                    totalAmount += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Amount") ?? 0);
                }
            }

            lblTotalQty.Text = totalQty.ToString("#,##0");
            lblTotalWeight.Text = totalWeight.ToString("#,##0.00") + " kg";
            lblTotalAmount.Text = totalAmount.ToString("#,##0") + " 원";
        }

        protected override void OnSearchClick()
        {
            LoadList();
        }

        private void LoadList()
        {
            try
            {
                string sql = @"
                    SELECT h.ShipmentNo, h.ShipmentDate, c.CustomerName, h.TotalAmount, h.Status
                    FROM ShipmentHeader h
                    LEFT JOIN Customer c ON h.CustomerCode = c.CustomerCode
                    ORDER BY h.ShipmentNo DESC";

                dtList = DbHelper.ExecuteQuery(sql);
                gridList.DataSource = dtList;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadShipmentDetail(string shipmentNo)
        {
            try
            {
                // 헤더 로드
                string sqlHeader = @"SELECT * FROM ShipmentHeader WHERE ShipmentNo = @ShipmentNo";
                var dtHeader = DbHelper.ExecuteQuery(sqlHeader, new SqlParameter("@ShipmentNo", shipmentNo));

                if (dtHeader.Rows.Count > 0)
                {
                    var row = dtHeader.Rows[0];
                    txtShipmentNo.Text = row["ShipmentNo"].ToString();
                    deShipmentDate.EditValue = row["ShipmentDate"];
                    slueCustomer.EditValue = row["CustomerCode"];
                    cboStatus.Text = row["Status"].ToString();
                    txtRemark.Text = row["Remark"]?.ToString();
                }

                // 상세 로드
                string sqlDetail = @"
                    SELECT d.*, i.ItemName, 
                        CONCAT(i.Shape, ' ', i.Diameter, 'x', i.Thickness, 'x', i.Width, 'x', i.Length) AS Spec
                    FROM ShipmentDetail d
                    LEFT JOIN Item i ON d.ItemCode = i.ItemCode
                    WHERE d.ShipmentNo = @ShipmentNo
                    ORDER BY d.SeqNo";

                dtDetail = DbHelper.ExecuteQuery(sqlDetail, new SqlParameter("@ShipmentNo", shipmentNo));
                gridDetail.DataSource = dtDetail;

                CalculateTotals();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"상세 조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnNewClick()
        {
            // 신규 출하
            isNewMode = true;
            txtShipmentNo.Text = GenerateShipmentNo();
            deShipmentDate.EditValue = DateTime.Today;
            slueCustomer.EditValue = null;
            cboStatus.SelectedIndex = 0;
            txtRemark.Text = "";

            dtDetail = CreateDetailTable();
            gridDetail.DataSource = dtDetail;

            lblTotalQty.Text = "0";
            lblTotalWeight.Text = "0.00 kg";
            lblTotalAmount.Text = "0 원";
        }

        private string GenerateShipmentNo()
        {
            string prefix = "SH" + DateTime.Today.ToString("yyyyMMdd");
            string sql = "SELECT ISNULL(MAX(ShipmentNo), @Prefix + '000') FROM ShipmentHeader WHERE ShipmentNo LIKE @Prefix + '%'";
            string maxNo = DbHelper.ExecuteScalar(sql, new SqlParameter("@Prefix", prefix))?.ToString();

            if (string.IsNullOrEmpty(maxNo) || maxNo == prefix + "000")
                return prefix + "001";

            int seq = int.Parse(maxNo.Substring(maxNo.Length - 3)) + 1;
            return prefix + seq.ToString("000");
        }

        private DataTable CreateDetailTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("ShipmentNo", typeof(string));
            dt.Columns.Add("SeqNo", typeof(int));
            dt.Columns.Add("ItemCode", typeof(string));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("Spec", typeof(string));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("UnitWeight", typeof(decimal));
            dt.Columns.Add("TotalWeight", typeof(decimal));
            dt.Columns.Add("UnitPrice", typeof(decimal));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("Remark", typeof(string));
            return dt;
        }

        protected override void OnSaveClick()
        {
            try
            {
                if (slueCustomer.EditValue == null)
                {
                    XtraMessageBox.Show("거래처를 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                gridViewDetail.CloseEditor();
                gridViewDetail.UpdateCurrentRow();

                string shipmentNo = txtShipmentNo.Text;
                string customerCode = slueCustomer.EditValue.ToString();
                DateTime shipmentDate = Convert.ToDateTime(deShipmentDate.EditValue);
                string status = cboStatus.Text;
                string remark = txtRemark.Text;

                // 합계 계산
                decimal totalQty = 0, totalWeight = 0, totalAmount = 0;
                for (int i = 0; i < gridViewDetail.RowCount; i++)
                {
                    if (gridViewDetail.IsValidRowHandle(i))
                    {
                        totalQty += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Qty") ?? 0);
                        totalWeight += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? 0);
                        totalAmount += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Amount") ?? 0);
                    }
                }

                if (isNewMode)
                {
                    // INSERT 헤더
                    string sqlHeader = @"
                        INSERT INTO ShipmentHeader (ShipmentNo, ShipmentDate, CustomerCode, TotalQty, TotalWeight, TotalAmount, Status, Remark, CreatedBy, CreatedAt)
                        VALUES (@ShipmentNo, @ShipmentDate, @CustomerCode, @TotalQty, @TotalWeight, @TotalAmount, @Status, @Remark, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sqlHeader,
                        new SqlParameter("@ShipmentNo", shipmentNo),
                        new SqlParameter("@ShipmentDate", shipmentDate),
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@TotalQty", totalQty),
                        new SqlParameter("@TotalWeight", totalWeight),
                        new SqlParameter("@TotalAmount", totalAmount),
                        new SqlParameter("@Status", status),
                        new SqlParameter("@Remark", (object)remark ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

                    // 미수금 등록 (출하완료 시)
                    if (status == "출하완료")
                    {
                        InsertReceivable(customerCode, shipmentDate, shipmentNo, totalAmount);
                    }
                }
                else
                {
                    // UPDATE 헤더
                    string sqlHeader = @"
                        UPDATE ShipmentHeader SET ShipmentDate = @ShipmentDate, CustomerCode = @CustomerCode,
                            TotalQty = @TotalQty, TotalWeight = @TotalWeight, TotalAmount = @TotalAmount,
                            Status = @Status, Remark = @Remark, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE ShipmentNo = @ShipmentNo";

                    DbHelper.ExecuteNonQuery(sqlHeader,
                        new SqlParameter("@ShipmentNo", shipmentNo),
                        new SqlParameter("@ShipmentDate", shipmentDate),
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@TotalQty", totalQty),
                        new SqlParameter("@TotalWeight", totalWeight),
                        new SqlParameter("@TotalAmount", totalAmount),
                        new SqlParameter("@Status", status),
                        new SqlParameter("@Remark", (object)remark ?? DBNull.Value),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));

                    // 기존 상세 삭제
                    DbHelper.ExecuteNonQuery("DELETE FROM ShipmentDetail WHERE ShipmentNo = @ShipmentNo",
                        new SqlParameter("@ShipmentNo", shipmentNo));
                }

                // 상세 저장
                int seqNo = 1;
                for (int i = 0; i < gridViewDetail.RowCount; i++)
                {
                    if (!gridViewDetail.IsValidRowHandle(i)) continue;

                    string itemCode = gridViewDetail.GetRowCellValue(i, "ItemCode")?.ToString();
                    if (string.IsNullOrEmpty(itemCode)) continue;

                    string sqlDetail = @"
                        INSERT INTO ShipmentDetail (ShipmentNo, SeqNo, ItemCode, Qty, UnitWeight, TotalWeight, UnitPrice, Amount, Remark)
                        VALUES (@ShipmentNo, @SeqNo, @ItemCode, @Qty, @UnitWeight, @TotalWeight, @UnitPrice, @Amount, @Remark)";

                    DbHelper.ExecuteNonQuery(sqlDetail,
                        new SqlParameter("@ShipmentNo", shipmentNo),
                        new SqlParameter("@SeqNo", seqNo++),
                        new SqlParameter("@ItemCode", itemCode),
                        new SqlParameter("@Qty", gridViewDetail.GetRowCellValue(i, "Qty") ?? 0),
                        new SqlParameter("@UnitWeight", gridViewDetail.GetRowCellValue(i, "UnitWeight") ?? 0),
                        new SqlParameter("@TotalWeight", gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? 0),
                        new SqlParameter("@UnitPrice", gridViewDetail.GetRowCellValue(i, "UnitPrice") ?? 0),
                        new SqlParameter("@Amount", gridViewDetail.GetRowCellValue(i, "Amount") ?? 0),
                        new SqlParameter("@Remark", gridViewDetail.GetRowCellValue(i, "Remark") ?? DBNull.Value));
                }

                XtraMessageBox.Show("저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isNewMode = false;
                LoadList();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"저장 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertReceivable(string customerCode, DateTime transDate, string refNo, decimal amount)
        {
            // 이전 잔액 조회
            string sqlBalance = "SELECT ISNULL(SUM(Amount), 0) FROM Receivable WHERE CustomerCode = @CustomerCode";
            decimal prevBalance = Convert.ToDecimal(DbHelper.ExecuteScalar(sqlBalance, new SqlParameter("@CustomerCode", customerCode)));
            decimal newBalance = prevBalance + amount;

            string sql = @"
                INSERT INTO Receivable (CustomerCode, TransDate, TransType, RefNo, Amount, Balance, CreatedBy, CreatedAt)
                VALUES (@CustomerCode, @TransDate, N'출하', @RefNo, @Amount, @Balance, @CreatedBy, GETDATE())";

            DbHelper.ExecuteNonQuery(sql,
                new SqlParameter("@CustomerCode", customerCode),
                new SqlParameter("@TransDate", transDate),
                new SqlParameter("@RefNo", refNo),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@Balance", newBalance),
                new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
        }

        protected override void OnDeleteClick()
        {
            if (string.IsNullOrEmpty(txtShipmentNo.Text))
            {
                XtraMessageBox.Show("삭제할 출하를 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Confirm($"출하번호 '{txtShipmentNo.Text}'을(를) 삭제하시겠습니까?")) return;

            try
            {
                string shipmentNo = txtShipmentNo.Text;

                DbHelper.ExecuteNonQuery("DELETE FROM ShipmentDetail WHERE ShipmentNo = @ShipmentNo",
                    new SqlParameter("@ShipmentNo", shipmentNo));
                DbHelper.ExecuteNonQuery("DELETE FROM ShipmentHeader WHERE ShipmentNo = @ShipmentNo",
                    new SqlParameter("@ShipmentNo", shipmentNo));

                XtraMessageBox.Show("삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnNewClick();
                LoadList();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
