using System;
using System.Data;
using System.Data.SqlClient;
using Pfm.Common;
using Pfm.Models;
using Pfm.Services;
using Pfm.ViewModels.Base;

namespace Pfm.ViewModels
{
    /// <summary>
    /// 사용자 관리 ViewModel
    /// </summary>
    public class UserViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _userList;
        private DataTable _roleList;
        private UserModel _selectedUser;
        private string _searchKeyword;
        #endregion

        #region 속성
        public DataTable UserList
        {
            get => _userList;
            set => SetProperty(ref _userList, value);
        }

        public DataTable RoleList
        {
            get => _roleList;
            set => SetProperty(ref _roleList, value);
        }

        public UserModel SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }
        #endregion

        public UserViewModel()
        {
            MenuId = "SYS_USER";
            FormTitle = "사용자 관리";
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                string sql = "SELECT RoleId, RoleName FROM Role WHERE IsActive = 1 ORDER BY RoleName";
                RoleList = DbHelper.ExecuteQuery(sql);
            }
            catch { }
        }

        protected override void OnNew()
        {
            SelectedUser = new UserModel
            {
                IsActive = true
            };
            StatusMessage = "신규 사용자 입력";
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = @"SELECT UserId, UserName, Email, Tel, Department, IsActive
                               FROM Users 
                               WHERE (@Keyword IS NULL OR UserName LIKE '%' + @Keyword + '%' 
                                      OR UserId LIKE '%' + @Keyword + '%')
                               ORDER BY UserName";

                UserList = DbHelper.ExecuteQuery(sql,
                    new SqlParameter("@Keyword", string.IsNullOrEmpty(SearchKeyword) ? (object)DBNull.Value : SearchKeyword));

                EndWork($"조회 완료: {UserList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            if (SelectedUser == null)
            {
                ServiceLocator.MessageService.ShowWarning("저장할 사용자를 선택하세요.");
                return;
            }

            if (string.IsNullOrEmpty(SelectedUser.UserId))
            {
                ServiceLocator.MessageService.ShowWarning("사용자 ID를 입력하세요.");
                return;
            }

            if (string.IsNullOrEmpty(SelectedUser.UserName))
            {
                ServiceLocator.MessageService.ShowWarning("사용자명을 입력하세요.");
                return;
            }

            try
            {
                BeginWork("저장 중...");

                string checkSql = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId";
                var dt = DbHelper.ExecuteQuery(checkSql, new SqlParameter("@UserId", SelectedUser.UserId));
                bool exists = Convert.ToInt32(dt.Rows[0][0]) > 0;

                if (exists)
                {
                    string sql = @"UPDATE Users SET 
                                    UserName = @UserName, Email = @Email, Tel = @Tel,
                                    Department = @Department, IsActive = @IsActive,
                                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE UserId = @UserId";

                    // 비밀번호가 입력된 경우에만 업데이트
                    if (!string.IsNullOrEmpty(SelectedUser.Password))
                    {
                        sql = @"UPDATE Users SET 
                                    UserName = @UserName, Password = @Password, Email = @Email, Tel = @Tel,
                                    Department = @Department, IsActive = @IsActive,
                                    ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE UserId = @UserId";
                    }

                    DbHelper.ExecuteNonQuery(sql, GetSaveParameters(false));
                }
                else
                {
                    if (string.IsNullOrEmpty(SelectedUser.Password))
                    {
                        ServiceLocator.MessageService.ShowWarning("신규 사용자는 비밀번호를 입력해야 합니다.");
                        EndWork("");
                        return;
                    }

                    string sql = @"INSERT INTO Users 
                                    (UserId, UserName, Password, Email, Tel, Department, IsActive, CreatedBy, CreatedAt)
                                   VALUES 
                                    (@UserId, @UserName, @Password, @Email, @Tel, @Department, @IsActive, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql, GetSaveParameters(true));
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

        protected override void OnDelete()
        {
            if (SelectedUser == null || string.IsNullOrEmpty(SelectedUser.UserId))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 사용자를 선택하세요.");
                return;
            }

            if (SelectedUser.UserId.ToLower() == "admin")
            {
                ServiceLocator.MessageService.ShowWarning("관리자 계정은 삭제할 수 없습니다.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"'{SelectedUser.UserName}' 사용자를 삭제하시겠습니까?"))
                return;

            try
            {
                BeginWork("삭제 중...");

                // 사용자 역할 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM UserRole WHERE UserId = @UserId",
                    new SqlParameter("@UserId", SelectedUser.UserId));

                // 사용자 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM Users WHERE UserId = @UserId",
                    new SqlParameter("@UserId", SelectedUser.UserId));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                SelectedUser = null;
                OnSearch();
            }
            catch (Exception ex)
            {
                EndWork("삭제 실패");
                ServiceLocator.MessageService.ShowError($"삭제 오류: {ex.Message}");
            }
        }

        private SqlParameter[] GetSaveParameters(bool isInsert)
        {
            var u = SelectedUser;
            var paramList = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@UserId", u.UserId ?? (object)DBNull.Value),
                new SqlParameter("@UserName", u.UserName ?? (object)DBNull.Value),
                new SqlParameter("@Email", u.Email ?? (object)DBNull.Value),
                new SqlParameter("@Tel", u.Tel ?? (object)DBNull.Value),
                new SqlParameter("@Department", u.Department ?? (object)DBNull.Value),
                new SqlParameter("@IsActive", u.IsActive),
                new SqlParameter(isInsert ? "@CreatedBy" : "@ModifiedBy", UserSession.UserId ?? "SYSTEM")
            };

            if (!string.IsNullOrEmpty(u.Password))
            {
                paramList.Add(new SqlParameter("@Password", u.Password));
            }

            return paramList.ToArray();
        }
    }
}
