using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Pfm.Common;

namespace Pfm.Forms
{
    /// <summary>
    /// 거래처 관리 폼
    /// </summary>
    public class CustomerForm : BaseForm
    {
        private GridControl gridCustomer;
        private GridView gridViewCustomer;
        private PanelControl pnlSearch;
        private TextEdit txtSearchCode;
        private TextEdit txtSearchName;
        private LabelControl lblSearchCode;
        private LabelControl lblSearchName;
        private RepositoryItemLookUpEdit repoTaxType;
        private RepositoryItemLookUpEdit repoBankName;

        public override string MenuId => "MST_CUST";
        public override string FormTitle => "거래처 관리";

        public CustomerForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(900, 600);

            // 검색 패널
            pnlSearch = new PanelControl();
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Height = 45;
            pnlSearch.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            lblSearchCode = new LabelControl();
            lblSearchCode.Text = "거래처코드";
            lblSearchCode.Location = new Point(12, 14);

            txtSearchCode = new TextEdit();
            txtSearchCode.Location = new Point(80, 11);
            txtSearchCode.Size = new Size(120, 22);

            lblSearchName = new LabelControl();
            lblSearchName.Text = "거래처명";
            lblSearchName.Location = new Point(220, 14);

            txtSearchName = new TextEdit();
            txtSearchName.Location = new Point(280, 11);
            txtSearchName.Size = new Size(150, 22);

            pnlSearch.Controls.AddRange(new Control[] { lblSearchCode, txtSearchCode, lblSearchName, txtSearchName });

            // 그리드
            gridCustomer = new GridControl();
            gridCustomer.Dock = DockStyle.Fill;
            gridViewCustomer = new GridView(gridCustomer);
            gridCustomer.MainView = gridViewCustomer;

            // 그리드 컬럼 설정
            AddColumn("CustomerCode", "거래처코드", 90);
            AddColumn("CustomerName", "거래처명", 150);
            AddColumn("CustomerType", "유형", 70);
            AddColumn("BizNo", "사업자번호", 110);
            AddColumn("BizType", "업태", 80);
            AddColumn("BizItem", "종목", 100);
            AddColumn("Representative", "대표자", 80);
            AddColumn("ContactName", "담당자", 80);
            AddColumn("ContactPhone", "담당자연락처", 110);
            AddColumn("Phone", "전화번호", 110);
            AddColumn("Fax", "팩스", 110);
            AddColumn("ZipCode", "우편번호", 70);
            AddColumn("Address", "주소", 200);
            AddColumn("Address2", "상세주소", 150);
            AddColumn("Email", "이메일", 150);
            AddColumn("TaxType", "과세유형", 70);
            AddColumn("PaymentTerms", "결제조건", 80);
            AddColumn("CreditLimit", "여신한도", 100, true);
            AddColumn("BankName", "은행명", 80);
            AddColumn("AccountNo", "계좌번호", 120);
            AddColumn("AccountHolder", "예금주", 80);
            AddColumn("StartDate", "거래시작일", 90);
            AddColumn("IsActive", "사용", 50);
            AddColumn("Remark", "비고", 150);
            AddColumn("CreatedBy", "등록자", 80);
            AddColumn("CreatedAt", "등록일시", 130);
            AddColumn("ModifiedBy", "수정자", 80);
            AddColumn("ModifiedAt", "수정일시", 130);

            gridViewCustomer.OptionsView.ShowGroupPanel = false;
            gridViewCustomer.OptionsBehavior.Editable = true;
            gridViewCustomer.OptionsView.ColumnAutoWidth = false;
            gridViewCustomer.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
            gridViewCustomer.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

            // 테마 적용
            ThemeManager.ApplyGridViewTheme(gridViewCustomer);
            SetupGridExcelExport(gridCustomer, gridViewCustomer, "거래처목록");

            // 사업자번호, 전화번호 포맷 마스크 적용
            ApplyPhoneMask("BizNo", "000-00-00000");      // 사업자번호: 123-45-67890
            ApplyPhoneMask("Phone", "000-0000-0000");     // 전화번호: 010-1234-5678
            ApplyPhoneMask("ContactPhone", "000-0000-0000"); // 담당자연락처
            ApplyPhoneMask("Fax", "000-0000-0000");       // 팩스

