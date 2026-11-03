using System;
using System.Data;
using System.Data.SqlClient;
using Pfm.Common;
using Pfm.Services;
using Pfm.ViewModels.Base;

namespace Pfm.ViewModels
{
    /// <summary>
    /// 권한 관리 ViewModel
    /// </summary>
    public class AuthViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _roleList;
        private DataTable _permissionList;
        private string _selectedRoleId;
        private string _selectedRoleName;
        #endregion

        #region 속성
        public DataTable RoleList
        {
            get => _roleList;
            set => SetProperty(ref _roleList, value);
        }

        public DataTable PermissionList
        {
            get => _permissionList;
            set => SetProperty(ref _permissionList, value);
        }

        public string SelectedRoleId
        {
            get => _selectedRoleId;
            set
            {
                if (SetProperty(ref _selectedRoleId, value))
                {
                    LoadPermissions();
                }
            }
        }

        public string SelectedRoleName
        {
            get => _selectedRoleName;
            set => SetProperty(ref _selectedRoleName, value);
        }

        public string RoleInfoText => string.IsNullOrEmpty(SelectedRoleId) 
            ? "역할을 선택하세요" 
            : $"[{SelectedRoleId}] {SelectedRoleName} - 프로그램 권한";
        #endregion

        public AuthViewModel()
        {
            MenuId = "SYS_AUTH";
            FormTitle = "권한 관리";
        }

        protected override void OnNew()
        {
            // 새 역할 추가는 그리드에서 직접 처리
            StatusMessage = "신규 역할 입력";
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = "SELECT RoleId, RoleName, Description, IsActive FROM Role ORDER BY RoleId";
                RoleList = DbHelper.ExecuteQuery(sql);

                EndWork($"조회 완료: {RoleList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        private void LoadPermissions()
        {
            if (string.IsNullOrEmpty(SelectedRoleId))
            {
                PermissionList = null;
                OnPropertyChanged(nameof(RoleInfoText));
                return;
            }

            try
            {
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

                PermissionList = DbHelper.ExecuteQuery(sql, new SqlParameter("@RoleId", SelectedRoleId));
                OnPropertyChanged(nameof(RoleInfoText));
            }
            catch (Exception ex)
            {
                ServiceLocator.MessageService.ShowError($"권한 조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            try
            {
                BeginWork("저장 중...");

                SaveRoles();

                if (!string.IsNullOrEmpty(SelectedRoleId))
                {
                    SavePermissions();
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

        private void SaveRoles()
        {
            if (RoleList == null) return;

            foreach (DataRow row in RoleList.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string roleId = row["RoleId"]?.ToString();
                    if (string.IsNullOrEmpty(roleId)) continue;

                    string sql = @"INSERT INTO Role (RoleId, RoleName, Description, IsActive, CreatedBy, CreatedAt)
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
                    string sql = @"UPDATE Role SET RoleName = @RoleName, Description = @Description, 
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
            if (PermissionList == null) return;

            // 기존 권한 삭제
            DbHelper.ExecuteNonQuery("DELETE FROM MenuPermission WHERE RoleId = @RoleId",
                new SqlParameter("@RoleId", SelectedRoleId));

            // 새 권한 입력
            foreach (DataRow row in PermissionList.Rows)
            {
                string menuId = row["MenuId"]?.ToString();
                bool canRead = Convert.ToBoolean(row["CanRead"] ?? false);
                bool canCreate = Convert.ToBoolean(row["CanCreate"] ?? false);
                bool canSave = Convert.ToBoolean(row["CanSave"] ?? false);
                bool canDelete = Convert.ToBoolean(row["CanDelete"] ?? false);

                if (canRead || canCreate || canSave || canDelete)
                {
                    string sql = @"INSERT INTO MenuPermission (RoleId, MenuId, CanRead, CanCreate, CanSave, CanDelete, CreatedBy, CreatedAt)
                                   VALUES (@RoleId, @MenuId, @CanRead, @CanCreate, @CanSave, @CanDelete, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@RoleId", SelectedRoleId),
                        new SqlParameter("@MenuId", menuId),
                        new SqlParameter("@CanRead", canRead),
                        new SqlParameter("@CanCreate", canCreate),
                        new SqlParameter("@CanSave", canSave),
                        new SqlParameter("@CanDelete", canDelete),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
        }

        protected override void OnDelete()
        {
            if (string.IsNullOrEmpty(SelectedRoleId))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 역할을 선택하세요.");
                return;
            }

            if (SelectedRoleId == "ADMIN")
            {
                ServiceLocator.MessageService.ShowWarning("관리자 역할은 삭제할 수 없습니다.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"역할 '{SelectedRoleName}'을(를) 삭제하시겠습니까?\n관련 권한도 함께 삭제됩니다."))
                return;

            try
            {
                BeginWork("삭제 중...");

                // 권한 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM MenuPermission WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", SelectedRoleId));

                // 사용자 역할 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM UserRole WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", SelectedRoleId));

                // 역할 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM Role WHERE RoleId = @RoleId",
                    new SqlParameter("@RoleId", SelectedRoleId));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                SelectedRoleId = null;
                PermissionList = null;
                OnSearch();
            }
            catch (Exception ex)
            {
                EndWork("삭제 실패");
                ServiceLocator.MessageService.ShowError($"삭제 오류: {ex.Message}");
            }
        }
    }
}
