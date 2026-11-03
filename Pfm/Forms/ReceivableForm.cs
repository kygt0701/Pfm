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
    /// 미수금 관리 (거래처별)
    /// </summary>
    public class ReceivableForm : BaseForm
    {
        #region 컨트롤
        private SplitContainerControl splitContainer;

        // 거래처별 미수금 현황
        private GridControl gridSummary;
        private GridView gridViewSummary;

        // 거래 내역
        private PanelControl pnlDetail;
        private LabelControl lblCustomerInfo;
        private GridControl gridDetail;
        private GridView gridViewDetail;

        // 수금 입력
        private PanelControl pnlCollection;
        private DateEdit deCollectionDate;
        private SpinEdit seAmount;
        private ComboBoxEdit cboPayMethod;
        private TextEdit txtRemark;
        private SimpleButton btnAddCollection;
        #endregion

        private DataTable dtSummary;
        private DataTable dtDetail;
        private string selectedCustomerCode;

        public ReceivableForm()
        {
            MenuId = "WRK_RECV";
            FormTitle = "미수금 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 700);

            // 스플릿 컨테이너 (좌: 거래처별 현황, 우: 상세)
            splitContainer = new SplitContainerControl();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterPosition = 400;
            splitContainer.FixedPanel = SplitFixedPanel.Panel1;

            // 좌측: 거래처별 미수금 현황
            SetupSummaryGrid();
            splitContainer.Panel1.Controls.Add(gridSummary);

            // 우측: 거래 내역 + 수금 입력
            var pnlRight = new PanelControl();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            SetupCollectionPanel();
            SetupDetailGrid();

            pnlRight.Controls.Add(gridDetail);
            pnlRight.Controls.Add(pnlCollection);
            splitContainer.Panel2.Controls.Add(pnlRight);

            pnlContent.Controls.Add(splitContainer);
        }

        private void SetupSummaryGrid()
        {
            gridSummary = new GridControl();
            gridSummary.Dock = DockStyle.Fill;
            gridViewSummary = new GridView(gridSummary);
            gridSummary.MainView = gridViewSummary;

            gridViewSummary.Columns.Clear();
            AddSummaryColumn("CustomerCode", "거래처코드", 90);
            AddSummaryColumn("CustomerName", "거래처명", 150);
            AddSummaryColumn("TotalShipment", "총출하액", 100);
            AddSummaryColumn("TotalCollection", "총수금액", 100);
            AddSummaryColumn("Balance", "미수잔액", 100);

            gridViewSummary.OptionsBehavior.Editable = false;
            gridViewSummary.OptionsView.ShowGroupPanel = false;
            gridViewSummary.OptionsView.ShowFooter = true;
            gridViewSummary.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridViewSummary.FocusedRowChanged += GridViewSummary_FocusedRowChanged;

            // 숫자 포맷
            gridViewSummary.Columns["TotalShipment"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewSummary.Columns["TotalShipment"].DisplayFormat.FormatString = "#,##0";
            gridViewSummary.Columns["TotalCollection"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewSummary.Columns["TotalCollection"].DisplayFormat.FormatString = "#,##0";
            gridViewSummary.Columns["Balance"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewSummary.Columns["Balance"].DisplayFormat.FormatString = "#,##0";

            // 합계
            gridViewSummary.Columns["TotalShipment"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewSummary.Columns["TotalShipment"].SummaryItem.DisplayFormat = "{0:#,##0}";
            gridViewSummary.Columns["TotalCollection"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewSummary.Columns["TotalCollection"].SummaryItem.DisplayFormat = "{0:#,##0}";
            gridViewSummary.Columns["Balance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewSummary.Columns["Balance"].SummaryItem.DisplayFormat = "{0:#,##0}";

            // 미수잔액 컬럼 색상 (양수: 빨강)
            gridViewSummary.RowCellStyle += GridViewSummary_RowCellStyle;

            // 거래처코드, 거래처명 고정
            gridViewSummary.Columns["CustomerCode"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridViewSummary.Columns["CustomerName"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            ThemeManager.ApplyGridViewTheme(gridViewSummary);
            SetupGridExcelExport(gridSummary, gridViewSummary, "미수금요약");
        }

        private void GridViewSummary_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "Balance")
            {
                decimal balance = Convert.ToDecimal(e.CellValue ?? 0);
                if (balance > 0)
                {
                    e.Appearance.ForeColor = Color.Red;
                    e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
                }
            }
        }

        private void AddSummaryColumn(string fieldName, string caption, int width)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewSummary.Columns.Count;
            gridViewSummary.Columns.Add(col);
        }

        private void SetupCollectionPanel()
        {
            pnlCollection = new PanelControl();
            pnlCollection.Dock = DockStyle.Top;
            pnlCollection.Height = 90;
            pnlCollection.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // 거래처 정보
            lblCustomerInfo = new LabelControl();
            lblCustomerInfo.Text = "거래처를 선택하세요";
            lblCustomerInfo.Location = new Point(15, 10);
            lblCustomerInfo.Appearance.Font = new Font("맑은 고딕", 11, FontStyle.Bold);
            pnlCollection.Controls.Add(lblCustomerInfo);

            int y = 45;

            // 수금일자
            pnlCollection.Controls.Add(CreateLabel("수금일자:", 15, y));
            deCollectionDate = new DateEdit();
            deCollectionDate.Location = new Point(75, y - 3);
            deCollectionDate.Size = new Size(110, 22);
            deCollectionDate.EditValue = DateTime.Today;
            pnlCollection.Controls.Add(deCollectionDate);

            // 수금액
            pnlCollection.Controls.Add(CreateLabel("수금액:", 200, y));
            seAmount = new SpinEdit();
            seAmount.Location = new Point(250, y - 3);
            seAmount.Size = new Size(120, 22);
            seAmount.Properties.IsFloatValue = false;
            seAmount.Properties.Mask.EditMask = "N0";
            seAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
            pnlCollection.Controls.Add(seAmount);

            // 결제방법
            pnlCollection.Controls.Add(CreateLabel("결제방법:", 390, y));
            cboPayMethod = new ComboBoxEdit();
            cboPayMethod.Location = new Point(450, y - 3);
            cboPayMethod.Size = new Size(100, 22);
            LoadPaymentMethods();
            pnlCollection.Controls.Add(cboPayMethod);

            // 비고
            pnlCollection.Controls.Add(CreateLabel("비고:", 570, y));
            txtRemark = new TextEdit();
            txtRemark.Location = new Point(605, y - 3);
            txtRemark.Size = new Size(150, 22);
            pnlCollection.Controls.Add(txtRemark);

            // 수금 등록 버튼
            btnAddCollection = new SimpleButton();
            btnAddCollection.Text = "수금 등록";
            btnAddCollection.Location = new Point(770, y - 5);
            btnAddCollection.Size = new Size(90, 28);
            btnAddCollection.Click += BtnAddCollection_Click;
            ThemeManager.ApplyPrimaryButton(btnAddCollection);
            pnlCollection.Controls.Add(btnAddCollection);
        }

        private LabelControl CreateLabel(string text, int x, int y)
        {
            var lbl = new LabelControl();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            return lbl;
        }

        private void LoadPaymentMethods()
        {
            try
            {
                string sql = "SELECT CodeName FROM CommonCode WHERE GroupCode = 'PAY_METHOD' AND IsActive = 1 ORDER BY SortOrder";
                var dt = DbHelper.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    cboPayMethod.Properties.Items.Add(row["CodeName"].ToString());
                }
                if (cboPayMethod.Properties.Items.Count > 0)
                    cboPayMethod.SelectedIndex = 0;
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
            AddDetailColumn("TransDate", "거래일자", 90);
            AddDetailColumn("TransType", "구분", 60);
            AddDetailColumn("RefNo", "참조번호", 120);
            AddDetailColumn("Amount", "금액", 100);
            AddDetailColumn("Balance", "잔액", 100);
            AddDetailColumn("Remark", "비고", 150);
            AddDetailColumn("CreatedBy", "등록자", 80);
            AddDetailColumn("CreatedAt", "등록일시", 130);

            gridViewDetail.OptionsBehavior.Editable = false;
            gridViewDetail.OptionsView.ShowGroupPanel = false;
            gridViewDetail.OptionsView.ShowFooter = true;

            // 숫자 포맷
            gridViewDetail.Columns["Amount"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["Amount"].DisplayFormat.FormatString = "#,##0";
            gridViewDetail.Columns["Balance"].DisplayFormat.FormatType = FormatType.Numeric;
            gridViewDetail.Columns["Balance"].DisplayFormat.FormatString = "#,##0";

            // 합계
            gridViewDetail.Columns["Amount"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridViewDetail.Columns["Amount"].SummaryItem.DisplayFormat = "{0:#,##0}";

            // 구분별 색상
            gridViewDetail.RowCellStyle += GridViewDetail_RowCellStyle;

            ThemeManager.ApplyGridViewTheme(gridViewDetail);
            SetupGridExcelExport(gridDetail, gridViewDetail, "미수금상세");
        }

        private void GridViewDetail_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "TransType" || e.Column.FieldName == "Amount")
            {
                string transType = gridViewDetail.GetRowCellValue(e.RowHandle, "TransType")?.ToString();
                if (transType == "출하")
                {
                    e.Appearance.ForeColor = Color.Blue;
                }
                else if (transType == "수금")
                {
                    e.Appearance.ForeColor = Color.Green;
                }
            }
        }

        private void AddDetailColumn(string fieldName, string caption, int width)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewDetail.Columns.Count;
            gridViewDetail.Columns.Add(col);
        }

        private void GridViewSummary_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                selectedCustomerCode = gridViewSummary.GetRowCellValue(e.FocusedRowHandle, "CustomerCode")?.ToString();
                string customerName = gridViewSummary.GetRowCellValue(e.FocusedRowHandle, "CustomerName")?.ToString();
                decimal balance = Convert.ToDecimal(gridViewSummary.GetRowCellValue(e.FocusedRowHandle, "Balance") ?? 0);

                lblCustomerInfo.Text = $"{customerName} ({selectedCustomerCode}) - 미수잔액: {balance:#,##0}원";
                LoadDetail(selectedCustomerCode);
            }
        }

        protected override void OnSearchClick()
        {
            LoadSummary();
        }

        private void LoadSummary()
        {
            try
            {
                string sql = @"
                    SELECT c.CustomerCode, c.CustomerName,
                        ISNULL(SUM(CASE WHEN r.TransType = N'출하' THEN r.Amount ELSE 0 END), 0) AS TotalShipment,
                        ISNULL(SUM(CASE WHEN r.TransType = N'수금' THEN ABS(r.Amount) ELSE 0 END), 0) AS TotalCollection,
                        ISNULL(SUM(r.Amount), 0) AS Balance
                    FROM Customer c
                    LEFT JOIN Receivable r ON c.CustomerCode = r.CustomerCode
                    WHERE c.IsActive = 1
                    GROUP BY c.CustomerCode, c.CustomerName
                    HAVING ISNULL(SUM(r.Amount), 0) <> 0 OR EXISTS (SELECT 1 FROM Receivable WHERE CustomerCode = c.CustomerCode)
                    ORDER BY c.CustomerName";

                dtSummary = DbHelper.ExecuteQuery(sql);
                gridSummary.DataSource = dtSummary;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDetail(string customerCode)
        {
            try
            {
                string sql = @"
                    SELECT TransDate, TransType, RefNo, Amount, Balance, Remark, CreatedBy, CreatedAt
                    FROM Receivable
                    WHERE CustomerCode = @CustomerCode
                    ORDER BY TransDate DESC, ReceivableId DESC";

                dtDetail = DbHelper.ExecuteQuery(sql, new SqlParameter("@CustomerCode", customerCode));
                gridDetail.DataSource = dtDetail;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"상세 조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddCollection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedCustomerCode))
            {
                XtraMessageBox.Show("거래처를 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal amount = Convert.ToDecimal(seAmount.Value);
            if (amount <= 0)
            {
                XtraMessageBox.Show("수금액을 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DateTime collectionDate = Convert.ToDateTime(deCollectionDate.EditValue);
                string payMethod = cboPayMethod.Text;
                string remark = txtRemark.Text;

                // 수금번호 생성
                string collectionNo = GenerateCollectionNo();

                // 이전 잔액 조회
                string sqlBalance = "SELECT ISNULL(SUM(Amount), 0) FROM Receivable WHERE CustomerCode = @CustomerCode";
                decimal prevBalance = Convert.ToDecimal(DbHelper.ExecuteScalar(sqlBalance, new SqlParameter("@CustomerCode", selectedCustomerCode)));
                decimal newBalance = prevBalance - amount;  // 수금은 마이너스

                // 수금 등록
                string sqlCollection = @"
                    INSERT INTO Collection (CollectionNo, CollectionDate, CustomerCode, Amount, PaymentMethod, Remark, CreatedBy, CreatedAt)
                    VALUES (@CollectionNo, @CollectionDate, @CustomerCode, @Amount, @PaymentMethod, @Remark, @CreatedBy, GETDATE())";

                DbHelper.ExecuteNonQuery(sqlCollection,
                    new SqlParameter("@CollectionNo", collectionNo),
                    new SqlParameter("@CollectionDate", collectionDate),
                    new SqlParameter("@CustomerCode", selectedCustomerCode),
                    new SqlParameter("@Amount", amount),
                    new SqlParameter("@PaymentMethod", payMethod),
                    new SqlParameter("@Remark", (object)remark ?? DBNull.Value),
                    new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

                // 미수금 내역 등록
                string sqlReceivable = @"
                    INSERT INTO Receivable (CustomerCode, TransDate, TransType, RefNo, Amount, Balance, Remark, CreatedBy, CreatedAt)
                    VALUES (@CustomerCode, @TransDate, N'수금', @RefNo, @Amount, @Balance, @Remark, @CreatedBy, GETDATE())";

                DbHelper.ExecuteNonQuery(sqlReceivable,
                    new SqlParameter("@CustomerCode", selectedCustomerCode),
                    new SqlParameter("@TransDate", collectionDate),
                    new SqlParameter("@RefNo", collectionNo),
                    new SqlParameter("@Amount", -amount),  // 수금은 마이너스
                    new SqlParameter("@Balance", newBalance),
                    new SqlParameter("@Remark", $"{payMethod} {remark}".Trim()),
                    new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

                XtraMessageBox.Show($"수금이 등록되었습니다.\n수금번호: {collectionNo}", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 초기화
                seAmount.Value = 0;
                txtRemark.Text = "";

                // 새로고침
                LoadSummary();
                LoadDetail(selectedCustomerCode);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"수금 등록 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateCollectionNo()
        {
            string prefix = "CO" + DateTime.Today.ToString("yyyyMMdd");
            string sql = "SELECT ISNULL(MAX(CollectionNo), @Prefix + '000') FROM Collection WHERE CollectionNo LIKE @Prefix + '%'";
            string maxNo = DbHelper.ExecuteScalar(sql, new SqlParameter("@Prefix", prefix))?.ToString();

            if (string.IsNullOrEmpty(maxNo) || maxNo == prefix + "000")
                return prefix + "001";

            int seq = int.Parse(maxNo.Substring(maxNo.Length - 3)) + 1;
            return prefix + seq.ToString("000");
        }

        protected override void OnNewClick()
        {
            // 수금 입력 초기화
            seAmount.Value = 0;
            txtRemark.Text = "";
            deCollectionDate.EditValue = DateTime.Today;
            seAmount.Focus();
        }

        protected override void OnSaveClick()
        {
            // 수금 등록 버튼과 동일
            BtnAddCollection_Click(null, null);
        }

        protected override void OnDeleteClick()
        {
            // 선택된 거래 내역 삭제
            if (gridViewDetail.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("삭제할 내역을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string transType = gridViewDetail.GetRowCellValue(gridViewDetail.FocusedRowHandle, "TransType")?.ToString();
            if (transType == "출하")
            {
                XtraMessageBox.Show("출하 내역은 출하 관리에서 삭제하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string refNo = gridViewDetail.GetRowCellValue(gridViewDetail.FocusedRowHandle, "RefNo")?.ToString();
            if (!Confirm($"수금번호 '{refNo}'을(를) 삭제하시겠습니까?")) return;

            try
            {
                // 미수금 내역 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM Receivable WHERE RefNo = @RefNo AND TransType = N'수금'",
                    new SqlParameter("@RefNo", refNo));

                // 수금 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM Collection WHERE CollectionNo = @CollectionNo",
                    new SqlParameter("@CollectionNo", refNo));

                XtraMessageBox.Show("삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadSummary();
                LoadDetail(selectedCustomerCode);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
