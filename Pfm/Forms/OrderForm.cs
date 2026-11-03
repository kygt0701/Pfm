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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils;
using Pfm.Common;

namespace Pfm.Forms
{
    /// <summary>
    /// 발주 관리
    /// </summary>
    public class OrderForm : BaseForm
    {
        #region 컨트롤
        // 헤더
        private PanelControl pnlHeader;
        private TextEdit txtOrderNo;
        private DateEdit deOrderDate;
        private SearchLookUpEdit slueCustomer;
        private ComboBoxEdit cboOrderType;
        private ComboBoxEdit cboStatus;
        private MemoEdit txtRemark;
        private LabelControl lblTotalWeight;
        private LabelControl lblTotalAmount;

        // 상세 그리드
        private GridControl gridDetail;
        private GridView gridViewDetail;

        // 엑셀 버튼
        private SimpleButton btnExcel;

        // 품목 검색 팝업
        private RepositoryItemSearchLookUpEdit repoItemLookup;
        private DataTable dtItems;
        #endregion

        private DataTable dtDetail;
        private bool _isNewMode = false;

        public OrderForm()
        {
            MenuId = "WRK_ORD";
            FormTitle = "발주 관리";
            InitializeComponents();
            
            // 폼 로드 시 거래처 데이터 로드
            this.Load += (s, e) => SetupCustomerLookup();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1300, 700);

            // 헤더 패널
            SetupHeaderPanel();

            // 상세 그리드
            SetupDetailGrid();

            // 엑셀 버튼 (그리드 우측 상단)
            SetupExcelButton();

            // 헤더와 그리드를 직접 배치 (SplitContainer 제거)
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 120;
            gridDetail.Dock = DockStyle.Fill;

            pnlContent.Controls.Add(gridDetail);
            pnlContent.Controls.Add(pnlHeader);
        }

        private void SetupHeaderPanel()
        {
            pnlHeader = new PanelControl();
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            int y = 10;
            int labelWidth = 70;
            int gap = 15;

            // Row 1: 발주번호, 발주일자, 거래처, 발주유형, 상태
            AddLabel("발주번호:", 10, y, labelWidth);
            txtOrderNo = new TextEdit();
            txtOrderNo.Location = new Point(10 + labelWidth, y);
            txtOrderNo.Size = new Size(120, 22);
            txtOrderNo.Properties.ReadOnly = true;
            pnlHeader.Controls.Add(txtOrderNo);

            int x = 10 + labelWidth + 120 + gap;
            AddLabel("발주일자:", x, y, labelWidth);
            deOrderDate = new DateEdit();
            deOrderDate.Location = new Point(x + labelWidth, y);
            deOrderDate.Size = new Size(110, 22);
            deOrderDate.EditValue = DateTime.Today;
            pnlHeader.Controls.Add(deOrderDate);

            x += labelWidth + 110 + gap;
            AddLabel("거래처:", x, y, 55);
            slueCustomer = new SearchLookUpEdit();
            slueCustomer.Location = new Point(x + 55, y);
            slueCustomer.Size = new Size(200, 22);
            pnlHeader.Controls.Add(slueCustomer);

            x += 55 + 200 + gap;
            AddLabel("유형:", x, y, 40);
            cboOrderType = new ComboBoxEdit();
            cboOrderType.Location = new Point(x + 40, y);
            cboOrderType.Size = new Size(70, 22);
            cboOrderType.Properties.Items.AddRange(new[] { "발주", "수주" });
            cboOrderType.SelectedIndex = 0;
            pnlHeader.Controls.Add(cboOrderType);

            x += 40 + 70 + gap;
            AddLabel("상태:", x, y, 40);
            cboStatus = new ComboBoxEdit();
            cboStatus.Location = new Point(x + 40, y);
            cboStatus.Size = new Size(80, 22);
            cboStatus.Properties.Items.AddRange(new[] { "접수", "확정", "진행", "완료", "취소" });
            cboStatus.SelectedIndex = 0;
            pnlHeader.Controls.Add(cboStatus);

            // Row 2: 비고, 합계
            y += 32;
            AddLabel("비고:", 10, y, labelWidth);
            txtRemark = new MemoEdit();
            txtRemark.Location = new Point(10 + labelWidth, y);
            txtRemark.Size = new Size(400, 45);
            pnlHeader.Controls.Add(txtRemark);

            // 합계 (우측 정렬)
            lblTotalWeight = new LabelControl();
            lblTotalWeight.Text = "총중량: 0.00 kg";
            lblTotalWeight.Appearance.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTotalWeight.Location = new Point(550, y);
            pnlHeader.Controls.Add(lblTotalWeight);

            lblTotalAmount = new LabelControl();
            lblTotalAmount.Text = "총금액: 0 원";
            lblTotalAmount.Appearance.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblTotalAmount.Location = new Point(550, y + 22);
            pnlHeader.Controls.Add(lblTotalAmount);
        }

