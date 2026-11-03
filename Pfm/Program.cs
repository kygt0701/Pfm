using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pfm.Common;

namespace Pfm
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 테마 초기화
            ThemeManager.Initialize();

            // 로그인 필수 - 로그인 성공 시에만 메인 폼 실행
            using (LoginForm loginForm = new LoginForm())
            {
                loginForm.StartPosition = FormStartPosition.CenterScreen;

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
