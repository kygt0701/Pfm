using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils;
using Pfm.Common;

namespace Pfm.Forms
{
    /// <summary>
    /// 공통코드 관리 폼
    /// 그리드 인라인 편집 방식
    /// </summary>
    public class CommonCodeForm : BaseForm
    {
        #region 컨트롤
        private SplitContainerControl splitContainer;
        private GridControl gridGroup;
        private GridView gridViewGroup;
        private GridControl gridCode;
        private GridView gridViewCode;
        private LabelControl lblGroup;
        private LabelControl lblCode;
        #endregion

        #region 데이터
        private DataTable dtGroup;
        private DataTable dtCode;
        private string _selectedGroupCode = "";
        #endregion

        public CommonCodeForm()
        {
            MenuId = "MST_CODE";
            FormTitle = "공통코드 관리";
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(1200, 700);

            // 스플릿 컨테이너 (좌: 그룹, 우: 코드)
            splitContainer = new SplitContainerControl();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterPosition = 450;
            splitContainer.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.Panel1;

            // === 좌측: 코드그룹 ===
            var pnlGroup = new PanelControl();
            pnlGroup.Dock = DockStyle.Fill;
            pnlGroup.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            lblGroup = new LabelControl();
            lblGroup.Text = "코드그룹";
            lblGroup.Appearance.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblGroup.Appearance.Options.UseFont = true;
            lblGroup.Location = new Point(10, 10);

            gridGroup = new GridControl();
            gridGroup.Location = new Point(10, 35);
            gridGroup.Dock = DockStyle.None;
            gridGroup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            gridViewGroup = new GridView(gridGroup);
            gridGroup.MainView = gridViewGroup;

            // 그룹 그리드 설정
            SetupGroupGrid();

            pnlGroup.Controls.Add(lblGroup);
            pnlGroup.Controls.Add(gridGroup);

            // === 우측: 공통코드 ===
            var pnlCode = new PanelControl();
            pnlCode.Dock = DockStyle.Fill;
            pnlCode.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            lblCode = new LabelControl();
            lblCode.Text = "공통코드";
            lblCode.Appearance.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblCode.Appearance.Options.UseFont = true;
            lblCode.Location = new Point(10, 10);

            gridCode = new GridControl();
            gridCode.Location = new Point(10, 35);
            gridCode.Dock = DockStyle.None;
            gridCode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            gridViewCode = new GridView(gridCode);
            gridCode.MainView = gridViewCode;

            // 코드 그리드 설정
            SetupCodeGrid();

            pnlCode.Controls.Add(lblCode);
            pnlCode.Controls.Add(gridCode);

            splitContainer.Panel1.Controls.Add(pnlGroup);
            splitContainer.Panel2.Controls.Add(pnlCode);

            pnlContent.Controls.Add(splitContainer);

            // 그리드 크기 조정
            this.Load += (s, e) => AdjustGridSize();
            this.Resize += (s, e) => AdjustGridSize();
        }

        private void AdjustGridSize()
        {
            // 그룹 그리드
            gridGroup.Location = new Point(10, 35);
            gridGroup.Size = new Size(
                splitContainer.Panel1.Width - 20,
                splitContainer.Panel1.Height - 45);

            // 코드 그리드
            gridCode.Location = new Point(10, 35);
            gridCode.Size = new Size(
                splitContainer.Panel2.Width - 20,
                splitContainer.Panel2.Height - 45);
        }

        #region 그룹 그리드 설정
        private void SetupGroupGrid()
        {
            // 컬럼 정의
            gridViewGroup.Columns.Clear();

            var colGroupCode = new GridColumn();
            colGroupCode.FieldName = "GroupCode";
            colGroupCode.Caption = "그룹코드";
            colGroupCode.Width = 100;
            colGroupCode.Visible = true;
            colGroupCode.VisibleIndex = 0;

            var colGroupName = new GridColumn();
            colGroupName.FieldName = "GroupName";
            colGroupName.Caption = "그룹명";
            colGroupName.Width = 150;
            colGroupName.Visible = true;
            colGroupName.VisibleIndex = 1;

            var colDescription = new GridColumn();
            colDescription.FieldName = "Description";
            colDescription.Caption = "설명";
            colDescription.Width = 150;
            colDescription.Visible = true;
            colDescription.VisibleIndex = 2;

            var colIsActive = new GridColumn();
            colIsActive.FieldName = "IsActive";
            colIsActive.Caption = "사용여부";
            colIsActive.Width = 70;
            colIsActive.Visible = true;
            colIsActive.VisibleIndex = 3;

            gridViewGroup.Columns.AddRange(new GridColumn[] { 
                colGroupCode, colGroupName, colDescription, colIsActive });

            // 그리드 옵션 설정
            ApplyGridOptions(gridViewGroup);

            // 행 선택 이벤트
            gridViewGroup.FocusedRowChanged += GridViewGroup_FocusedRowChanged;

            // 테마 적용
            ThemeManager.ApplyGridViewTheme(gridViewGroup);
            SetupGridExcelExport(gridGroup, gridViewGroup, "코드그룹");
        }
        #endregion