        private void AddLabel(string text, int x, int y, int width)
        {
            var lbl = new LabelControl();
            lbl.Text = text;
            lbl.Location = new Point(x, y + 3);
            lbl.AutoSizeMode = LabelAutoSizeMode.None;
            lbl.Size = new Size(width, 20);
            pnlHeader.Controls.Add(lbl);
        }

        private void SetupCustomerLookup()
        {
            try
            {
                string sql = "SELECT CustomerCode, CustomerName, CustomerType FROM Customer WHERE IsActive = 1 ORDER BY CustomerName";
                var dt = DbHelper.ExecuteQuery(sql);
                slueCustomer.Properties.DataSource = dt;
                slueCustomer.Properties.DisplayMember = "CustomerName";
                slueCustomer.Properties.ValueMember = "CustomerCode";

                var view = slueCustomer.Properties.View;
                view.Columns.Clear();
                view.Columns.AddField("CustomerCode").Caption = "코드";
                view.Columns.AddField("CustomerName").Caption = "거래처명";
                view.Columns.AddField("CustomerType").Caption = "유형";
                view.Columns["CustomerCode"].Width = 80;
                view.Columns["CustomerName"].Width = 150;
                view.Columns["CustomerType"].Width = 80;

                // 팝업 그리드 글자색 검정색으로 설정
                view.Appearance.Row.ForeColor = System.Drawing.Color.Black;
                view.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
                view.Appearance.SelectedRow.ForeColor = System.Drawing.Color.Black;
                view.Appearance.HeaderPanel.ForeColor = System.Drawing.Color.Black;
            }
            catch { }
        }

        private void SetupItemLookup()
        {
            try
            {
                // 품목 데이터 로드
                string sql = @"SELECT ItemCode, ItemName, Shape, Grade, SpecificGravity, UnitPrice 
                               FROM Item WHERE IsActive = 1 ORDER BY ItemName";
                dtItems = DbHelper.ExecuteQuery(sql);

                // SearchLookUpEdit 설정
                repoItemLookup = new RepositoryItemSearchLookUpEdit();
                repoItemLookup.DataSource = dtItems;
                repoItemLookup.DisplayMember = "ItemName";
                repoItemLookup.ValueMember = "ItemName";
                repoItemLookup.NullText = "";
                repoItemLookup.PopupFormSize = new Size(500, 300);

                // 팝업 그리드 컬럼 설정
                var view = repoItemLookup.View;
                view.Columns.Clear();
                view.Columns.AddField("ItemCode").Caption = "품목코드";
                view.Columns.AddField("ItemName").Caption = "제품명";
                view.Columns.AddField("Shape").Caption = "형태";
                view.Columns.AddField("Grade").Caption = "등급";
                view.Columns.AddField("SpecificGravity").Caption = "비중";
                view.Columns.AddField("UnitPrice").Caption = "단가";
                view.Columns["ItemCode"].Width = 80;
                view.Columns["ItemName"].Width = 150;
                view.Columns["Shape"].Width = 60;
                view.Columns["Grade"].Width = 60;
                view.Columns["SpecificGravity"].Width = 60;
                view.Columns["UnitPrice"].Width = 80;

                // 팝업 그리드 글자색 검정색
                view.Appearance.Row.ForeColor = Color.Black;
                view.Appearance.FocusedRow.ForeColor = Color.Black;
                view.Appearance.SelectedRow.ForeColor = Color.Black;
                view.Appearance.HeaderPanel.ForeColor = Color.Black;
            }
            catch { }
        }

