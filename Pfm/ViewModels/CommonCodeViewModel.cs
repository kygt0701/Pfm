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
    /// 공통코드 관리 ViewModel
    /// </summary>
    public class CommonCodeViewModel : FormViewModelBase
    {
        #region 필드
        private DataTable _groupList;
        private DataTable _codeList;
        private string _selectedGroupCode;
        #endregion

        #region 속성
        public DataTable GroupList
        {
            get => _groupList;
            set => SetProperty(ref _groupList, value);
        }

        public DataTable CodeList
        {
            get => _codeList;
            set => SetProperty(ref _codeList, value);
        }

        public string SelectedGroupCode
        {
            get => _selectedGroupCode;
            set
            {
                if (SetProperty(ref _selectedGroupCode, value))
                {
                    LoadCodes();
                }
            }
        }
        #endregion

        public CommonCodeViewModel()
        {
            MenuId = "MST_CODE";
            FormTitle = "공통코드 관리";
        }

        protected override void OnNew()
        {
            // 그룹 추가 모드
            StatusMessage = "신규 코드그룹 입력";
        }

        protected override void OnSearch()
        {
            try
            {
                BeginWork("조회 중...");

                string sql = "SELECT GroupCode, GroupName, Description, IsActive FROM CodeGroup ORDER BY GroupCode";
                GroupList = DbHelper.ExecuteQuery(sql);

                EndWork($"조회 완료: {GroupList.Rows.Count}건");
            }
            catch (Exception ex)
            {
                EndWork("조회 실패");
                ServiceLocator.MessageService.ShowError($"조회 오류: {ex.Message}");
            }
        }

        private void LoadCodes()
        {
            if (string.IsNullOrEmpty(SelectedGroupCode))
            {
                CodeList = null;
                return;
            }

            try
            {
                string sql = @"SELECT Code, CodeName, SortOrder, Remark, IsActive 
                               FROM CommonCode 
                               WHERE GroupCode = @GroupCode 
                               ORDER BY SortOrder, Code";

                CodeList = DbHelper.ExecuteQuery(sql, new SqlParameter("@GroupCode", SelectedGroupCode));
            }
            catch (Exception ex)
            {
                ServiceLocator.MessageService.ShowError($"코드 조회 오류: {ex.Message}");
            }
        }

        protected override void OnSave()
        {
            try
            {
                BeginWork("저장 중...");

                // 그룹 저장
                SaveGroups();

                // 코드 저장
                if (!string.IsNullOrEmpty(SelectedGroupCode))
                {
                    SaveCodes();
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

        private void SaveGroups()
        {
            if (GroupList == null) return;

            foreach (DataRow row in GroupList.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string sql = @"INSERT INTO CodeGroup (GroupCode, GroupName, Description, IsActive, CreatedBy, CreatedAt)
                                   VALUES (@GroupCode, @GroupName, @Description, @IsActive, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@GroupCode", row["GroupCode"]),
                        new SqlParameter("@GroupName", row["GroupName"] ?? DBNull.Value),
                        new SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    string sql = @"UPDATE CodeGroup SET GroupName = @GroupName, Description = @Description,
                                   IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE GroupCode = @GroupCode";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@GroupCode", row["GroupCode"]),
                        new SqlParameter("@GroupName", row["GroupName"] ?? DBNull.Value),
                        new SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
        }

        private void SaveCodes()
        {
            if (CodeList == null) return;

            foreach (DataRow row in CodeList.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string sql = @"INSERT INTO CommonCode (GroupCode, Code, CodeName, SortOrder, Remark, IsActive, CreatedBy, CreatedAt)
                                   VALUES (@GroupCode, @Code, @CodeName, @SortOrder, @Remark, @IsActive, @CreatedBy, GETDATE())";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@GroupCode", SelectedGroupCode),
                        new SqlParameter("@Code", row["Code"]),
                        new SqlParameter("@CodeName", row["CodeName"] ?? DBNull.Value),
                        new SqlParameter("@SortOrder", row["SortOrder"] ?? 0),
                        new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM"));
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    string sql = @"UPDATE CommonCode SET CodeName = @CodeName, SortOrder = @SortOrder,
                                   Remark = @Remark, IsActive = @IsActive, ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                                   WHERE GroupCode = @GroupCode AND Code = @Code";

                    DbHelper.ExecuteNonQuery(sql,
                        new SqlParameter("@GroupCode", SelectedGroupCode),
                        new SqlParameter("@Code", row["Code"]),
                        new SqlParameter("@CodeName", row["CodeName"] ?? DBNull.Value),
                        new SqlParameter("@SortOrder", row["SortOrder"] ?? 0),
                        new SqlParameter("@Remark", row["Remark"] ?? DBNull.Value),
                        new SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM"));
                }
            }
        }

        protected override void OnDelete()
        {
            if (string.IsNullOrEmpty(SelectedGroupCode))
            {
                ServiceLocator.MessageService.ShowWarning("삭제할 코드그룹을 선택하세요.");
                return;
            }

            if (!ServiceLocator.MessageService.Confirm($"'{SelectedGroupCode}' 코드그룹을 삭제하시겠습니까?\n하위 코드도 함께 삭제됩니다."))
                return;

            try
            {
                BeginWork("삭제 중...");

                // 하위 코드 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM CommonCode WHERE GroupCode = @GroupCode",
                    new SqlParameter("@GroupCode", SelectedGroupCode));

                // 그룹 삭제
                DbHelper.ExecuteNonQuery("DELETE FROM CodeGroup WHERE GroupCode = @GroupCode",
                    new SqlParameter("@GroupCode", SelectedGroupCode));

                EndWork("삭제 완료");
                ServiceLocator.MessageService.ShowInfo("삭제되었습니다.");
                SelectedGroupCode = null;
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