        #region 코드 그리드 설정
        private void SetupCodeGrid()
        {
            // 컬럼 정의
            gridViewCode.Columns.Clear();

            var colGroupCode = new GridColumn();
            colGroupCode.FieldName = "GroupCode";
            colGroupCode.Caption = "그룹코드";
            colGroupCode.Width = 80;
            colGroupCode.Visible = true;
            colGroupCode.VisibleIndex = 0;
            colGroupCode.OptionsColumn.AllowEdit = false;
            colGroupCode.OptionsColumn.ReadOnly = true;

            var colCode = new GridColumn();
            colCode.FieldName = "Code";
            colCode.Caption = "코드";
            colCode.Width = 80;
            colCode.Visible = true;
            colCode.VisibleIndex = 1;

            var colCodeName = new GridColumn();
            colCodeName.FieldName = "CodeName";
            colCodeName.Caption = "코드명";
            colCodeName.Width = 150;
            colCodeName.Visible = true;
            colCodeName.VisibleIndex = 2;

            var colSortOrder = new GridColumn();
            colSortOrder.FieldName = "SortOrder";
            colSortOrder.Caption = "정렬순서";
            colSortOrder.Width = 70;
            colSortOrder.Visible = true;
            colSortOrder.VisibleIndex = 3;

            var colIsActive = new GridColumn();
            colIsActive.FieldName = "IsActive";
            colIsActive.Caption = "사용여부";
            colIsActive.Width = 70;
            colIsActive.Visible = true;
            colIsActive.VisibleIndex = 4;

            gridViewCode.Columns.AddRange(new GridColumn[] { 
                colGroupCode, colCode, colCodeName, colSortOrder, colIsActive });

            // 그리드 옵션 설정
            ApplyGridOptions(gridViewCode);

            // 테마 적용
            ThemeManager.ApplyGridViewTheme(gridViewCode);
            SetupGridExcelExport(gridCode, gridViewCode, "코드목록");
        }
        #endregion

        #region 그리드 공통 옵션
        private void ApplyGridOptions(GridView view)
        {
            // 인라인 편집 활성화
            view.OptionsBehavior.Editable = true;
            view.OptionsBehavior.AllowAddRows = DefaultBoolean.True;
            view.OptionsBehavior.AllowDeleteRows = DefaultBoolean.True;

            // 새 행 추가 위치
            view.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;

            // 헤더 기능 활성화
            view.OptionsCustomization.AllowFilter = true;           // 필터
            view.OptionsCustomization.AllowSort = true;             // 정렬
            view.OptionsCustomization.AllowGroup = true;            // 그룹핑
            view.OptionsCustomization.AllowColumnMoving = true;     // 컬럼 이동
            view.OptionsCustomization.AllowColumnResizing = true;   // 컬럼 크기 조정
            view.OptionsCustomization.AllowQuickHideColumns = true; // 컬럼 숨기기

            // 그룹 패널 숨김
            view.OptionsView.ShowGroupPanel = false;

            // 컬럼 헤더 메뉴
            view.OptionsMenu.EnableColumnMenu = true;
            view.OptionsMenu.EnableFooterMenu = true;

            // 푸터 (합계 등)
            view.OptionsView.ShowFooter = false;

            // 행 번호 표시
            view.OptionsView.ShowIndicator = true;

            // 선택 모드
            view.OptionsSelection.MultiSelect = true;
            view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            // 클립보드
            view.OptionsClipboard.AllowCopy = DefaultBoolean.True;
            view.OptionsClipboard.AllowExcelFormat = DefaultBoolean.True;

            // 행 높이
            view.RowHeight = 25;

            // 컬럼 헤더 높이
            view.ColumnPanelRowHeight = 30;
        }
        #endregion

        #region 이벤트 핸들러
        private void GridViewGroup_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0) return;

