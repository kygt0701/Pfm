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
    /// 사용자 관리
    /// </summary>
    public class UserForm : BaseForm
    {
        #region 컨트롤
        private GridControl gridUser;
        private GridView gridViewUser;
        private RepositoryItemComboBox cboRole;
        #endregion

        private DataTable dtUser;

        public UserForm()
        {
            MenuId = "SYS_USER";
            FormTitle = "사용자 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1000, 600);

            // 그리드
            gridUser = new GridControl();
            gridUser.Dock = DockStyle.Fill;

            gridViewUser = new GridView(gridUser);
            gridUser.MainView = gridViewUser;

            SetupGrid();

            pnlContent.Controls.Add(gridUser);
        }

        private void SetupGrid()
        {
            gridViewUser.Columns.Clear();

            // 역할 콤보
            cboRole = new RepositoryItemComboBox();
            cboRole.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            LoadRoles();
            gridUser.RepositoryItems.Add(cboRole);

            AddColumn("UserId", "사용자ID", 100);
            AddColumn("UserName", "사용자명", 120);
            AddColumn("Password", "비밀번호", 100);
            AddColumn("RoleName", "역할", 100, cboRole);
            AddColumn("IsActive", "사용", 50);
            AddColumn("Remark", "비고", 150);

            // 그리드 옵션
            gridViewUser.OptionsBehavior.Editable = true;
            gridViewUser.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            gridViewUser.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewUser.OptionsView.ShowAutoFilterRow = true;
            gridViewUser.OptionsView.ShowGroupPanel = false;
            gridViewUser.RowHeight = 25;

            // 비밀번호 마스킹
            var colPassword = gridViewUser.Columns["Password"];
            var repoPassword = new RepositoryItemTextEdit();
            repoPassword.PasswordChar = '*';
            colPassword.ColumnEdit = repoPassword;
            gridUser.RepositoryItems.Add(repoPassword);

            ThemeManager.ApplyGridViewTheme(gridViewUser);
            SetupGridExcelExport(gridUser, gridViewUser, "사용자목록");
        }

        private void LoadRoles()
        {
            try
            {
                string sql = "SELECT RoleName FROM Role WHERE IsActive = 1 ORDER BY RoleId";
                var dt = DbHelper.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    cboRole.Items.Add(row["RoleName"].ToString());
                }
            }
            catch { }
        }

        private void AddColumn(string fieldName, string caption, int width, RepositoryItemComboBox combo = null)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewUser.Columns.Count;
            if (combo != null)
                col.ColumnEdit = combo;
            gridViewUser.Columns.Add(col);
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
                    SELECT u.UserId, u.UserName, u.Password, 
                           r.RoleName, u.Email, u.Phone, u.IsActive, 
                           u.DeptCode AS Remark
                    FROM Users u
                    LEFT JOIN UserRole ur ON u.UserId = ur.UserId
                    LEFT JOIN Role r ON ur.RoleId = r.RoleId
                    ORDER BY u.UserId";

                dtUser = DbHelper.ExecuteQuery(sql);
                gridUser.DataSource = dtUser;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnNewClick()
        {
            gridViewUser.AddNewRow();
            gridViewUser.SetRowCellValue(GridControl.NewItemRowHandle, "IsActive", true);
            gridViewUser.FocusedColumn = gridViewUser.Columns["UserId"];
        }

        protected override void OnSaveClick()
        {
            try
            {
                gridViewUser.CloseEditor();
                gridViewUser.UpdateCurrentRow();

                int count = 0;
                foreach (DataRow row in dtUser.Rows)
                {
                    if (row.RowState == DataRowState.Added)
                    {
                        string userId = row["UserId"]?.ToString();
                        if (string.IsNullOrEmpty(userId)) continue;

                        // 사용자 INSERT
                        string sql = @"
                            INSERT INTO Users (UserId, UserName, Password, Email, Phone, IsActive, DeptCode, CreatedBy, CreatedAt)
                            VALUES (@UserId, @UserName, @Password, @Email, @Phone, @IsActive, @DeptCode, @CreatedBy, GETDATE())";

                        DbHelper.ExecuteNonQuery(sql,
                            new SqlParameter("@UserId", userId),
                            new SqlParameter("@UserName", row["UserName"] ?? DBNull.Value),
                            new SqlParameter("@Password", row["Password"] ?? "1234"),
                            new SqlParameter("@Email", row["Email"] ?? DBNull.Value),
                            new SqlParameter("@Phone", row["Phone"] ?? DBNull.Value),
                            new SqlParameter("@IsActive", row["IsActive"] ?? true),
                            new SqlParameter("@DeptCode", row["Remark"] ?? DBNull.Value),
                            new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));

                        // 역할 할당
                        string roleName = row["RoleName"]?.ToString();
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            AssignRole(userId, roleName);
                        }

                        count++;
                    }
                    else if (row.RowState == DataRowState.Modified)
                    {
                        string userId = row["UserId"]?.ToString();

                        string sql = @"
                            UPDATE Users SET UserName = @UserName, Password = @Password, 
                                Email = @Email, Phone = @Phone, IsActive = @IsActive, DeptCode = @DeptCode,
                                ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                            WHERE UserId = @UserId";

                        DbHelper.ExecuteNonQuery(sql,
                            new SqlParameter("@UserId", userId),
                            new SqlParameter("@UserName", row["UserName"] ?? DBNull.Value),
                            new SqlParameter("@Password", row["Password"] ?? DBNull.Value),
                            new SqlParameter("@Email", row["Email"] ?? DBNull.Value),
                            new SqlParameter("@Phone", row["Phone"] ?? DBNull.Value),
                            new SqlParameter("@IsActive", row["IsActive"] ?? true),
                            new SqlParameter("@DeptCode", row["Remark"] ?? DBNull.Value),
                            new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));

                        // 역할 재할당
                        string roleName = row["RoleName"]?.ToString();
                        DbHelper.ExecuteNonQuery("DELETE FROM UserRole WHERE UserId = @UserId",
                            new SqlParameter("@UserId", userId));
                        if (!string.IsNullOrEmpty(roleName))
                        {
                            AssignRole(userId, roleName);
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

        private void AssignRole(string userId, string roleName)
        {
            try
            {
                string sqlGetRole = "SELECT RoleId FROM Role WHERE RoleName = @RoleName";
                var roleId = DbHelper.ExecuteScalar(sqlGetRole, new SqlParameter("@RoleName", roleName));

                if (roleId != null)
                {
                    string sqlInsert = @"
                        INSERT INTO UserRole (UserId, RoleId, CreatedBy, CreatedAt)
                        VALUES (@UserId, @RoleId, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sqlInsert,
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@RoleId", roleId),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
            catch { }
        }

        protected override void OnDeleteClick()
        {
            if (gridViewUser.FocusedRowHandle < 0) return;

            var userId = gridViewUser.GetRowCellValue(gridViewUser.FocusedRowHandle, "UserId")?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            if (userId.ToUpper() == "ADMIN")
            {
                XtraMessageBox.Show("관리자 계정은 삭제할 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show($"사용자 '{userId}'를 삭제하시겠습니까?", "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // 역할 삭제
                    DbHelper.ExecuteNonQuery("DELETE FROM UserRole WHERE UserId = @UserId",
                        new SqlParameter("@UserId", userId));

                    // 사용자 삭제
                    DbHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserId = @UserId",
                        new SqlParameter("@UserId", userId));

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
