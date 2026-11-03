namespace Pfm
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.treeListMenu = new DevExpress.XtraTreeList.TreeList();
            this.colMenuName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.btnToggleMenu = new DevExpress.XtraEditors.SimpleButton();
            this.lblUserInfo = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).BeginInit();
            this.splitContainerControl1.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).BeginInit();
            this.splitContainerControl1.Panel2.SuspendLayout();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListMenu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnToggleMenu
            // 
            this.btnToggleMenu.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.btnToggleMenu.Appearance.Options.UseFont = true;
            this.btnToggleMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnToggleMenu.Location = new System.Drawing.Point(0, 0);
            this.btnToggleMenu.Name = "btnToggleMenu";
            this.btnToggleMenu.Size = new System.Drawing.Size(30, 768);
            this.btnToggleMenu.TabIndex = 1;
            this.btnToggleMenu.Text = "◀";
            this.btnToggleMenu.ToolTip = "메뉴 숨기기/보이기 (F3)";
            this.btnToggleMenu.Click += new System.EventHandler(this.btnToggleMenu_Click);
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.Panel1;
            this.splitContainerControl1.Location = new System.Drawing.Point(30, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            // 
            // splitContainerControl1.Panel1
            // 
            this.splitContainerControl1.Panel1.Controls.Add(this.treeListMenu);
            this.splitContainerControl1.Panel1.MinSize = 200;
            this.splitContainerControl1.Panel1.Text = "Panel1";
            // 
            // splitContainerControl1.Panel2
            // 
            this.splitContainerControl1.Panel2.Controls.Add(this.xtraTabControl1);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(994, 768);
            this.splitContainerControl1.SplitterPosition = 220;
            this.splitContainerControl1.TabIndex = 0;
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.Size = new System.Drawing.Size(800, 768);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.ClosePageButtonShowMode = DevExpress.XtraTab.ClosePageButtonShowMode.InAllTabPageHeaders;
            this.xtraTabControl1.CloseButtonClick += new System.EventHandler(this.xtraTabControl1_CloseButtonClick);
            // 둥근 탭 헤더 스타일
            this.xtraTabControl1.HeaderAutoFill = DevExpress.Utils.DefaultBoolean.False;
            this.xtraTabControl1.HeaderButtonsShowMode = DevExpress.XtraTab.TabButtonShowMode.WhenNeeded;
            this.xtraTabControl1.AppearancePage.Header.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.xtraTabControl1.AppearancePage.Header.Options.UseBorderColor = true;
            this.xtraTabControl1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            this.xtraTabControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.xtraTabControl1.BorderStylePage = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            // 
            // treeListMenu
            // 
            this.treeListMenu.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.colMenuName});
            this.treeListMenu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListMenu.Location = new System.Drawing.Point(0, 0);
            this.treeListMenu.Name = "treeListMenu";
            this.treeListMenu.OptionsBehavior.Editable = false;
            this.treeListMenu.OptionsView.ShowColumns = false;
            this.treeListMenu.OptionsView.ShowIndicator = false;
            this.treeListMenu.Size = new System.Drawing.Size(220, 768);
            this.treeListMenu.TabIndex = 0;
            this.treeListMenu.DoubleClick += new System.EventHandler(this.treeListMenu_DoubleClick);
            // 
            // colMenuName
            // 
            this.colMenuName.Caption = "메뉴";
            this.colMenuName.FieldName = "MenuName";
            this.colMenuName.Name = "colMenuName";
            this.colMenuName.Visible = true;
            this.colMenuName.VisibleIndex = 0;
            this.colMenuName.Width = 200;
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.lblUserInfo.Appearance.Options.UseFont = true;
            this.lblUserInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUserInfo.Location = new System.Drawing.Point(0, 0);
            this.lblUserInfo.Name = "lblUserInfo";
            this.lblUserInfo.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.lblUserInfo.Size = new System.Drawing.Size(100, 30);
            this.lblUserInfo.TabIndex = 2;
            this.lblUserInfo.Text = "사용자 정보";
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.btnToggleMenu);
            this.Controls.Add(this.lblUserInfo);
            this.IsMdiContainer = false;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "PFM 시스템";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel1)).EndInit();
            this.splitContainerControl1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1.Panel2)).EndInit();
            this.splitContainerControl1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListMenu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraTreeList.TreeList treeListMenu;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colMenuName;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraEditors.SimpleButton btnToggleMenu;
        private DevExpress.XtraEditors.LabelControl lblUserInfo;
    }
}
