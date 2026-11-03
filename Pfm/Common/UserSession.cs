using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Pfm.Common
{
    /// <summary>
    /// 현재 로그인한 사용자 세션 정보
    /// </summary>
    public static class UserSession
    {
        public static string UserId { get; private set; }
        public static string UserName { get; private set; }
        public static bool IsLoggedIn { get; private set; }
        public static List<string> Roles { get; private set; } = new List<string>();

        private static Dictionary<string, MenuPermission> _permissions = new Dictionary<string, MenuPermission>();

        /// <summary>
        /// 로그인 처리
        /// </summary>
        public static bool Login(string userId, string password)
        {
            try
            {
                string sql = @"SELECT UserId, UserName FROM Users 
                              WHERE UserId = @UserId AND Password = @Password AND IsActive = 1";
                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Password", password)
                };

                DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                if (dt.Rows.Count > 0)
                {
                    UserId = dt.Rows[0]["UserId"].ToString();
                    UserName = dt.Rows[0]["UserName"].ToString();
                    IsLoggedIn = true;

                    LoadRoles();
                    LoadPermissions();
                    return true;
                }
            }
            catch
            {
                // DB 연결 실패 시 기본 계정 허용
                if (userId == "admin" && password == "1234")
                {
                    UserId = "admin";
                    UserName = "관리자";
                    IsLoggedIn = true;
                    Roles.Add("ADMIN");
                    SetDefaultAdminPermissions();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 로그아웃 처리
        /// </summary>
        public static void Logout()
        {
            UserId = null;
            UserName = null;
            IsLoggedIn = false;
            Roles.Clear();
            _permissions.Clear();
        }

        /// <summary>
        /// 사용자 역할 로드
        /// </summary>
        private static void LoadRoles()
        {
            Roles.Clear();
            try
            {
                string sql = "SELECT RoleId FROM UserRole WHERE UserId = @UserId";
                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@UserId", UserId));
                foreach (DataRow row in dt.Rows)
                {
                    Roles.Add(row["RoleId"].ToString());
                }
            }
            catch { }
        }

        /// <summary>
        /// 메뉴 권한 로드
        /// </summary>
        private static void LoadPermissions()
        {
            _permissions.Clear();
            try
            {
                string sql = @"
                    SELECT mp.MenuId, 
                           MAX(CAST(mp.CanRead AS INT)) AS CanRead,
                           MAX(CAST(mp.CanCreate AS INT)) AS CanCreate,
                           MAX(CAST(mp.CanSave AS INT)) AS CanSave,
                           MAX(CAST(mp.CanDelete AS INT)) AS CanDelete
                    FROM MenuPermission mp
                    INNER JOIN UserRole ur ON mp.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId
                    GROUP BY mp.MenuId";

                DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@UserId", UserId));
                foreach (DataRow row in dt.Rows)
                {
                    string menuId = row["MenuId"].ToString();
                    _permissions[menuId] = new MenuPermission
                    {
                        MenuId = menuId,
                        CanRead = Convert.ToBoolean(row["CanRead"]),
                        CanCreate = Convert.ToBoolean(row["CanCreate"]),
                        CanSave = Convert.ToBoolean(row["CanSave"]),
                        CanDelete = Convert.ToBoolean(row["CanDelete"])
                    };
                }
            }
            catch { }

            // DB에 권한 데이터가 없으면 기본 권한 설정
            if (_permissions.Count == 0)
            {
                SetDefaultAdminPermissions();
            }
        }

        /// <summary>
        /// 기본 관리자 권한 설정 (DB 연결 실패 시)
        /// </summary>
        private static void SetDefaultAdminPermissions()
        {
            string[] menuIds = { "SYS_LOGIN", "SYS_USER", "SYS_AUTH", "MST_CODE", "MST_CUST", "MST_ITEM", "WRK_ORD", "WRK_INV", "WRK_SHP", "WRK_RECV" };
            foreach (var menuId in menuIds)
            {
                _permissions[menuId] = new MenuPermission
                {
                    MenuId = menuId,
                    CanRead = true,
                    CanCreate = true,
                    CanSave = true,
                    CanDelete = true
                };
            }
        }

        /// <summary>
        /// 메뉴 권한 조회
        /// </summary>
        public static MenuPermission GetPermission(string menuId)
        {
            if (_permissions.TryGetValue(menuId, out MenuPermission permission))
            {
                return permission;
            }
            return new MenuPermission { MenuId = menuId };
        }

        /// <summary>
        /// 권한 확인
        /// </summary>
        public static bool HasPermission(string menuId, PermissionType type)
        {
            var perm = GetPermission(menuId);
            switch (type)
            {
                case PermissionType.Read: return perm.CanRead;
                case PermissionType.Create: return perm.CanCreate;
                case PermissionType.Save: return perm.CanSave;
                case PermissionType.Delete: return perm.CanDelete;
                default: return false;
            }
        }
    }

    /// <summary>
    /// 메뉴 권한 정보
    /// </summary>
    public class MenuPermission
    {
        public string MenuId { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanSave { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// 권한 유형
    /// </summary>
    public enum PermissionType
    {
        Read,
        Create,
        Save,
        Delete
    }
}