        private void ApplyItemDataToRow(int rowHandle, string itemName)
        {
            if (string.IsNullOrEmpty(itemName) || dtItems == null) return;

            try
            {
                DataRow[] rows = dtItems.Select($"ItemName = '{itemName.Replace("'", "''")}'");
                if (rows.Length == 0) return;

                DataRow itemRow = rows[0];

                // 관련 필드 자동 입력 (BeginUpdate/EndUpdate로 감싸서 이벤트 중복 방지)
                gridViewDetail.BeginUpdate();
                try
                {
                    gridViewDetail.SetRowCellValue(rowHandle, "ItemCode", itemRow["ItemCode"]);
                    gridViewDetail.SetRowCellValue(rowHandle, "Shape", itemRow["Shape"]);
                    gridViewDetail.SetRowCellValue(rowHandle, "Grade", itemRow["Grade"]);
                    gridViewDetail.SetRowCellValue(rowHandle, "SpecificGravity", itemRow["SpecificGravity"]);
                    gridViewDetail.SetRowCellValue(rowHandle, "UnitPrice", itemRow["UnitPrice"]);
                }
                finally
                {
                    gridViewDetail.EndUpdate();
                }
            }
            catch { }
        }

        private void SetupDetailGrid()
        {
            gridDetail = new GridControl();
            gridDetail.Dock = DockStyle.Fill;

            gridViewDetail = new GridView(gridDetail);
            gridDetail.MainView = gridViewDetail;

            gridViewDetail.Columns.Clear();

            // 품목 검색 팝업 설정
            SetupItemLookup();

            AddDetailColumn("Seq", "순번", 50, false);
            AddDetailColumn("ItemCode", "품목코드", 80, false);
            AddDetailColumnWithLookup("ItemName", "제품명", 150, repoItemLookup);
            AddDetailColumn("Shape", "형태", 50, false);
            AddDetailColumn("Grade", "등급", 50, false);
            AddDetailColumn("Diameter", "직경", 60);
            AddDetailColumn("Thickness", "두께", 60);
            AddDetailColumn("Width", "너비", 60);
            AddDetailColumn("Length", "길이", 60);
            AddDetailColumn("Quantity", "수량", 60);
            AddDetailColumn("SpecificGravity", "비중", 60, false);
            AddDetailColumn("Division", "구분", 50);
            AddDetailColumn("UnitWeight", "개당중량", 80);
            AddDetailColumn("TotalWeight", "합계중량", 80, false);
            AddDetailColumn("UnitPrice", "단가", 80, false);
            AddDetailColumn("Amount", "금액", 100, false);
            AddDetailColumn("Remark", "비고", 120);

            // 그리드 옵션
            gridViewDetail.OptionsBehavior.Editable = true;
            gridViewDetail.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            gridViewDetail.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewDetail.OptionsView.ShowFooter = true;
            gridViewDetail.OptionsView.ShowGroupPanel = false;  // 그룹 패널 숨기기
            gridViewDetail.RowHeight = 25;

            // 숫자 포맷
            SetNumericFormat("Diameter", "N2");
            SetNumericFormat("Thickness", "N4");
            SetNumericFormat("SpecificGravity", "N4");
            SetNumericFormat("UnitWeight", "N4");
            SetNumericFormat("TotalWeight", "N2");
            SetNumericFormat("UnitPrice", "N0");
            SetNumericFormat("Amount", "N0");

            // 합계
            gridViewDetail.Columns["TotalWeight"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewDetail.Columns["TotalWeight"].SummaryItem.DisplayFormat = "{0:N2}";
            gridViewDetail.Columns["Amount"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewDetail.Columns["Amount"].SummaryItem.DisplayFormat = "{0:N0}";

            // 셀 값 변경 이벤트
            gridViewDetail.CellValueChanged += GridViewDetail_CellValueChanged;

            ThemeManager.ApplyGridViewTheme(gridViewDetail);
        }

        private void SetupExcelButton()
        {
            // 엑셀 버튼
            btnExcel = new SimpleButton();
            btnExcel.Text = "Excel";
            btnExcel.Size = new Size(70, 25);
            btnExcel.Appearance.BackColor = Color.FromArgb(34, 139, 34);
            btnExcel.Appearance.ForeColor = Color.White;
            btnExcel.Appearance.Options.UseBackColor = true;
            btnExcel.Appearance.Options.UseForeColor = true;
            btnExcel.Click += (s, e) => ExportGridToExcel(gridDetail, gridViewDetail, $"발주내역_{txtOrderNo.Text}");
            btnExcel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pnlContent.Controls.Add(btnExcel);

            this.Load += (s, e) => 
            {
                btnExcel.Location = new Point(pnlContent.Width - btnExcel.Width - 10, pnlHeader.Height + 5);
                btnExcel.BringToFront();
            };

            // 그리드 우클릭 메뉴
            SetupGridExcelExport(gridDetail, gridViewDetail, $"발주내역_{txtOrderNo.Text}");
        }

        private void AddDetailColumn(string fieldName, string caption, int width, bool editable = true)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewDetail.Columns.Count;
            if (!editable)
            {
                col.OptionsColumn.AllowEdit = false;
                col.OptionsColumn.ReadOnly = true;
            }
            gridViewDetail.Columns.Add(col);
        }

        private void AddDetailColumnWithLookup(string fieldName, string caption, int width, RepositoryItemSearchLookUpEdit repo)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewDetail.Columns.Count;
            col.ColumnEdit = repo;
            gridViewDetail.Columns.Add(col);
            gridDetail.RepositoryItems.Add(repo);
        }

        private void SetNumericFormat(string fieldName, string format)
        {
            var col = gridViewDetail.Columns[fieldName];
            if (col != null)
            {
                col.DisplayFormat.FormatType = FormatType.Numeric;
                col.DisplayFormat.FormatString = format;
            }
        }

        private void GridViewDetail_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            // 품목 선택 시 관련 필드 자동 입력
            if (e.Column.FieldName == "ItemName")
            {
                string itemName = e.Value?.ToString();
                if (!string.IsNullOrEmpty(itemName))
                {
                    ApplyItemDataToRow(e.RowHandle, itemName);
                }
            }

            // 중량 및 금액 자동 계산
            if (e.Column.FieldName == "ItemName" || e.Column.FieldName == "Thickness" || 
                e.Column.FieldName == "Width" || e.Column.FieldName == "Length" || 
                e.Column.FieldName == "Quantity" || e.Column.FieldName == "SpecificGravity" || 
                e.Column.FieldName == "Division" || e.Column.FieldName == "UnitPrice")
            {
                CalculateRow(e.RowHandle);
                UpdateTotals();
            }
        }

