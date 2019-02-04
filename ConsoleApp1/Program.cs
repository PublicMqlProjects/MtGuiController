using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MtGuiController;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string assembly = "c:\\Users\\Bazil\\source\\repos\\TradePanel\\TradePanel\\bin\\Debug\\TradePanel.dll";
            string FormName = "TradePanelForm";
            GuiController.ShowForm(assembly, FormName);
            GuiController.SendEvent("AskLabel", (int)GuiEventType.TextChange, 0, 0.0, "0.12345");
        }
    }
}