            // 과세유형 콤보박스
            repoTaxType = CreateLookUpEdit("TAX_TYPE");
            gridViewCustomer.Columns["TaxType"].ColumnEdit = repoTaxType;

            // 은행명 콤보박스
            repoBankName = CreateLookUpEdit("BANK");
            gridViewCustomer.Columns["BankName"].ColumnEdit = repoBankName;

            // 등록자/수정자 컬럼 읽기전용
            gridViewCustomer.Columns["CreatedBy"].OptionsColumn.AllowEdit = false;
            gridViewCustomer.Columns["CreatedAt"].OptionsColumn.AllowEdit = false;
            gridViewCustomer.Columns["ModifiedBy"].OptionsColumn.AllowEdit = false;
            gridViewCustomer.Columns["ModifiedAt"].OptionsColumn.AllowEdit = false;

            // 거래처코드, 거래처명 컬럼 고정
            gridViewCustomer.Columns["CustomerCode"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridViewCustomer.Columns["CustomerName"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            // 컨텐츠 패널에 추가
            pnlContent.Controls.Add(gridCustomer);
            pnlContent.Controls.Add(pnlSearch);
        }

        private void ApplyPhoneMask(string fieldName, string mask)
        {
            var col = gridViewCustomer.Columns[fieldName];
            if (col != null)
            {
                var repoMask = new RepositoryItemTextEdit();
                repoMask.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
                repoMask.Mask.EditMask = mask;
                repoMask.Mask.UseMaskAsDisplayFormat = true;
                col.ColumnEdit = repoMask;
                gridCustomer.RepositoryItems.Add(repoMask);
            }
        }

        private RepositoryItemLookUpEdit CreateLookUpEdit(string groupCode)
        {
            var repo = new RepositoryItemLookUpEdit();
            repo.NullText = "";
            repo.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;

            try
            {
                string sql = "SELECT Code, CodeName FROM CommonCode WHERE GroupCode = @GroupCode AND IsActive = 1 ORDER BY SortOrder";
                var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@GroupCode", groupCode));
                repo.DataSource = dt;
                repo.ValueMember = "CodeName";
                repo.DisplayMember = "CodeName";
                repo.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Code", "코드", 50));
                repo.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("CodeName", "명칭", 100));
            }
            catch { }

            gridCustomer.RepositoryItems.Add(repo);
            return repo;
        }

        private void AddColumn(string fieldName, string caption, int width, bool isNumeric = false)
        {
            var col = gridViewCustomer.Columns.AddField(fieldName);
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            if (isNumeric)
            {
                col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                col.DisplayFormat.FormatString = "#,##0";
            }
        }

        protected override void OnNewClick()
        {
            if (!CheckPermission(PermissionType.Create)) return;

            // 새 행 추가
            gridViewCustomer.AddNewRow();
            gridViewCustomer.SetFocusedRowCellValue("IsActive", true);
            gridViewCustomer.SetFocusedRowCellValue("CreatedBy", UserSession.UserId);
            gridViewCustomer.SetFocusedRowCellValue("CreatedAt", DateTime.Now);
        }

        protected override void OnSearchClick()
        {
            if (!CheckPermission(PermissionType.Read)) return;

            try
            {
                string sql = @"SELECT CustomerCode, CustomerName, CustomerType, BizNo, 
                                      BizType, BizItem, Representative, ContactName, ContactPhone,
                                      Phone, Fax, ZipCode, Address, Address2, Email,
                                      TaxType, PaymentTerms, CreditLimit, BankName, AccountNo, AccountHolder,
                                      StartDate, IsActive, Remark,
                                      CreatedBy, CreatedAt, ModifiedBy, ModifiedAt
                               FROM Customer WHERE 1=1";

                var parameters = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrEmpty(txtSearchCode.Text))
                {
                    sql += " AND CustomerCode LIKE @Code";
                    parameters.Add(new SqlParameter("@Code", "%" + txtSearchCode.Text + "%"));
                }

                if (!string.IsNullOrEmpty(txtSearchName.Text))
                {
                    sql += " AND CustomerName LIKE @Name";
                    parameters.Add(new SqlParameter("@Name", "%" + txtSearchName.Text + "%"));
                }

                sql += " ORDER BY CustomerCode";

                DataTable dt = DbHelper.ExecuteQuery(sql, parameters.ToArray());
                gridCustomer.DataSource = dt;

                ShowInfo($"{dt.Rows.Count}건이 조회되었습니다.");
            }
            catch (Exception ex)
            {
                ShowError("조회 중 오류가 발생했습니다.\n" + ex.Message);
            }
        }