            var groupCode = gridViewGroup.GetRowCellValue(e.FocusedRowHandle, "GroupCode")?.ToString();
            if (!string.IsNullOrEmpty(groupCode) && groupCode != _selectedGroupCode)
            {
                _selectedGroupCode = groupCode;
                LoadCodeData(groupCode);
            }
        }
        #endregion

        #region 데이터 로드
        private void LoadGroupData()
        {
            try
            {
                string sql = @"
                    SELECT GroupCode, GroupName, Description, IsActive,
                           CreatedBy, CreatedAt, ModifiedBy, ModifiedAt
                    FROM CommonCodeGroup
                    ORDER BY GroupCode";

                dtGroup = DbHelper.ExecuteQuery(sql);
                gridGroup.DataSource = dtGroup;

                // 첫 번째 행 선택
                if (dtGroup.Rows.Count > 0)
                {
                    gridViewGroup.FocusedRowHandle = 0;
                    _selectedGroupCode = dtGroup.Rows[0]["GroupCode"].ToString();
                    LoadCodeData(_selectedGroupCode);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"코드그룹 조회 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCodeData(string groupCode)
        {
            try
            {
                string sql = @"
                    SELECT GroupCode, Code, CodeName, SortOrder, IsActive,
                           CreatedBy, CreatedAt, ModifiedBy, ModifiedAt
                    FROM CommonCode
                    WHERE GroupCode = @GroupCode
                    ORDER BY SortOrder, Code";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@GroupCode", groupCode)
                };

                dtCode = DbHelper.ExecuteQuery(sql, parameters);
                gridCode.DataSource = dtCode;

                lblCode.Text = $"공통코드 [{groupCode}]";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"공통코드 조회 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 공통 버튼 이벤트
        protected override void OnNewClick()
        {
            // 그룹 그리드에 포커스가 있으면 그룹 추가
            if (gridGroup.Focused || gridViewGroup.IsEditing)
            {
                gridViewGroup.AddNewRow();
                gridViewGroup.FocusedColumn = gridViewGroup.Columns["GroupCode"];
            }
            else
            {
                // 코드 그리드에 새 행 추가
                if (string.IsNullOrEmpty(_selectedGroupCode))
                {
                    XtraMessageBox.Show("먼저 코드그룹을 선택하세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                gridViewCode.AddNewRow();
                // 그룹코드 자동 설정
                gridViewCode.SetRowCellValue(GridControl.NewItemRowHandle, "GroupCode", _selectedGroupCode);
                gridViewCode.SetRowCellValue(GridControl.NewItemRowHandle, "IsActive", true);
                gridViewCode.SetRowCellValue(GridControl.NewItemRowHandle, "SortOrder", GetNextSortOrder());
                gridViewCode.FocusedColumn = gridViewCode.Columns["Code"];
            }
        }

        private int GetNextSortOrder()
        {
            int maxOrder = 0;
            for (int i = 0; i < gridViewCode.RowCount; i++)
            {
                var order = gridViewCode.GetRowCellValue(i, "SortOrder");
                if (order != null && order != DBNull.Value)
                {
                    int val = Convert.ToInt32(order);
                    if (val > maxOrder) maxOrder = val;
                }
            }
            return maxOrder + 10;
        }

        protected override void OnSearchClick()
        {
            LoadGroupData();
        }

        protected override void OnSaveClick()
        {
            try
            {
                // 편집 종료
                gridViewGroup.CloseEditor();
                gridViewGroup.UpdateCurrentRow();
                gridViewCode.CloseEditor();
                gridViewCode.UpdateCurrentRow();

                int savedCount = 0;

                // 그룹 저장
                savedCount += SaveGroupData();

                // 코드 저장
                savedCount += SaveCodeData();

                if (savedCount > 0)
                {
                    XtraMessageBox.Show($"{savedCount}건이 저장되었습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 데이터 새로고침
                    LoadGroupData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"저장 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int SaveGroupData()
        {
            int count = 0;
            if (dtGroup == null) return count;

            foreach (DataRow row in dtGroup.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string sql = @"
                        INSERT INTO CommonCodeGroup (GroupCode, GroupName, Description, IsActive, CreatedBy, CreatedAt)
                        VALUES (@GroupCode, @GroupName, @Description, @IsActive, @CreatedBy, GETDATE())";

                    var parameters = new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@GroupCode", row["GroupCode"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@GroupName", row["GroupName"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new System.Data.SqlClient.SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM")
                    };

                    DbHelper.ExecuteNonQuery(sql, parameters);
                    count++;
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    string sql = @"
                        UPDATE CommonCodeGroup 
                        SET GroupName = @GroupName, Description = @Description, IsActive = @IsActive,
                            ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE GroupCode = @GroupCode";

                    var parameters = new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@GroupCode", row["GroupCode"]),
                        new System.Data.SqlClient.SqlParameter("@GroupName", row["GroupName"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@Description", row["Description"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new System.Data.SqlClient.SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM")
                    };

                    DbHelper.ExecuteNonQuery(sql, parameters);
                    count++;
                }
            }

            return count;
        }

        private int SaveCodeData()
        {
            int count = 0;
            if (dtCode == null) return count;

            foreach (DataRow row in dtCode.Rows)
            {
                if (row.RowState == DataRowState.Added)
                {
                    string sql = @"
                        INSERT INTO CommonCode (GroupCode, Code, CodeName, SortOrder, IsActive, CreatedBy, CreatedAt)
                        VALUES (@GroupCode, @Code, @CodeName, @SortOrder, @IsActive, @CreatedBy, GETDATE())";

                    var parameters = new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@GroupCode", row["GroupCode"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@Code", row["Code"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@CodeName", row["CodeName"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@SortOrder", row["SortOrder"] ?? 0),
                        new System.Data.SqlClient.SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new System.Data.SqlClient.SqlParameter("@CreatedBy", UserSession.UserId ?? "SYSTEM")
                    };

                    DbHelper.ExecuteNonQuery(sql, parameters);
                    count++;
                }
                else if (row.RowState == DataRowState.Modified)
                {
                    string sql = @"
                        UPDATE CommonCode 
                        SET CodeName = @CodeName, SortOrder = @SortOrder, IsActive = @IsActive,
                            ModifiedBy = @ModifiedBy, ModifiedAt = GETDATE()
                        WHERE GroupCode = @GroupCode AND Code = @Code";

                    var parameters = new System.Data.SqlClient.SqlParameter[]
                    {
                        new System.Data.SqlClient.SqlParameter("@GroupCode", row["GroupCode"]),
                        new System.Data.SqlClient.SqlParameter("@Code", row["Code"]),
                        new System.Data.SqlClient.SqlParameter("@CodeName", row["CodeName"] ?? DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@SortOrder", row["SortOrder"] ?? 0),
                        new System.Data.SqlClient.SqlParameter("@IsActive", row["IsActive"] ?? true),
                        new System.Data.SqlClient.SqlParameter("@ModifiedBy", UserSession.UserId ?? "SYSTEM")
                    };

                    DbHelper.ExecuteNonQuery(sql, parameters);
                    count++;
                }
            }

            return count;
        }

        protected override void OnDeleteClick()
        {
            // 코드 그리드에 포커스가 있으면 코드 삭제
            if (gridCode.Focused && gridViewCode.FocusedRowHandle >= 0)
            {
                var code = gridViewCode.GetRowCellValue(gridViewCode.FocusedRowHandle, "Code")?.ToString();
                var groupCode = gridViewCode.GetRowCellValue(gridViewCode.FocusedRowHandle, "GroupCode")?.ToString();

                if (XtraMessageBox.Show($"코드 '{code}'를 삭제하시겠습니까?", "확인",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        string sql = "DELETE FROM CommonCode WHERE GroupCode = @GroupCode AND Code = @Code";
                        var parameters = new System.Data.SqlClient.SqlParameter[]
                        {
                            new System.Data.SqlClient.SqlParameter("@GroupCode", groupCode),
                            new System.Data.SqlClient.SqlParameter("@Code", code)
                        };

                        DbHelper.ExecuteNonQuery(sql, parameters);
                        LoadCodeData(_selectedGroupCode);

                        XtraMessageBox.Show("삭제되었습니다.", "알림",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // 그룹 그리드에 포커스가 있으면 그룹 삭제
            else if (gridGroup.Focused && gridViewGroup.FocusedRowHandle >= 0)
            {
                var groupCode = gridViewGroup.GetRowCellValue(gridViewGroup.FocusedRowHandle, "GroupCode")?.ToString();

                if (XtraMessageBox.Show($"코드그룹 '{groupCode}'와 하위 코드를 모두 삭제하시겠습니까?", "확인",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        // 하위 코드 먼저 삭제
                        string sql1 = "DELETE FROM CommonCode WHERE GroupCode = @GroupCode";
                        var param1 = new System.Data.SqlClient.SqlParameter[]
                        {
                            new System.Data.SqlClient.SqlParameter("@GroupCode", groupCode)
                        };
                        DbHelper.ExecuteNonQuery(sql1, param1);

                        // 그룹 삭제
                        string sql2 = "DELETE FROM CommonCodeGroup WHERE GroupCode = @GroupCode";
                        DbHelper.ExecuteNonQuery(sql2, param1);

                        LoadGroupData();

                        XtraMessageBox.Show("삭제되었습니다.", "알림",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"삭제 오류: {ex.Message}", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        #endregion
    }
}
