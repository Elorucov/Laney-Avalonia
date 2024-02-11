using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ELOR.Laney.ViewModels {
    public class ViewModelBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            try {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            } catch (Exception ex) {
                Log.Error(ex, $"A strange error occured in ViewModelBase when property changed, lol! Property name: {propertyName}");
            }
        }
    }
}