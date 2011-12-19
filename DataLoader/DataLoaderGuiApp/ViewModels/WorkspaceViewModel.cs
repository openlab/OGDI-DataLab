using System;
using System.Windows.Input;
using Ogdi.Data.DataLoaderGuiApp.Commands;

namespace Ogdi.Data.DataLoaderGuiApp.ViewModels
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        private RelayCommand _closeCommand;

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => Close(), param => CanClose());
                }
                return _closeCommand;
            }
        }

        public event Action RequestClose;

        public virtual void Close()
        {
            if (RequestClose != null)
            {
                RequestClose();
            }
        }

        public virtual bool CanClose()
        {
            return true;
        }
    }
}