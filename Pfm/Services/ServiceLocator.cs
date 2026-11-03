using Pfm.Services.Interfaces;

namespace Pfm.Services
{
    /// <summary>
    /// 서비스 로케이터 (간단한 DI 컨테이너)
    /// </summary>
    public static class ServiceLocator
    {
        private static IMessageService _messageService;
        private static IDataService _dataService;

        public static IMessageService MessageService => _messageService ?? (_messageService = new MessageService());
        public static IDataService DataService => _dataService ?? (_dataService = new DataService());

        /// <summary>
        /// 서비스 등록 (테스트용)
        /// </summary>
        public static void Register(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public static void Register(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public static void Reset()
        {
            _messageService = null;
            _dataService = null;
        }
    }
}
