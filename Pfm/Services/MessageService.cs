using System.Windows.Forms;
using DevExpress.XtraEditors;
using Pfm.Services.Interfaces;

namespace Pfm.Services
{
    /// <summary>
    /// 메시지 표시 서비스 구현
    /// </summary>
    public class MessageService : IMessageService
    {
        public void ShowInfo(string message, string title = "알림")
        {
            XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string message, string title = "경고")
        {
            XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string message, string title = "오류")
        {
            XtraMessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool Confirm(string message, string title = "확인")
        {
            return XtraMessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
