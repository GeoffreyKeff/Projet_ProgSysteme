using GuiApp.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuiApp.ViewModel
{
    interface IObserver
    {
        void Notify(Save save, string str_type);
    }
}
