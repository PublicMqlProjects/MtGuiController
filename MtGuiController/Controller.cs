using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Threading;
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
        /// Windows form
        /// </summary>
        private Form m_form = null;
        /// <summary>
        /// 
        /// </summary>
        private bool m_is_closing = false;
        /// <summary>
        /// Global events list
        /// </summary>
        private List<GuiEvent> m_events = null;
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
                id = GuiEventType.ScrollChange,
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
                id = GuiEventType.ClickOnElement,
                el_name = control.Name,
                sparam = control.Text
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a click event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender">Any winform element</param>
        /// <param name="e"></param>
        private void OnChecked(object sender, EventArgs e)
        {
            CheckBox control = (CheckBox)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.CheckBoxChange,
                el_name = control.Name,
                sparam = control.Text,
                lparam = (long)control.CheckState
            };
            m_events.Add(evnt);
        }
        private void OnRadioBtnCheckedChange(object sender, EventArgs e)
        {
            RadioButton control = (RadioButton)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.RadioButtonChange,
                el_name = control.Name,
                sparam = control.Text,
                lparam = control.Checked ? 1 : 0
            };
            m_events.Add(evnt);
        }
        private void OnComboBoxChange(object sender, EventArgs e)
        {
            ComboBox control = (ComboBox)sender;
            string item_text = control.SelectedItem as string;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.ComboBoxChange,
                el_name = control.Name,
                sparam = item_text,
                lparam = control.SelectedIndex
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives the availability of the objec to MetaTrader 
        /// </summary>
        /// <param name="sender">Any winform element</param>
        /// <param name="e"></param>
        private void OnEnableChange(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.ElementEnable,
                el_name = control.Name,
                sparam = control.Text,
                lparam = control.Enabled ? 1 : 0,
            };
            m_events.Add(evnt);
        }

        private void OnNumericChanged(object sender, EventArgs e)
        {
            NumericUpDown numeric = (NumericUpDown)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.NumericChange,
                el_name = numeric.Name,
                dparam = (double)numeric.Value
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDateTimePickerChanged(object sender, EventArgs e)
        {
            DateTimePicker picker = (DateTimePicker)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.DateTimePickerChange,
                el_name = picker.Name,
                lparam = MtConverter.ToMqlDateTime(picker.Value)
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a click event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender">Any winform element</param>
        /// <param name="e"></param>
        private void OnTabChanged(object sender, EventArgs e)
        {
            TabControl control = (TabControl)sender;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.TabIndexChange,
                el_name = control.Name,
                lparam = control.SelectedIndex,
                sparam = control.SelectedTab.Name
            };
            m_events.Add(evnt);
        }
        /// <summary>
        /// This method receives a change text event and sends it's to MetaTrader 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChange(object c, EventArgs e)
        {
            Control control = (Control)c;
            GuiEvent evnt = new GuiEvent
            {
                id = GuiEventType.TextChange,
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
            OnTextChange(sender, e);
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
                OnTextChange(sender, e);
        }
        
        #endregion
        
        /// <summary>
        /// Create new gui controller
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
            form.FormClosing += OnClosingForm;
            //-- define resolve events
            Dictionary<Type, List<HandlerControl>> types_and_events = new Dictionary<Type, List<HandlerControl>>();
            types_and_events.Add(typeof(VScrollBar), new List<HandlerControl>() { vscrol => ((VScrollBar)vscrol).Scroll += OnScroll });
            types_and_events.Add(typeof(Button), new List<HandlerControl>() { button => ((Button)button).Click += OnClick });
            types_and_events.Add(typeof(Label), new List<HandlerControl>());
            types_and_events.Add(typeof(TextBox), new List<HandlerControl>() { text_box => text_box.LostFocus += OnLostFocus,
                                                                               text_box => text_box.KeyDown += OnKeyDown });
            types_and_events.Add(typeof(CheckBox), new List<HandlerControl>() { check_box => ((CheckBox)check_box).CheckStateChanged += OnChecked });
            types_and_events.Add(typeof(TabControl), new List<HandlerControl>() { tab_box => ((TabControl)tab_box).SelectedIndexChanged += OnTabChanged });
            types_and_events.Add(typeof(RadioButton), new List<HandlerControl>() { radio_box => ((RadioButton)radio_box).CheckedChanged += OnRadioBtnCheckedChange });
            types_and_events.Add(typeof(ComboBox), new List<HandlerControl>() { combo_box => ((ComboBox)combo_box).SelectedIndexChanged += OnComboBoxChange,
                                                                                combo_box => ((ComboBox)combo_box).TextChanged += OnTextChange});
            types_and_events.Add(typeof(NumericUpDown), new List<HandlerControl>() { numeric => ((NumericUpDown)numeric).ValueChanged += OnNumericChanged});
            types_and_events.Add(typeof(DateTimePicker), new List<HandlerControl>() { numeric => ((DateTimePicker)numeric).ValueChanged += OnDateTimePickerChanged });
            //-- Recursive subscribe on controls
            SubscribeOnControls(types_and_events, form);
        }
        /// <summary>
        /// Recursive subscribe on controls
        /// </summary>
        /// <param name="control"></param>
        private void SubscribeOnControls(Dictionary<Type, List<HandlerControl>> types_and_events, Control control)
        {
            if (control.GetType() != typeof(NumericUpDown))
            {
                foreach (Control cnt in control.Controls)
                    SubscribeOnControls(types_and_events, cnt);
            }
            try
            {
                m_controls.Add(control.Name, control);
            }
            catch(ArgumentException ex)
            {
                throw new ArgumentException("Ключ с таким именем уже существует", control.Name, ex);
            }
            if (types_and_events.ContainsKey(control.GetType()))
                types_and_events[control.GetType()].ForEach(el => el.Invoke(control));
        }
        /// <summary>
        /// Recursive subscribe on controls
        /// </summary>
        /// <param name="control"></param>
        private void SubscribeOnControls_old(Dictionary<Type, List<HandlerControl>> types_and_events, Control control)
        {
            foreach (Control cnt in control.Controls)
                SubscribeOnControls(types_and_events, cnt);
            if (control.GetType().BaseType == typeof(Form))
                m_controls.Add(control.Name, control);
            control.EnabledChanged += OnEnableChange;
            if (types_and_events.ContainsKey(control.GetType()))
            {
                types_and_events[control.GetType()].ForEach(el => el.Invoke(control));
                m_controls.Add(control.Name, control);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void MessageBoxCreate(string el_name, bool is_blocked, string info)
        {

        }
        /// <summary>
        /// Run form
        /// </summary>
        public void RunForm()
        {
            Thread thread = new Thread(() => Application.Run(m_form));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Thread.Sleep(200);
        }
        #region Dispose
        
        /// <summary>
        /// Dispose form
        /// </summary>
        public void DisposeForm()
        {
            m_form.Dispose();
        }

        /// <summary>
        /// Handler on dispose of form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosingForm(object sender, EventArgs e)
        {
            m_is_closing = true;
            m_controls.Clear();
        }
        /// <summary>
        /// Dispose status
        /// </summary>
        private bool IsDiposed
        {
            get
            {
                if (m_is_closing)
                    return true;
                if (m_form == null)
                    return true;
                return m_form.IsDisposed;
            }
        }
        #endregion
    }
}
