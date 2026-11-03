using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using Pfm.Common;
using Pfm.Forms;

namespace Pfm
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeTreeMenu();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // 메인 폼 테마 적용
            ThemeManager.ApplyTheme(this);

            // TreeList 테마 적용
            ThemeManager.ApplyTreeListTheme(treeListMenu);

            // 탭 컨트롤 테마 적용 (둥근 탭 헤더)
            ThemeManager.ApplyTabControlTheme(xtraTabControl1);

            // 사용자 정보 라벨 스타일
            lblUserInfo.BackColor = AppColors.BackgroundHeader;
            lblUserInfo.ForeColor = AppColors.TextLight;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // 사용자 정보 표시
            UpdateUserInfo();
        }

        private void UpdateUserInfo()
        {
            lblUserInfo.Text = $"👤 {UserSession.UserName} ({UserSession.UserId}) | 로그인: {DateTime.Now:yyyy-MM-dd HH:mm}";
        }

        private void InitializeTreeMenu()
        {
            DataTable dt = LoadMenuFromDatabase();
            
            treeListMenu.DataSource = dt;
            treeListMenu.KeyFieldName = "MenuId";
            treeListMenu.ParentFieldName = "ParentMenuId";
            treeListMenu.ExpandAll();
        }

        /// <summary>
        /// DB에서 메뉴 로드
        /// </summary>
        private DataTable LoadMenuFromDatabase()
        {
            try
            {
                string sql = @"SELECT MenuId, ParentMenuId, MenuName, FormName AS MenuTag, MenuOrder
                               FROM Menu 
                               WHERE IsActive = 1
                               ORDER BY MenuOrder, MenuId";

                return DbHelper.ExecuteQuery(sql);
            }
            catch
            {
                // DB 연결 실패 시 기본 메뉴 반환
                return GetDefaultMenuData();
            }
        }

        /// <summary>
        /// 기본 메뉴 데이터 (DB 연결 실패 시)
        /// </summary>
        private DataTable GetDefaultMenuData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MenuId", typeof(string));
            dt.Columns.Add("ParentMenuId", typeof(string));
            dt.Columns.Add("MenuName", typeof(string));
            dt.Columns.Add("MenuTag", typeof(string));

            // 시스템 관리
            dt.Rows.Add("SYS", null, "시스템 관리", "");
            dt.Rows.Add("SYS_LOGIN", "SYS", "로그인", "Login");
            dt.Rows.Add("SYS_USER", "SYS", "사용자 관리", "UserManage");
            dt.Rows.Add("SYS_AUTH", "SYS", "권한 관리", "AuthManage");

            // 기준정보
            dt.Rows.Add("MST", null, "기준정보", "");
            dt.Rows.Add("MST_CODE", "MST", "공통코드 관리", "CommonCode");
            dt.Rows.Add("MST_CUST", "MST", "거래처 관리", "Customer");
            dt.Rows.Add("MST_ITEM", "MST", "품목 관리", "Item");

            // 업무
            dt.Rows.Add("WRK", null, "업무", "");
            dt.Rows.Add("ORD_ORDER", "WRK", "주문 관리", "Order");
            dt.Rows.Add("INV_STOCK", "WRK", "재고 관리", "Inventory");
            dt.Rows.Add("SHP_SHIP", "WRK", "출하 관리", "Shipment");
            dt.Rows.Add("ACC_RECV", "WRK", "미수금 관리", "Receivable");

            // 프로그램
            dt.Rows.Add("PRG", null, "프로그램", "");
            dt.Rows.Add("PRG_EXIT", "PRG", "종료", "Exit");

            return dt;
        }

        /// <summary>
        /// 메뉴 접근 권한 확인
        /// </summary>
        private bool CheckMenuPermission(string menuId, string menuName)
        {
            if (string.IsNullOrEmpty(menuId)) return true;

            if (!UserSession.HasPermission(menuId, PermissionType.Read))
            {
                XtraMessageBox.Show($"'{menuName}' 메뉴에 대한 접근 권한이 없습니다.\n관리자에게 문의하세요.", 
                    "권한 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void treeListMenu_DoubleClick(object sender, EventArgs e)
        {
            TreeListNode node = treeListMenu.FocusedNode;
            if (node == null) return;

            string menuId = node.GetValue("MenuId")?.ToString();
            string menuTag = node.GetValue("MenuTag")?.ToString();
            string menuName = node.GetValue("MenuName")?.ToString();
            if (string.IsNullOrEmpty(menuTag)) return;

            // 종료, 로그인은 권한 체크 제외
            if (menuTag != "Exit" && menuTag != "Login")
            {
                if (!CheckMenuPermission(menuId, menuName)) return;
            }

            switch (menuTag)
            {
                case "Login":
                    // 이미 로그인됨 - 로그아웃 처리
                    if (XtraMessageBox.Show("로그아웃 하시겠습니까?", "확인",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        UserSession.Logout();
                        this.Close();
                    }
                    break;
                case "UserForm":
                    OpenFormInTab<UserForm>(menuTag, menuName);
                    break;
                case "AuthForm":
                    OpenFormInTab<AuthForm>(menuTag, menuName);
                    break;
                case "CommonCodeForm":
                    OpenFormInTab<CommonCodeForm>(menuTag, menuName);
                    break;
                case "CustomerForm":
                    OpenFormInTab<CustomerForm>(menuTag, menuName);
                    break;
                case "ItemForm":
                    OpenFormInTab<ItemForm>(menuTag, menuName);
                    break;
                case "OrderForm":
                    OpenFormInTab<OrderForm>(menuTag, menuName);
                    break;
                case "InventoryForm":
                    OpenFormInTab<InventoryForm>(menuTag, menuName);
                    break;
                case "ShipmentForm":
                    OpenFormInTab<ShipmentForm>(menuTag, menuName);
                    break;
                case "ReceivableForm":
                    OpenFormInTab<ReceivableForm>(menuTag, menuName);
                    break;
                case "Exit":
                    Application.Exit();
                    break;
                default:
                    XtraMessageBox.Show($"'{menuName}' 기능은 준비 중입니다.", "알림", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        /// <summary>
        /// 탭에 폼 열기 (중복 방지)
        /// </summary>
        private void OpenFormInTab<T>(string tabName, string tabText) where T : Control, new()
        {
            // 이미 열린 탭이 있는지 확인
            foreach (XtraTabPage page in xtraTabControl1.TabPages)
            {
                if (page.Name == tabName)
                {
                    xtraTabControl1.SelectedTabPage = page;
                    return;
                }
            }

            // 새 탭 생성
            XtraTabPage newPage = new XtraTabPage();
            newPage.Name = tabName;
            newPage.Text = tabText;
            newPage.ShowCloseButton = DevExpress.Utils.DefaultBoolean.True;

            // 폼/컨트롤 생성 및 탭에 추가
            T control = new T();

            // Form인 경우 특별 처리
            if (control is Form form)
            {
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                newPage.Controls.Add(form);
                form.Show();
            }
            else
            {
                control.Dock = DockStyle.Fill;
                newPage.Controls.Add(control);
            }

            xtraTabControl1.TabPages.Add(newPage);
            xtraTabControl1.SelectedTabPage = newPage;
        }

        /// <summary>
        /// 탭 닫기 버튼 클릭 이벤트
        /// </summary>
        private void xtraTabControl1_CloseButtonClick(object sender, EventArgs e)
        {
            DevExpress.XtraTab.ViewInfo.ClosePageButtonEventArgs args = e as DevExpress.XtraTab.ViewInfo.ClosePageButtonEventArgs;
            if (args != null && args.Page != null)
            {
                XtraTabPage tabPage = args.Page as XtraTabPage;
                if (tabPage != null)
                {
                    // 탭 내 컨트롤 정리
                    foreach (Control ctrl in tabPage.Controls)
                    {
                        if (ctrl is Form form)
                        {
                            form.Close();
                        }
                        ctrl.Dispose();
                    }
                    xtraTabControl1.TabPages.Remove(tabPage);
                }
            }
        }

        #region 메뉴 토글 기능
        private bool _isMenuVisible = true;
        private int _lastSplitterPosition = 220;

        private void btnToggleMenu_Click(object sender, EventArgs e)
        {
            ToggleMenu();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // F3 키로 메뉴 토글
            if (e.KeyCode == Keys.F3)
            {
                ToggleMenu();
                e.Handled = true;
            }
        }

        private void ToggleMenu()
        {
            if (_isMenuVisible)
            {
                // 메뉴 숨기기
                _lastSplitterPosition = splitContainerControl1.SplitterPosition;
                splitContainerControl1.SplitterPosition = 0;
                splitContainerControl1.Panel1.MinSize = 0;
                splitContainerControl1.Collapsed = true;
                splitContainerControl1.CollapsePanel = DevExpress.XtraEditors.SplitCollapsePanel.Panel1;
                btnToggleMenu.Text = "▶";
                btnToggleMenu.ToolTip = "메뉴 보이기 (F3)";
            }
            else
            {
                // 메뉴 보이기
                splitContainerControl1.Collapsed = false;
                splitContainerControl1.Panel1.MinSize = 200;
                splitContainerControl1.SplitterPosition = _lastSplitterPosition;
                btnToggleMenu.Text = "◀";
                btnToggleMenu.ToolTip = "메뉴 숨기기 (F3)";
            }
            _isMenuVisible = !_isMenuVisible;
        }
        #endregion
    }
}
