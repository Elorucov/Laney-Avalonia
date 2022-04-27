using System;

namespace ELOR.Laney.Views {
    public interface IMainWindowRightView {
        event EventHandler BackButtonClick;
        void ChangeBackButtonVisibility(bool isVisible);
    }
}