        protected override void OnSaveClick()
        {
            if (!CheckPermission(PermissionType.Save)) return;

            try
            {
                gridViewCustomer.CloseEditor();
                gridViewCustomer.UpdateCurrentRow();

                if (!(gridCustomer.DataSource is DataTable dt)) return;

                int savedCount = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row.RowState == DataRowState.Added)
                    {
                        InsertCustomer(row);
                        savedCount++;
                    }
                    else if (row.RowState == DataRowState.Modified)
                    {
                        UpdateCustomer(row);
                        savedCount++;
                    }
                }

                if (savedCount > 0)
                {
                    dt.AcceptChanges();
                    ShowInfo($"{savedCount}건이 저장되었습니다.");
                }
                else
                {
                    ShowInfo("변경된 데이터가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                ShowError("저장 중 오류가 발생했습니다.\n" + ex.Message);
            }
        }

        protected override void OnDeleteClick()
        {
            if (!CheckPermission(PermissionType.Delete)) return;

            if (gridViewCustomer.FocusedRowHandle < 0)
            {
                ShowWarning("삭제할 항목을 선택하세요.");
                return;
            }

            string code = gridViewCustomer.GetFocusedRowCellValue("CustomerCode")?.ToString();
            if (string.IsNullOrEmpty(code))
            {
                ShowWarning("거래처코드가 없습니다.");
                return;
            }

            if (!Confirm($"거래처 '{code}'을(를) 삭제하시겠습니까?")) return;

            try
            {
                string sql = "DELETE FROM Customer WHERE CustomerCode = @Code";
                DbHelper.ExecuteNonQuery(sql, new SqlParameter("@Code", code));

                gridViewCustomer.DeleteRow(gridViewCustomer.FocusedRowHandle);
                ShowInfo("삭제되었습니다.");
            }
            catch (Exception ex)
            {
                ShowError("삭제 중 오류가 발생했습니다.\n" + ex.Message);
            }
        }

        private void InsertCustomer(DataRow row)
        {
            string sql = @"INSERT INTO Customer 
                          (CustomerCode, CustomerName, CustomerType, BizNo, BizType, BizItem,
                           Representative, ContactName, ContactPhone, Phone, Fax, 
                           ZipCode, Address, Address2, Email, TaxType, PaymentTerms, CreditLimit,
                           BankName, AccountNo, AccountHolder, StartDate, IsActive, Remark,
                           CreatedBy, CreatedAt)
                          VALUES (@Code, @Name, @Type, @BizNo, @BizType, @BizItem,
                           @Rep, @ContactName, @ContactPhone, @Phone, @Fax,
                           @ZipCode, @Addr, @Addr2, @Email, @TaxType, @PaymentTerms, @CreditLimit,
                           @BankName, @AccountNo, @AccountHolder, @StartDate, @Active, @Remark,
                           @CreatedBy, GETDATE())";

            DbHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Code", row["CustomerCode"] ?? DBNull.Value),
                new SqlParameter("@Name", row["CustomerName"] ?? DBNull.Value),
                new SqlParameter("@Type", row["CustomerType"] ?? DBNull.Value),
                new SqlParameter("@BizNo", row["BizNo"] ?? DBNull.Value),
                new SqlParameter("@BizType", row["BizType"] ?? DBNull.Value),
                new SqlParameter("@BizItem", row["BizItem"] ?? DBNull.Value),
                new SqlParameter("@Rep", row["Representative"] ?? DBNull.Value),
                new SqlParameter("@ContactName", row["ContactName"] ?? DBNull.Value),
                new SqlParameter("@ContactPhone", row["ContactPhone"] ?? DBNull.Value),
                new SqlParameter("@Phone", row["Phone"] ?? DBNull.Value),
                new SqlParameter("@Fax", row["Fax"] ?? DBNull.Value),
                new SqlParameter("@ZipCode", row["ZipCode"] ?? DBNull.Value),
                new SqlParameter("@Addr", row["Address"] ?? DBNull.Value),
                new SqlParameter("@Addr2", row["Address2"] ?? DBNull.Value),
                new SqlParameter("@Email", row["Email"] ?? DBNull.Value),
                new SqlParameter("@TaxType", row["TaxType"] ?? DBNull.Value),
                new SqlParameter("@PaymentTerms", row["PaymentTerms"] ?? DBNull.Value),
                new SqlParameter("@CreditLimit", row["CreditLimit"] ?? DBNull.Value),
                new SqlParameter("@BankName", row["BankName"] ?? DBNull.Value),
                new SqlParameter("@AccountNo", row["AccountNo"] ?? DBNull.Value),
                new SqlParameter("@AccountHolder", row["AccountHolder"] ?? DBNull.Value),
                new SqlParameter("@StartDate", row["StartDate"] ?? DBNull.Value),
                new SqlParameter("@Active", row["IsActive"] ?? true),
                new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
        }

