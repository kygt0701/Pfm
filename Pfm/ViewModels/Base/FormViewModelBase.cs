using System;
using System.Windows.Input;
using Pfm.Common;

namespace Pfm.ViewModels.Base
{
    /// <summary>
    /// 업무 폼용 ViewModel 기본 클래스
    /// 공통 버튼(신규, 조회, 저장, 삭제) 및 권한 처리
    /// </summary>
    public abstract class FormViewModelBase : ViewModelBase
    {
        #region 속성
        private string _menuId;
        private string _formTitle;
        private bool _isBusy;
        private string _statusMessage;

        /// <summary>
        /// 메뉴 ID (권한 체크용)
        /// </summary>
        public string MenuId
        {
            get => _menuId;
            set => SetProperty(ref _menuId, value);
        }

        /// <summary>
        /// 폼 제목
        /// </summary>
        public string FormTitle
        {
            get => _formTitle;
            set => SetProperty(ref _formTitle, value);
        }

        /// <summary>
        /// 작업 중 여부
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// 상태 메시지
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 현재 메뉴 권한
        /// </summary>
        public MenuPermission Permission { get; private set; }
        #endregion

        #region 권한 속성
        public bool CanCreate => Permission?.CanCreate ?? false;
        public bool CanRead => Permission?.CanRead ?? false;
        public bool CanSave => Permission?.CanSave ?? false;
        public bool CanDelete => Permission?.CanDelete ?? false;
        #endregion

        #region Commands
        public ICommand NewCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        #endregion

        protected FormViewModelBase()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            NewCommand = new RelayCommand(OnNew, () => CanCreate && !IsBusy);
            SearchCommand = new RelayCommand(OnSearch, () => CanRead && !IsBusy);
            SaveCommand = new RelayCommand(OnSave, () => CanSave && !IsBusy);
            DeleteCommand = new RelayCommand(OnDelete, () => CanDelete && !IsBusy);
        }

        /// <summary>
        /// 초기화 (폼 로드 시 호출)
        /// </summary>
        public virtual void Initialize()
        {
            LoadPermission();
            OnPropertiesChanged(nameof(CanCreate), nameof(CanRead), nameof(CanSave), nameof(CanDelete));
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
                Permission = new MenuPermission
                {
                    CanRead = true,
                    CanCreate = true,
                    CanSave = true,
                    CanDelete = true
                };
            }
        }

        #region 추상/가상 메서드
        /// <summary>
        /// 신규
        /// </summary>
        protected abstract void OnNew();

        /// <summary>
        /// 조회
        /// </summary>
        protected abstract void OnSearch();

        /// <summary>
        /// 저장
        /// </summary>
        protected abstract void OnSave();

        /// <summary>
        /// 삭제
        /// </summary>
        protected abstract void OnDelete();
        #endregion

        #region 헬퍼 메서드
        /// <summary>
        /// 작업 시작
        /// </summary>
        protected void BeginWork(string message = "처리 중...")
        {
            IsBusy = true;
            StatusMessage = message;
        }

        /// <summary>
        /// 작업 완료
        /// </summary>
        protected void EndWork(string message = "완료")
        {
            IsBusy = false;
            StatusMessage = message;
        }
        #endregion
    }
}
