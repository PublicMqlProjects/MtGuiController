using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

namespace MtGuiController
{
    
    
    /// <summary>
    /// Class contein not-static methods and handlers for abstract windows form
    /// </summary>
    public partial class GuiController
    {
        #region Private fileds
        
        /// <summary>
        /// 
        /// </summary>
        private Assembly m_assembly = null;
        /// <summary>
        /// 
        /// </summary>
        private Form m_form = null;
        /// <summary>
        /// Global events list
        /// </summary>
        private List<GuiEvent> m_events = null;
        /// <summary>
        /// Current run process
        /// </summary>
        private Task m_task = null;
        /// <summary>
        /// Controls collection
        /// </summary>
        private Dictionary<string, Control> m_controls = new Dictionary<string, Control>();
        /// <summary>
        /// Abstract handler of any event of win-form element
        /// </summary>
        /// <param name="control"></param>
        private delegate void HandlerControl(Control control);
        
        #endregion
        #region Handlers of events
        /// <summary>
        /// Handler on dispose of form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisposeForm(object sender, EventArgs e)
        {
            //Form form = (Form)sender;
            
            //m_controls.Clear();
        }
        /// <summary>
        /// This method receives a scroll event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender">ScrollBar element</param>
        /// <param name="e">Params of scroll</param>
        private void OnScroll(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            ScrollEventArgs scroll_args = (ScrollEventArgs)e;
            if (scroll_args.Type != ScrollEventType.SmallIncrement &&
                scroll_args.Type != ScrollEventType.SmallDecrement)
                return;
            GuiEvent evnt = new GuiEvent
            {
                id = (int)GuiEventType.ScrollChange,
                el_name = control.Name,
                lparam = scroll_args.OldValue,
                dparam = scroll_args.NewValue,
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a click event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender">Any winform element</param>
        /// <param name="e"></param>
        private void OnClick(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = (int)GuiEventType.ClickOnElement,
                el_name = control.Name
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a change text event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChange(Control control)
        {
            GuiEvent evnt = new GuiEvent
            {
                id = (int)GuiEventType.TextChange,
                el_name = control.Name,
                sparam = control.Text
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a lost focus event and interprets it as end text changed event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLostFocus(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            OnTextChange(control);
        }
        /// <summary>
        /// This method receives a key down event and if pressed key is enter interprets it as end text changed event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDown(object sender, EventArgs e)
        {
            KeyEventArgs key_args = (KeyEventArgs)e;
            if (key_args.KeyCode == Keys.Enter)
                OnTextChange((Control)sender);
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        private GuiController(Assembly assembly, Form form, List<GuiEvent> global_events_list)
        {
            m_assembly = assembly;
            m_form = form;
            m_events = global_events_list;
            SubscribeOnElements(m_form);
        }
        /// <summary>
        /// Subscribe on supported events
        /// </summary>
        /// <param name="form">Windows form</param>
        private void SubscribeOnElements(Form form)
        {
            Dictionary<Type, List<HandlerControl>> types_and_events = new Dictionary<Type, List<HandlerControl>>();
            types_and_events.Add(typeof(VScrollBar), new List<HandlerControl>() { vscrol => ((VScrollBar)vscrol).Scroll += OnScroll });
            types_and_events.Add(typeof(Button), new List<HandlerControl>()  { button => ((Button)button).Click += OnClick });
            types_and_events.Add(typeof(Label), new List<HandlerControl>());
            types_and_events.Add(typeof(TextBox), new List<HandlerControl>() { text_box => text_box.LostFocus += OnLostFocus, text_box => text_box.KeyDown += OnKeyDown });
            foreach (Control control in form.Controls)
            {
                if (types_and_events.ContainsKey(control.GetType()))
                {
                    types_and_events[control.GetType()].ForEach(el => el.Invoke(control));
                    m_controls.Add(control.Name, control);
                }
            }
        }
        /// <summary>
        /// Run form
        /// </summary>
        public async void RunForm()
        {
            m_task = Task.Run(() => Application.Run(m_form));
            await m_task;
        }
        /// <summary>
        /// Dispose form
        /// </summary>
        public async void DisposeForm()
        {
            m_controls.Clear();
            await Task.Run(() => m_form.Dispose());
        }
    }
}
