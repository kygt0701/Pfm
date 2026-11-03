using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTreeList;

namespace Pfm.Common
{
    /// <summary>
    /// 테마 관리자 - DevExpress 컨트롤에 SaaS 스타일 테마 적용
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// 애플리케이션 전체 테마 초기화
        /// Program.cs의 Main()에서 호출
        /// </summary>
        public static void Initialize()
        {
            // DevExpress 기본 스킨 설정 (Office 2019 Colorful 기반)
            UserLookAndFeel.Default.SetSkinStyle(SkinStyle.Office2019Colorful);

            // 기본 폰트 설정
            WindowsFormsSettings.DefaultFont = new Font("맑은 고딕", 9F);
        }

        /// <summary>
        /// XtraForm에 테마 적용
        /// </summary>
        public static void ApplyTheme(XtraForm form)
        {
            form.BackColor = AppColors.BackgroundMain;
        }

        /// <summary>
        /// 저장/확정 버튼 스타일 적용 (Primary)
        /// </summary>
        public static void ApplyPrimaryButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.Primary;
            button.Appearance.ForeColor = AppColors.TextLight;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.Primary;
        }

        /// <summary>
        /// 보조 버튼 스타일 적용 (Secondary)
        /// </summary>
        public static void ApplySecondaryButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.Secondary;
            button.Appearance.ForeColor = AppColors.TextLight;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.Secondary;
        }

        /// <summary>
        /// 기본 버튼 스타일 적용 (Default - 흰색 배경)
        /// </summary>
        public static void ApplyDefaultButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.ButtonDefault;
            button.Appearance.ForeColor = AppColors.TextPrimary;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.ButtonDefaultBorder;
        }

        /// <summary>
        /// 취소 버튼 스타일 적용
        /// </summary>
        public static void ApplyCancelButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.ButtonCancel;
            button.Appearance.ForeColor = AppColors.TextLight;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.ButtonCancel;
        }

        /// <summary>
        /// 경고 버튼 스타일 적용 (Warning)
        /// </summary>
        public static void ApplyWarningButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.Warning;
            button.Appearance.ForeColor = AppColors.TextLight;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.Warning;
        }

        /// <summary>
        /// 오류/삭제 버튼 스타일 적용 (Error)
        /// </summary>
        public static void ApplyErrorButton(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.Error;
            button.Appearance.ForeColor = AppColors.TextLight;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
            button.Appearance.Options.UseBorderColor = true;
            button.Appearance.BorderColor = AppColors.Error;
        }

        /// <summary>
        /// GridControl에 테마 적용
        /// </summary>
        public static void ApplyGridTheme(GridControl grid)
        {
            if (grid.MainView is GridView view)
            {
                ApplyGridViewTheme(view);
            }
        }

        /// <summary>
        /// GridView에 테마 적용
        /// </summary>
        public static void ApplyGridViewTheme(GridView view)
        {
            // 헤더 스타일 - 가운데 정렬, 진한 배경
            view.Appearance.HeaderPanel.BackColor = AppColors.GridHeader;
            view.Appearance.HeaderPanel.ForeColor = AppColors.GridHeaderText;
            view.Appearance.HeaderPanel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            view.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            view.Appearance.HeaderPanel.Options.UseBackColor = true;
            view.Appearance.HeaderPanel.Options.UseForeColor = true;
            view.Appearance.HeaderPanel.Options.UseFont = true;
            view.Appearance.HeaderPanel.Options.UseTextOptions = true;

            // 모든 컬럼 헤더 가운데 정렬
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in view.Columns)
            {
                col.AppearanceHeader.BackColor = AppColors.GridHeader;
                col.AppearanceHeader.ForeColor = AppColors.GridHeaderText;
                col.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                col.AppearanceHeader.Options.UseBackColor = true;
                col.AppearanceHeader.Options.UseForeColor = true;
                col.AppearanceHeader.Options.UseTextOptions = true;
            }

            // 짝수 행
            view.Appearance.EvenRow.BackColor = AppColors.GridRowEven;
            view.Appearance.EvenRow.Options.UseBackColor = true;
            view.OptionsView.EnableAppearanceEvenRow = true;

            // 홀수 행
            view.Appearance.OddRow.BackColor = AppColors.GridRowOdd;
            view.Appearance.OddRow.Options.UseBackColor = true;
            view.OptionsView.EnableAppearanceOddRow = true;

            // 선택 행 (포인트 컬러)
            view.Appearance.FocusedRow.BackColor = AppColors.GridRowSelected;
            view.Appearance.FocusedRow.ForeColor = AppColors.TextPrimary;
            view.Appearance.FocusedRow.Options.UseBackColor = true;
            view.Appearance.FocusedRow.Options.UseForeColor = true;

            // 선택 셀
            view.Appearance.FocusedCell.BackColor = AppColors.PrimaryLight;
            view.Appearance.FocusedCell.ForeColor = AppColors.TextPrimary;
            view.Appearance.FocusedCell.Options.UseBackColor = true;
            view.Appearance.FocusedCell.Options.UseForeColor = true;

            // 엑셀 스타일 필터 설정
            view.OptionsFilter.AllowFilterEditor = true;
            view.OptionsFilter.AllowColumnMRUFilterList = true;
            view.OptionsFilter.AllowMRUFilterList = true;
            view.OptionsFilter.AllowAutoFilterConditionChange = DevExpress.Utils.DefaultBoolean.True;
            view.OptionsFilter.ShowAllTableValuesInFilterPopup = true;
            view.OptionsFilter.AllowFilterIncrementalSearch = true;

            // 컬럼 헤더 필터 버튼 (엑셀 스타일)
            view.OptionsView.ShowAutoFilterRow = false;
            view.OptionsView.HeaderFilterButtonShowMode = DevExpress.XtraEditors.Controls.FilterButtonShowMode.Button;

            // 필터 패널 숨김
            view.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;

            // 행 번호 표시 (Row Indicator)
            view.OptionsView.ShowIndicator = true;
            view.IndicatorWidth = 40;
            view.CustomDrawRowIndicator += GridView_CustomDrawRowIndicator;

            // 등록일시, 수정일시 컬럼 날짜시간 포맷 적용
            ApplyDateTimeFormat(view, "CreatedAt");
            ApplyDateTimeFormat(view, "ModifiedAt");
        }

        /// <summary>
        /// 날짜시간 컬럼 포맷 적용 (yyyy-MM-dd HH:mm)
        /// </summary>
        private static void ApplyDateTimeFormat(GridView view, string fieldName)
        {
            var col = view.Columns[fieldName];
            if (col != null)
            {
                col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                col.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm";
            }
        }

        /// <summary>
        /// 그리드 행 번호 그리기
        /// </summary>
        private static void GridView_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
                e.Info.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
        }

        /// <summary>
        /// TreeList에 테마 적용
        /// </summary>
        public static void ApplyTreeListTheme(TreeList treeList)
        {
            // 선택 행 (포인트 컬러)
            treeList.Appearance.FocusedRow.BackColor = AppColors.GridRowSelected;
            treeList.Appearance.FocusedRow.ForeColor = AppColors.TextPrimary;
            treeList.Appearance.FocusedRow.Options.UseBackColor = true;
            treeList.Appearance.FocusedRow.Options.UseForeColor = true;

            // 선택 셀
            treeList.Appearance.FocusedCell.BackColor = AppColors.PrimaryLight;
            treeList.Appearance.FocusedCell.ForeColor = AppColors.TextPrimary;
            treeList.Appearance.FocusedCell.Options.UseBackColor = true;
            treeList.Appearance.FocusedCell.Options.UseForeColor = true;
        }

        /// <summary>
        /// XtraTabControl에 둥근 탭 테마 적용
        /// </summary>
        public static void ApplyTabControlTheme(DevExpress.XtraTab.XtraTabControl tabControl)
        {
            // 둥근 탭 헤더 스타일
            tabControl.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            tabControl.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            tabControl.BorderStylePage = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // 탭 헤더 스타일
            tabControl.AppearancePage.Header.BackColor = AppColors.BackgroundPanel;
            tabControl.AppearancePage.Header.ForeColor = AppColors.TextPrimary;
            tabControl.AppearancePage.Header.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);
            tabControl.AppearancePage.Header.Options.UseBackColor = true;
            tabControl.AppearancePage.Header.Options.UseForeColor = true;
            tabControl.AppearancePage.Header.Options.UseBorderColor = true;

            // 활성 탭 헤더 스타일
            tabControl.AppearancePage.HeaderActive.BackColor = AppColors.Primary;
            tabControl.AppearancePage.HeaderActive.ForeColor = System.Drawing.Color.White;
            tabControl.AppearancePage.HeaderActive.Options.UseBackColor = true;
            tabControl.AppearancePage.HeaderActive.Options.UseForeColor = true;

            // 호버 탭 헤더 스타일
            tabControl.AppearancePage.HeaderHotTracked.BackColor = AppColors.PrimaryLight;
            tabControl.AppearancePage.HeaderHotTracked.ForeColor = AppColors.TextPrimary;
            tabControl.AppearancePage.HeaderHotTracked.Options.UseBackColor = true;
            tabControl.AppearancePage.HeaderHotTracked.Options.UseForeColor = true;
        }

        /// <summary>
        /// PanelControl에 테마 적용
        /// </summary>
        public static void ApplyPanelTheme(PanelControl panel)
        {
            panel.Appearance.BackColor = AppColors.BackgroundPanel;
            panel.Appearance.Options.UseBackColor = true;
        }

        /// <summary>
        /// 사이드바 패널에 테마 적용
        /// </summary>
        public static void ApplySidebarTheme(PanelControl panel)
        {
            panel.Appearance.BackColor = AppColors.BackgroundSidebar;
            panel.Appearance.Options.UseBackColor = true;
        }

        #region 상태별 셀 색상 적용 헬퍼
        /// <summary>
        /// 경고 상태 셀 색상 반환 (미수 연체, 여신 초과 등)
        /// </summary>
        public static Color GetWarningBackColor() => AppColors.WarningLight;
        public static Color GetWarningForeColor() => AppColors.WarningText;

        /// <summary>
        /// 오류 상태 셀 색상 반환
        /// </summary>
        public static Color GetErrorBackColor() => AppColors.ErrorLight;
        public static Color GetErrorForeColor() => AppColors.ErrorText;

        /// <summary>
        /// 성공 상태 셀 색상 반환
        /// </summary>
        public static Color GetSuccessBackColor() => AppColors.SuccessLight;
        public static Color GetSuccessForeColor() => AppColors.SuccessText;
        #endregion
    }
}
