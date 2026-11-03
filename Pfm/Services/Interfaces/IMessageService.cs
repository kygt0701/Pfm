namespace Pfm.Services.Interfaces
{
    /// <summary>
    /// 메시지 표시 서비스 인터페이스
    /// </summary>
    public interface IMessageService
    {
        void ShowInfo(string message, string title = "알림");
        void ShowWarning(string message, string title = "경고");
        void ShowError(string message, string title = "오류");
        bool Confirm(string message, string title = "확인");
    }
}
