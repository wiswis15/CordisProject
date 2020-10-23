using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cordis.Mdsi.Client.Dtos.Controller;
using System.ComponentModel;

namespace Cordis.MdsiClientWrapper.Helpers
{
    public class CordisControllerRecord: INotifyPropertyChanged
    {
        public ControllerRecord ControllerRecord { get; set; }
        public string MachineName
        {
            get
            {
                return _machineName;
            }
            set
            {
                _machineName = value;
                NotifyPropertyChanged("MachineName");
            }
        }

        public override string ToString()
        {
            return MachineName;
        }

        private string _machineName;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
