using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pfm.ViewModels.Base
{
    /// <summary>
    /// ViewModel 기본 클래스 - INotifyPropertyChanged 구현
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 속성 변경 알림
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 속성 값 설정 및 변경 알림
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 여러 속성 변경 알림
        /// </summary>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }
        }
    }
}
