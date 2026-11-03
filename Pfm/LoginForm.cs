using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Pfm.Common;

namespace Pfm
{
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        public LoginForm()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            // 폼 테마 적용
            ThemeManager.ApplyTheme(this);

            // 로그인 버튼 - Primary 스타일
            ThemeManager.ApplyPrimaryButton(btnLogin);

            // 취소 버튼 - Cancel 스타일
            ThemeManager.ApplyCancelButton(btnCancel);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(userId))
            {
                XtraMessageBox.Show("사용자 ID를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserId.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                XtraMessageBox.Show("비밀번호를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // UserSession을 통한 로그인 처리
            if (UserSession.Login(userId, password))
            {
                XtraMessageBox.Show($"환영합니다, {UserSession.UserName}님!", "로그인 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                XtraMessageBox.Show("사용자 ID 또는 비밀번호가 올바르지 않습니다.", "로그인 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = string.Empty;
                txtPassword.Focus();
            }
        }

        private bool ValidateLogin_Legacy(string userId, string password)
        {
            // DB 연결 테스트
            if (!DbHelper.TestConnection())
            {
                // DB 연결 실패 시 기본 계정으로 로그인 허용
                return userId == "admin" && password == "1234";
            }

            try
            {
                string sql = "SELECT COUNT(*) FROM Users WHERE UserId = @UserId AND Password = @Password AND IsActive = 1";
                var parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Password", password)
                };

                int count = Convert.ToInt32(DbHelper.ExecuteScalar(sql, parameters));
                return count > 0;
            }
            catch
            {
                // DB 오류 시 기본 계정으로 로그인 허용
                return userId == "admin" && password == "1234";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtUserId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtPassword.Focus();
                e.Handled = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