        private void UpdateCustomer(DataRow row)
        {
            string sql = @"UPDATE Customer SET 
                          CustomerName = @Name, CustomerType = @Type, BizNo = @BizNo, 
                          BizType = @BizType, BizItem = @BizItem, Representative = @Rep,
                          ContactName = @ContactName, ContactPhone = @ContactPhone,
                          Phone = @Phone, Fax = @Fax, ZipCode = @ZipCode,
                          Address = @Addr, Address2 = @Addr2, Email = @Email,
                          TaxType = @TaxType, PaymentTerms = @PaymentTerms, CreditLimit = @CreditLimit,
                          BankName = @BankName, AccountNo = @AccountNo, AccountHolder = @AccountHolder,
                          StartDate = @StartDate, IsActive = @Active, Remark = @Remark,
                          ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                          WHERE CustomerCode = @Code";

            DbHelper.ExecuteNonQuery(sql,
                new SqlParameter("@Code", row["CustomerCode"]),
                new SqlParameter("@Name", row["CustomerName"] ?? DBNull.Value),
                new SqlParameter("@Type", row["CustomerType"] ?? DBNull.Value),
                new SqlParameter("@BizNo", row["BizNo"] ?? DBNull.Value),
                new SqlParameter("@BizType", row["BizType"] ?? DBNull.Value),
                new SqlParameter("@BizItem", row["BizItem"] ?? DBNull.Value),
                new SqlParameter("@Rep", row["Representative"] ?? DBNull.Value),
                new SqlParameter("@ContactName", row["ContactName"] ?? DBNull.Value),
                new SqlParameter("@ContactPhone", row["ContactPhone"] ?? DBNull.Value),
                new SqlParameter("@Phone", row["Phone"] ?? DBNull.Value),
                new SqlParameter("@Fax", row["Fax"] ?? DBNull.Value),
                new SqlParameter("@ZipCode", row["ZipCode"] ?? DBNull.Value),
                new SqlParameter("@Addr", row["Address"] ?? DBNull.Value),
                new SqlParameter("@Addr2", row["Address2"] ?? DBNull.Value),
                new SqlParameter("@Email", row["Email"] ?? DBNull.Value),
                new SqlParameter("@TaxType", row["TaxType"] ?? DBNull.Value),
                new SqlParameter("@PaymentTerms", row["PaymentTerms"] ?? DBNull.Value),
                new SqlParameter("@CreditLimit", row["CreditLimit"] ?? DBNull.Value),
                new SqlParameter("@BankName", row["BankName"] ?? DBNull.Value),
                new SqlParameter("@AccountNo", row["AccountNo"] ?? DBNull.Value),
                new SqlParameter("@AccountHolder", row["AccountHolder"] ?? DBNull.Value),
                new SqlParameter("@StartDate", row["StartDate"] ?? DBNull.Value),
                new SqlParameter("@Active", row["IsActive"] ?? true),
                new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
        }
    }
}
