using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace Pfm.Common
{
    /// <summary>
    /// 모든 업무 폼의 기본 클래스
    /// 우측 상단 공통 버튼(신규, 조회, 저장, 삭제) 및 권한 처리
    /// </summary>
    public class BaseForm : XtraForm
    {
        #region 컨트롤
        protected PanelControl pnlTop;
        protected LabelControl lblTitle;
        protected SimpleButton btnNew;
        protected SimpleButton btnSearch;
        protected SimpleButton btnSave;
        protected SimpleButton btnDelete;
        protected PanelControl pnlContent;
        #endregion

        #region 속성
        /// <summary>
        /// 메뉴 ID (권한 체크용)
        /// </summary>
        public virtual string MenuId { get; set; }

        /// <summary>
        /// 폼 제목
        /// </summary>
        public virtual string FormTitle { get; set; } = "업무 화면";

        /// <summary>
        /// 현재 메뉴 권한
        /// </summary>
        protected MenuPermission Permission { get; private set; }
        #endregion

        public BaseForm()
        {
            InitializeBaseComponents();
        }

        private void InitializeBaseComponents()
        {
            this.SuspendLayout();

            // 상단 패널
            pnlTop = new PanelControl();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 50;
            pnlTop.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // 제목 라벨
            lblTitle = new LabelControl();
            lblTitle.Appearance.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(12, 14);
            lblTitle.Text = FormTitle;

            // 버튼 패널 (우측 정렬)
            var pnlButtons = new PanelControl();
            pnlButtons.Dock = DockStyle.Right;
            pnlButtons.Width = 380;
            pnlButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // 신규 버튼
            btnNew = new SimpleButton();
            btnNew.Text = "신규 (F2)";
            btnNew.Size = new Size(85, 32);
            btnNew.Location = new Point(10, 9);
            btnNew.Click += (s, e) => OnNewClick();
            ThemeManager.ApplyDefaultButton(btnNew);

            // 조회 버튼
            btnSearch = new SimpleButton();
            btnSearch.Text = "조회 (F5)";
            btnSearch.Size = new Size(85, 32);
            btnSearch.Location = new Point(100, 9);
            btnSearch.Click += (s, e) => OnSearchClick();
            ThemeManager.ApplySecondaryButton(btnSearch);

            // 저장 버튼
            btnSave = new SimpleButton();
            btnSave.Text = "저장 (F6)";
            btnSave.Size = new Size(85, 32);
            btnSave.Location = new Point(190, 9);
            btnSave.Click += (s, e) => OnSaveClick();
            ThemeManager.ApplyPrimaryButton(btnSave);

            // 삭제 버튼
            btnDelete = new SimpleButton();
            btnDelete.Text = "삭제 (F8)";
            btnDelete.Size = new Size(85, 32);
            btnDelete.Location = new Point(280, 9);
            btnDelete.Click += (s, e) => OnDeleteClick();
            ThemeManager.ApplyErrorButton(btnDelete);

            pnlButtons.Controls.AddRange(new Control[] { btnNew, btnSearch, btnSave, btnDelete });
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(pnlButtons);

            // 컨텐츠 패널
            pnlContent = new PanelControl();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlTop);

            // 폼 설정
            this.KeyPreview = true;
            this.KeyDown += BaseForm_KeyDown;
            this.Load += BaseForm_Load;

            this.ResumeLayout(false);
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            // 제목 설정
            lblTitle.Text = FormTitle;
            this.Text = FormTitle;

            // 권한 로드 및 적용
            LoadPermission();
            ApplyPermission();

            // 테마 적용
            ThemeManager.ApplyTheme(this);
        }

        /// <summary>
        /// 권한 로드
        /// </summary>
        protected virtual void LoadPermission()
        {
            if (!string.IsNullOrEmpty(MenuId))
            {
                Permission = UserSession.GetPermission(MenuId);
            }
            else
            {
                // 기본 권한 (모두 허용)
                Permission = new MenuPermission
                {
                    CanRead = true,
                    CanCreate = true,
                    CanSave = true,
                    CanDelete = true
                };
            }
        }

        /// <summary>
        /// 권한에 따른 버튼 활성화/비활성화
        /// </summary>
        protected virtual void ApplyPermission()
        {
            btnNew.Enabled = Permission.CanCreate;
            btnSearch.Enabled = Permission.CanRead;
            btnSave.Enabled = Permission.CanSave;
            btnDelete.Enabled = Permission.CanDelete;

            // 비활성화된 버튼 스타일 변경
            if (!btnNew.Enabled) ApplyDisabledStyle(btnNew);
            if (!btnSearch.Enabled) ApplyDisabledStyle(btnSearch);
            if (!btnSave.Enabled) ApplyDisabledStyle(btnSave);
            if (!btnDelete.Enabled) ApplyDisabledStyle(btnDelete);
        }

        private void ApplyDisabledStyle(SimpleButton button)
        {
            button.Appearance.BackColor = AppColors.TextDisabled;
            button.Appearance.ForeColor = Color.White;
            button.Appearance.Options.UseBackColor = true;
            button.Appearance.Options.UseForeColor = true;
        }

        /// <summary>
        /// 단축키 처리
        /// </summary>
        private void BaseForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    if (btnNew.Enabled) OnNewClick();
                    e.Handled = true;
                    break;
                case Keys.F5:
                    if (btnSearch.Enabled) OnSearchClick();
                    e.Handled = true;
                    break;
                case Keys.F6:
                    if (btnSave.Enabled) OnSaveClick();
                    e.Handled = true;
                    break;
                case Keys.F8:
                    if (btnDelete.Enabled) OnDeleteClick();
                    e.Handled = true;
                    break;
            }
        }

        #region 가상 메서드 (하위 폼에서 오버라이드)
        /// <summary>
        /// 신규 버튼 클릭
        /// </summary>
        protected virtual void OnNewClick()
        {
            // 하위 폼에서 구현
        }

        /// <summary>
        /// 조회 버튼 클릭
        /// </summary>
        protected virtual void OnSearchClick()
        {
            // 하위 폼에서 구현
        }

        /// <summary>
        /// 저장 버튼 클릭
        /// </summary>
        protected virtual void OnSaveClick()
        {
            // 하위 폼에서 구현
        }

        /// <summary>
        /// 삭제 버튼 클릭
        /// </summary>
        protected virtual void OnDeleteClick()
        {
            // 하위 폼에서 구현
        }
        #endregion

        #region 헬퍼 메서드
        /// <summary>
        /// 권한 체크 후 메시지 표시
        /// </summary>
        protected bool CheckPermission(PermissionType type, bool showMessage = true)
        {
            bool hasPermission = false;
            string actionName = "";

            switch (type)
            {
                case PermissionType.Read:
                    hasPermission = Permission.CanRead;
                    actionName = "조회";
                    break;
                case PermissionType.Create:
                    hasPermission = Permission.CanCreate;
                    actionName = "신규";
                    break;
                case PermissionType.Save:
                    hasPermission = Permission.CanSave;
                    actionName = "저장";
                    break;
                case PermissionType.Delete:
                    hasPermission = Permission.CanDelete;
                    actionName = "삭제";
                    break;
            }

            if (!hasPermission && showMessage)
            {
                XtraMessageBox.Show($"{actionName} 권한이 없습니다.", "권한 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return hasPermission;
        }

        /// <summary>
        /// 확인 메시지 표시
        /// </summary>
        protected bool Confirm(string message)
        {
            return XtraMessageBox.Show(message, "확인",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// 정보 메시지 표시
        /// </summary>
        protected void ShowInfo(string message)
        {
            XtraMessageBox.Show(message, "알림",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 오류 메시지 표시
        /// </summary>
        protected void ShowError(string message)
        {
            XtraMessageBox.Show(message, "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 경고 메시지 표시
        /// </summary>
        protected void ShowWarning(string message)
        {
            XtraMessageBox.Show(message, "경고",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        #region 그리드 엑셀 내보내기
        /// <summary>
        /// 그리드에 우클릭 엑셀 다운로드 메뉴 추가
        /// </summary>
        protected void SetupGridExcelExport(GridControl grid, GridView gridView, string defaultFileName = null)
        {
            var contextMenu = new ContextMenuStrip();
            var menuExcel = new ToolStripMenuItem("엑셀 다운로드");
            menuExcel.Image = null;
            menuExcel.Click += (s, e) => ExportGridToExcel(grid, gridView, defaultFileName);
            contextMenu.Items.Add(menuExcel);

            gridView.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    contextMenu.Show(grid, e.Location);
                }
            };
        }

        /// <summary>
        /// 그리드를 엑셀로 내보내기
        /// </summary>
        protected void ExportGridToExcel(GridControl grid, GridView gridView, string defaultFileName = null)
        {
            try
            {
                if (gridView.RowCount == 0)
                {
                    ShowInfo("내보낼 데이터가 없습니다.");
                    return;
                }

                string fileName = defaultFileName ?? $"{FormTitle}_{DateTime.Now:yyyyMMdd}";

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Excel 파일 (*.xlsx)|*.xlsx|Excel 97-2003 (*.xls)|*.xls";
                    sfd.FileName = fileName;
                    sfd.DefaultExt = "xlsx";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string ext = Path.GetExtension(sfd.FileName).ToLower();

                        if (ext == ".xlsx")
                        {
                            grid.ExportToXlsx(sfd.FileName);
                        }
                        else
                        {
                            grid.ExportToXls(sfd.FileName);
                        }

                        ShowInfo("엑셀 파일이 저장되었습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"엑셀 저장 중 오류가 발생했습니다.\n{ex.Message}");
            }
        }
        #endregion
    }
}
