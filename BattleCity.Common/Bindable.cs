using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    public class Bindable : INotifyPropertyChanged  //megmutatja hogy melyik érték változott és mennyivel??
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
        protected void SetProperty<T>(ref T field, T newvalue, [CallerMemberName] string name = null)
        {
            field = newvalue;
            OnPropertyChanged(name);
        }
    }
}
