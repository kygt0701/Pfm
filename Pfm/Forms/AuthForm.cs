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
    /// 권한 관리 (역할별 프로그램 권한)
    /// </summary>
    public class AuthForm : BaseForm
    {
        #region 컨트롤
        private SplitContainerControl splitContainer;

        // 역할 목록
        private GridControl gridRole;
        private GridView gridViewRole;

        // 메뉴 권한
        private PanelControl pnlPermission;
        private LabelControl lblRoleInfo;
        private GridControl gridPermission;
        private GridView gridViewPermission;

        // 체크박스 에디터
        private RepositoryItemCheckEdit repoCheck;
        #endregion

        private DataTable dtRole;
        private DataTable dtPermission;
        private string selectedRoleId;

        public AuthForm()
        {
            MenuId = "SYS_AUTH";
            FormTitle = "권한 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1100, 650);

            // 스플릿 컨테이너 (좌: 역할, 우: 권한)
            splitContainer = new SplitContainerControl();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterPosition = 300;
            splitContainer.FixedPanel = SplitFixedPanel.Panel1;

            // 좌측: 역할 목록
            SetupRoleGrid();
            splitContainer.Panel1.Controls.Add(gridRole);

            // 우측: 메뉴 권한
            pnlPermission = new PanelControl();
            pnlPermission.Dock = DockStyle.Fill;
            pnlPermission.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            lblRoleInfo = new LabelControl();
            lblRoleInfo.Text = "역할을 선택하세요";
            lblRoleInfo.Location = new Point(15, 15);
            lblRoleInfo.Appearance.Font = new Font("맑은 고딕", 11, FontStyle.Bold);
            pnlPermission.Controls.Add(lblRoleInfo);

            SetupPermissionGrid();
            gridPermission.Location = new Point(10, 45);
            gridPermission.Dock = DockStyle.None;
            gridPermission.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlPermission.Controls.Add(gridPermission);

            splitContainer.Panel2.Controls.Add(pnlPermission);

            pnlContent.Controls.Add(splitContainer);

            this.Load += (s, e) => AdjustGridSize();
            this.Resize += (s, e) => AdjustGridSize();
        }

        private void AdjustGridSize()
        {
            gridPermission.Size = new Size(
                pnlPermission.Width - 20,
                pnlPermission.Height - 55);
        }

        private void SetupRoleGrid()
        {
            gridRole = new GridControl();
            gridRole.Dock = DockStyle.Fill;
            gridViewRole = new GridView(gridRole);
            gridRole.MainView = gridViewRole;

            gridViewRole.Columns.Clear();
            AddRoleColumn("RoleId", "역할ID", 80);
            AddRoleColumn("RoleName", "역할명", 100);
            AddRoleColumn("Description", "설명", 150);
            AddRoleColumn("IsActive", "사용", 50);

            gridViewRole.OptionsBehavior.Editable = true;
            gridViewRole.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            gridViewRole.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewRole.OptionsView.ShowGroupPanel = false;
            gridViewRole.FocusedRowChanged += GridViewRole_FocusedRowChanged;

            ThemeManager.ApplyGridViewTheme(gridViewRole);
            SetupGridExcelExport(gridRole, gridViewRole, "역할목록");
        }

        private void AddRoleColumn(string fieldName, string caption, int width)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewRole.Columns.Count;
            gridViewRole.Columns.Add(col);
        }

        private void SetupPermissionGrid()
        {
            gridPermission = new GridControl();
            gridViewPermission = new GridView(gridPermission);
            gridPermission.MainView = gridViewPermission;

            // 체크박스 에디터
            repoCheck = new RepositoryItemCheckEdit();
            repoCheck.ValueChecked = true;
            repoCheck.ValueUnchecked = false;
            gridPermission.RepositoryItems.Add(repoCheck);

            gridViewPermission.Columns.Clear();
            AddPermissionColumn("MenuId", "메뉴ID", 100, false);
            AddPermissionColumn("MenuName", "메뉴명", 150, false);
            AddPermissionColumn("CanRead", "조회", 60, true);
            AddPermissionColumn("CanCreate", "신규", 60, true);
            AddPermissionColumn("CanSave", "저장", 60, true);
            AddPermissionColumn("CanDelete", "삭제", 60, true);

            gridViewPermission.OptionsBehavior.Editable = true;
            gridViewPermission.OptionsView.ShowGroupPanel = false;
            gridViewPermission.RowHeight = 25;

            // 메뉴ID, 메뉴명 읽기전용
            gridViewPermission.Columns["MenuId"].OptionsColumn.AllowEdit = false;
            gridViewPermission.Columns["MenuName"].OptionsColumn.AllowEdit = false;

            // 체크박스 적용
            gridViewPermission.Columns["CanRead"].ColumnEdit = repoCheck;
            gridViewPermission.Columns["CanCreate"].ColumnEdit = repoCheck;
            gridViewPermission.Columns["CanSave"].ColumnEdit = repoCheck;
            gridViewPermission.Columns["CanDelete"].ColumnEdit = repoCheck;

            ThemeManager.ApplyGridViewTheme(gridViewPermission);
            SetupGridExcelExport(gridPermission, gridViewPermission, "권한목록");
        }

        private void AddPermissionColumn(string fieldName, string caption, int width, bool isCheck)
        {
            var col = new GridColumn();
            col.FieldName = fieldName;
            col.Caption = caption;
            col.Width = width;
            col.Visible = true;
            col.VisibleIndex = gridViewPermission.Columns.Count;
            gridViewPermission.Columns.Add(col);
        }

        private void GridViewRole_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                selectedRoleId = gridViewRole.GetRowCellValue(e.FocusedRowHandle, "RoleId")?.ToString();
                string roleName = gridViewRole.GetRowCellValue(e.FocusedRowHandle, "RoleName")?.ToString();

                lblRoleInfo.Text = $"[{selectedRoleId}] {roleName} - 프로그램 권한";
                LoadPermissions(selectedRoleId);
            }
        }

        protected override void OnSearchClick()
        {
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                string sql = "SELECT RoleId, RoleName, Description, IsActive FROM Role ORDER BY RoleId";
                dtRole = DbHelper.ExecuteQuery(sql);
                gridRole.DataSource = dtRole;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPermissions(string roleId)
        {
            try
            {
                // 모든 메뉴와 해당 역할의 권한을 조인
                string sql = @"
                    SELECT m.MenuId, m.MenuName,
                        ISNULL(p.CanRead, 0) AS CanRead,
                        ISNULL(p.CanCreate, 0) AS CanCreate,
                        ISNULL(p.CanSave, 0) AS CanSave,
                        ISNULL(p.CanDelete, 0) AS CanDelete
                    FROM Menu m
                    LEFT JOIN MenuPermission p ON m.MenuId = p.MenuId AND p.RoleId = @RoleId
                    WHERE m.FormName IS NOT NULL AND m.IsActive = 1
                    ORDER BY m.ParentMenuId, m.MenuOrder";

                dtPermission = DbHelper.ExecuteQuery(sql, new SqlParameter("@RoleId", roleId));
                gridPermission.DataSource = dtPermission;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"권한 조회 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnNewClick()
        {
            // 새 역할 추가
            gridViewRole.AddNewRow();
            gridViewRole.SetRowCellValue(GridControl.NewItemRowHandle, "IsActive", true);
            gridViewRole.FocusedColumn = gridViewRole.Columns["RoleId"];
        }

        protected override void OnSaveClick()
        {
            try
            {
                // 역할 저장
                SaveRoles();

                // 권한 저장
                if (!string.IsNullOrEmpty(selectedRoleId))
                {
                    SavePermissions();
                }

                XtraMessageBox.Show("저장되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRoles();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"저장 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveRoles()
        {
            gridViewRole.CloseEditor();
            gridViewRole.UpdateCurrentRow();

            foreach (DataRow row in dtRole.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string roleId = row["RoleId"]?.ToString();
                    if (string.IsNullOrEmpty(roleId)) continue;

                    string sql = @"
                        INSERT INTO Role (RoleId, RoleName, Description, IsActive, CreatedBy, CreatedAt)
                        VALUES (@RoleId, @RoleName, @Description, @IsActive, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@RoleId", roleId),
                        new SqlParameter("@RoleName", row["RoleName"] ?? DBNull.Value),
                        new SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    string sql = @"
                        UPDATE Role SET RoleName = @RoleName, Description = @Description, 
                            IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE RoleId = @RoleId";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@RoleId", row["RoleId"]),
                        new SqlParameter("@RoleName", row["RoleName"] ?? DBNull.Value),
                        new SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
        }

        private void SavePermissions()
        {
            gridViewPermission.CloseEditor();
            gridViewPermission.UpdateCurrentRow();

            // 기존 권한 삭제
            DbHelper.ExecuteNonQuery("DELETE FROM MenuPermission WHERE RoleId = @RoleId",
                new SqlParameter("@RoleId", selectedRoleId));

            // 새 권한 입력
            foreach (DataRow row in dtPermission.Rows)
            {
                string menuId = row["MenuId"]?.ToString();
                bool canRead = Convert.ToBoolean(row["CanRead"] ?? false);
                bool canCreate = Convert.ToBoolean(row["CanCreate"] ?? false);
                bool canSave = Convert.ToBoolean(row["CanSave"] ?? false);
                bool canDelete = Convert.ToBoolean(row["CanDelete"] ?? false);

                // 하나라도 권한이 있으면 저장
                if (canRead || canCreate || canSave || canDelete)
                {
                    string sql = @"
                        INSERT INTO MenuPermission (RoleId, MenuId, CanRead, CanCreate, CanSave, CanDelete, CreatedBy, CreatedAt)
                        VALUES (@RoleId, @MenuId, @CanRead, @CanCreate, @CanSave, @CanDelete, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@RoleId", selectedRoleId),
                        new SqlParameter("@MenuId", menuId),
                        new SqlParameter("@CanRead", canRead),
                        new SqlParameter("@CanCreate", canCreate),
                        new SqlParameter("@CanSave", canSave),
                        new SqlParameter("@CanDelete", canDelete),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
        }

        protected override void OnDeleteClick()
        {
            if (gridViewRole.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("삭제할 역할을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roleId = gridViewRole.GetRowCellValue(gridViewRole.FocusedRowHandle, "RoleId")?.ToString();
            string roleName = gridViewRole.GetRowCellValue(gridViewRole.FocusedRowHandle, "RoleName")?.ToString();

            if (roleId == "ADMIN")
            {
                XtraMessageBox.Show("관리자 역할은 삭제할 수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Confirm($"역할 '{roleName}'을(를) 삭제하시겠습니까?\n관련 권한도 함께 삭제됩니다.")) return;

            try
            {
                // 권한 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM MenuPermission WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", roleId));

                // 사용자 역할 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM UserRole WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", roleId));

                // 역할 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM Role WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", roleId));

                XtraMessageBox.Show("삭제되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRoles();

                dtPermission?.Clear();
                gridPermission.DataSource = null;
                lblRoleInfo.Text = "역할을 선택하세요";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