        private void CalculateRow(int rowHandle)
        {
            try
            {
                decimal thickness = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "Thickness") ?? 0);
                decimal width = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "Width") ?? 0);
                decimal length = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "Length") ?? 0);
                decimal quantity = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "Quantity") ?? 0);
                decimal specificGravity = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "SpecificGravity") ?? 0.0092m);
                int division = Convert.ToInt32(gridViewDetail.GetRowCellValue(rowHandle, "Division") ?? 1);
                decimal unitPrice = Convert.ToDecimal(gridViewDetail.GetRowCellValue(rowHandle, "UnitPrice") ?? 0);

                // 개당중량 = 두께 * 너비 * 길이 * 비중 * 2 / 구분 / 1000
                decimal unitWeight = thickness * width * length * specificGravity * 2 / (division == 0 ? 1 : division) / 1000;

                // 합계중량 = 개당중량 * 수량
                decimal totalWeight = unitWeight * quantity;

                // 금액 = 합계중량 * 단가
                decimal amount = totalWeight * unitPrice;

                gridViewDetail.SetRowCellValue(rowHandle, "UnitWeight", Math.Round(unitWeight, 4));
                gridViewDetail.SetRowCellValue(rowHandle, "TotalWeight", Math.Round(totalWeight, 2));
                gridViewDetail.SetRowCellValue(rowHandle, "Amount", Math.Round(amount, 0));
            }
            catch { }
        }

        private void UpdateTotals()
        {
            decimal totalWeight = 0;
            decimal totalAmount = 0;

            for (int i = 0; i < gridViewDetail.RowCount; i++)
            {
                totalWeight += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? 0);
                totalAmount += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Amount") ?? 0);
            }

            lblTotalWeight.Text = $"총중량: {totalWeight:N2} kg";
            lblTotalAmount.Text = $"총금액: {totalAmount:N0} 원";
        }

        protected override void OnNewClick()
        {
            _isNewMode = true;
            txtOrderNo.Text = GenerateOrderNo();
            deOrderDate.EditValue = DateTime.Today;
            slueCustomer.EditValue = null;
            cboOrderType.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            txtRemark.Text = "";

            // 빈 데이터 테이블
            dtDetail = new DataTable();
            dtDetail.Columns.Add("Seq", typeof(int));
            dtDetail.Columns.Add("ItemCode", typeof(string));
            dtDetail.Columns.Add("ItemName", typeof(string));
            dtDetail.Columns.Add("Shape", typeof(string));
            dtDetail.Columns.Add("Grade", typeof(string));
            dtDetail.Columns.Add("Diameter", typeof(decimal));
            dtDetail.Columns.Add("Thickness", typeof(decimal));
            dtDetail.Columns.Add("Width", typeof(decimal));
            dtDetail.Columns.Add("Length", typeof(decimal));
            dtDetail.Columns.Add("Quantity", typeof(decimal));
            dtDetail.Columns.Add("SpecificGravity", typeof(decimal));
            dtDetail.Columns.Add("Division", typeof(int));
            dtDetail.Columns.Add("UnitWeight", typeof(decimal));
            dtDetail.Columns.Add("TotalWeight", typeof(decimal));
            dtDetail.Columns.Add("UnitPrice", typeof(decimal));
            dtDetail.Columns.Add("Amount", typeof(decimal));
            dtDetail.Columns.Add("Remark", typeof(string));

            gridDetail.DataSource = dtDetail;
            UpdateTotals();
        }

        private string GenerateOrderNo()
        {
            string prefix = DateTime.Today.ToString("yyyyMMdd");
            try
            {
                string sql = "SELECT ISNULL(MAX(CAST(RIGHT(OrderNo, 4) AS INT)), 0) + 1 FROM OrderHeader WHERE OrderNo LIKE @Prefix + '%'";
                var result = DbHelper.ExecuteScalar(sql, new SqlParameter("@Prefix", prefix));
                int seq = Convert.ToInt32(result);
                return $"{prefix}{seq:D4}";
            }
            catch
            {
                return $"{prefix}0001";
            }
        }

        protected override void OnSearchClick()
        {
            // 간단한 조회 다이얼로그 (추후 확장)
            var result = XtraInputBox.Show("발주번호를 입력하세요:", "발주번호 조회", "");
            if (!string.IsNullOrEmpty(result))
            {
                LoadOrder(result);
            }
        }

        private void LoadOrder(string orderNo)
        {
            try
            {
                // 헤더 조회
                string sqlHeader = @"
                    SELECT o.*, c.CustomerName 
                    FROM OrderHeader o
                    LEFT JOIN Customer c ON o.CustomerCode = c.CustomerCode
                    WHERE o.OrderNo = @OrderNo";

                var dtHeader = DbHelper.ExecuteQuery(sqlHeader, new SqlParameter("@OrderNo", orderNo));
                if (dtHeader.Rows.Count == 0)
                {
                    XtraMessageBox.Show("발주 정보를 찾을 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var row = dtHeader.Rows[0];
                txtOrderNo.Text = row["OrderNo"].ToString();
                deOrderDate.EditValue = row["OrderDate"];
                slueCustomer.EditValue = row["CustomerCode"];
                cboOrderType.Text = row["OrderType"]?.ToString() == "PO" ? "발주" : "수주";

                string status = row["Status"]?.ToString();
                switch (status)
                {
                    case "10": cboStatus.SelectedIndex = 0; break;
                    case "20": cboStatus.SelectedIndex = 1; break;
                    case "30": cboStatus.SelectedIndex = 2; break;
                    case "40": cboStatus.SelectedIndex = 3; break;
                    case "90": cboStatus.SelectedIndex = 4; break;
                }
                txtRemark.Text = row["Remark"]?.ToString();

                // 상세 조회
                string sqlDetail = @"
                    SELECT Seq, ItemCode, ItemName, Shape, Grade, Diameter, Thickness, Width, Length,
                           Quantity, SpecificGravity, Division, UnitWeight, TotalWeight, UnitPrice, Amount, Remark
                    FROM OrderDetail
                    WHERE OrderNo = @OrderNo
                    ORDER BY Seq";

                dtDetail = DbHelper.ExecuteQuery(sqlDetail, new SqlParameter("@OrderNo", orderNo));
                gridDetail.DataSource = dtDetail;

                _isNewMode = false;
                UpdateTotals();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnSaveClick()
        {
            try
            {
                gridViewDetail.CloseEditor();
                gridViewDetail.UpdateCurrentRow();

                if (slueCustomer.EditValue == null)
                {
                    XtraMessageBox.Show("거래처를 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string orderNo = txtOrderNo.Text;
                string customerCode = slueCustomer.EditValue.ToString();
                DateTime orderDate = Convert.ToDateTime(deOrderDate.EditValue);
                string orderType = cboOrderType.Text == "발주" ? "PO" : "SO";
                string status = new[] { "10", "20", "30", "40", "90" }[cboStatus.SelectedIndex];

                // 합계 계산
                decimal totalQty = 0, totalWeight = 0, totalAmount = 0;
                for (int i = 0; i < gridViewDetail.RowCount; i++)
                {
                    totalQty += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Quantity") ?? 0);
                    totalWeight += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? 0);
                    totalAmount += Convert.ToDecimal(gridViewDetail.GetRowCellValue(i, "Amount") ?? 0);
                }

                if (_isNewMode)
                {
                    // 헤더 INSERT
                    string sqlHeader = @"
                        INSERT INTO OrderHeader (OrderNo, OrderDate, CustomerCode, OrderType, Status, 
                            TotalQty, TotalWeight, TotalAmount, Remark, CreatedBy, CreatedAt)
                        VALUES (@OrderNo, @OrderDate, @CustomerCode, @OrderType, @Status,
                            @TotalQty, @TotalWeight, @TotalAmount, @Remark, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sqlHeader,
                        new SqlParameter("@OrderNo", orderNo),
                        new SqlParameter("@OrderDate", orderDate),
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@OrderType", orderType),
                        new SqlParameter("@Status", status),
                        new SqlParameter("@TotalQty", totalQty),
                        new SqlParameter("@TotalWeight", totalWeight),
                        new SqlParameter("@TotalAmount", totalAmount),
                        new SqlParameter("@Remark", txtRemark.Text ?? ""),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
                else
                {
                    // 헤더 UPDATE
                    string sqlHeader = @"
                        UPDATE OrderHeader SET OrderDate = @OrderDate, CustomerCode = @CustomerCode, 
                            OrderType = @OrderType, Status = @Status, TotalQty = @TotalQty,
                            TotalWeight = @TotalWeight, TotalAmount = @TotalAmount, Remark = @Remark,
                            ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE OrderNo = @OrderNo";

                    DbHelper.ExecuteNonQuery(sqlHeader,
                        new SqlParameter("@OrderNo", orderNo),
                        new SqlParameter("@OrderDate", orderDate),
                        new SqlParameter("@CustomerCode", customerCode),
                        new SqlParameter("@OrderType", orderType),
                        new SqlParameter("@Status", status),
                        new SqlParameter("@TotalQty", totalQty),
                        new SqlParameter("@TotalWeight", totalWeight),
                        new SqlParameter("@TotalAmount", totalAmount),
                        new SqlParameter("@Remark", txtRemark.Text ?? ""),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));

                    // 기존 상세 삭제
                    DbHelper.ExecuteNonQuery("DELETE FROM OrderDetail WHERE OrderNo = @OrderNo",
                        new SqlParameter("@OrderNo", orderNo));
                }

                // 상세 INSERT
                int seq = 1;
                for (int i = 0; i < gridViewDetail.RowCount; i++)
                {
                    string sqlDetail = @"
                        INSERT INTO OrderDetail (OrderNo, Seq, ItemCode, ItemName, Shape, Grade, Diameter, Thickness,
                            Width, Length, Quantity, SpecificGravity, Division, UnitWeight, TotalWeight,
                            UnitPrice, Amount, Remark, CreatedBy, CreatedAt)
                        VALUES (@OrderNo, @Seq, @ItemCode, @ItemName, @Shape, @Grade, @Diameter, @Thickness,
                            @Width, @Length, @Quantity, @SpecificGravity, @Division, @UnitWeight, @TotalWeight,
                            @UnitPrice, @Amount, @Remark, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sqlDetail,
                        new SqlParameter("@OrderNo", orderNo),
                        new SqlParameter("@Seq", seq++),
                        new SqlParameter("@ItemCode", gridViewDetail.GetRowCellValue(i, "ItemCode") ?? DBNull.Value),
                        new SqlParameter("@ItemName", gridViewDetail.GetRowCellValue(i, "ItemName") ?? DBNull.Value),
                        new SqlParameter("@Shape", gridViewDetail.GetRowCellValue(i, "Shape") ?? DBNull.Value),
                        new SqlParameter("@Grade", gridViewDetail.GetRowCellValue(i, "Grade") ?? DBNull.Value),
                        new SqlParameter("@Diameter", gridViewDetail.GetRowCellValue(i, "Diameter") ?? DBNull.Value),
                        new SqlParameter("@Thickness", gridViewDetail.GetRowCellValue(i, "Thickness") ?? DBNull.Value),
                        new SqlParameter("@Width", gridViewDetail.GetRowCellValue(i, "Width") ?? DBNull.Value),
                        new SqlParameter("@Length", gridViewDetail.GetRowCellValue(i, "Length") ?? DBNull.Value),
                        new SqlParameter("@Quantity", gridViewDetail.GetRowCellValue(i, "Quantity") ?? DBNull.Value),
                        new SqlParameter("@SpecificGravity", gridViewDetail.GetRowCellValue(i, "SpecificGravity") ?? 0.0092m),
                        new SqlParameter("@Division", gridViewDetail.GetRowCellValue(i, "Division") ?? 1),
                        new SqlParameter("@UnitWeight", gridViewDetail.GetRowCellValue(i, "UnitWeight") ?? DBNull.Value),
                        new SqlParameter("@TotalWeight", gridViewDetail.GetRowCellValue(i, "TotalWeight") ?? DBNull.Value),
                        new SqlParameter("@UnitPrice", gridViewDetail.GetRowCellValue(i, "UnitPrice") ?? DBNull.Value),
                        new SqlParameter("@Amount", gridViewDetail.GetRowCellValue(i, "Amount") ?? DBNull.Value),
                        new SqlParameter("@Remark", gridViewDetail.GetRowCellValue(i, "Remark") ?? DBNull.Value),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }

                XtraMessageBox.Show("저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _isNewMode = false;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"저장 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnDeleteClick()
        {
            if (string.IsNullOrEmpty(txtOrderNo.Text)) return;

            if (XtraMessageBox.Show($"발주 '{txtOrderNo.Text}'를 삭제하시겠습니까?", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    DbHelper.ExecuteNonQuery("DELETE FROM OrderDetail WHERE OrderNo = @OrderNo",
                        new SqlParameter("@OrderNo", txtOrderNo.Text));
                    DbHelper.ExecuteNonQuery("DELETE FROM OrderHeader WHERE OrderNo = @OrderNo",
                        new SqlParameter("@OrderNo", txtOrderNo.Text));

                    XtraMessageBox.Show("삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OnNewClick();
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
