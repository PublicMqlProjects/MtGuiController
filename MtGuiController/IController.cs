using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace MtGuiController
{
    /// <summary>
    /// Type of gui event
    /// </summary>
    public enum GuiEventType
    {
        Exception,
        ClickOnElement,
        TextChange,
        ScrollChange
    }
    /// <summary>
    /// Container for event
    /// </summary>
    public class GuiEvent
    {
        public string assembly_name;
        public string form_name;
        public string el_name;
        public GuiEventType id;
        public long lparam;
        public double dparam;
        public string sparam;
    }

    public partial class GuiController
    {
        /// <summary>
        /// One controller for each windows form
        /// </summary>
        private static Dictionary<string, GuiController> m_controllers = new Dictionary<string, GuiController>();
        /// <summary>
        /// Events list
        /// </summary>
        private static List<GuiEvent> m_global_events = new List<GuiEvent>();

        #region private methods
        /// <summary>
        /// Create GuiController for windows form
        /// </summary>
        /// <param name="assembly_path">Path to assembly</param>
        /// <param name="form_name">Windows Form's name</param>
        /// <returns></returns>
        private static GuiController GetGuiController(string assembly_path, string form_name)
        {
            Assembly assembly = Assembly.LoadFile(assembly_path);
            Form form = FindForm(assembly, form_name);
            GuiController controller = new GuiController(assembly, form, m_global_events);
            return controller;
        }
        /// <summary>
        /// Find needed form
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <returns></returns>
        private static Form FindForm(Assembly assembly, string form_name)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                //assembly.CreateInstance()
                if (type.BaseType == typeof(Form) && type.Name == form_name)
                {
                    object obj_form = type.Assembly.CreateInstance(type.FullName);
                    return (Form)obj_form;
                }
            }
            throw new Exception("Form with name " + form_name + " in assembly " + assembly.FullName + "  not find");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        private static void SendExceptionEvent(Exception ex)
        {
            GuiEvent ex_event = new GuiEvent()
            {
                id = (int)GuiEventType.Exception,
                sparam = ex.Message
            };
            m_global_events.Add(ex_event);
        }

        #endregion
        #region MetaTrader Interface methods
        /// <summary>
        /// Show Windows form
        /// </summary>
        [STAThread]
        public static void ShowForm(string assembly_path, string form_name)
        {
            try
            {
                GuiController controller = GetGuiController(assembly_path, form_name);
                string full_path = assembly_path + "/" + form_name;
                m_controllers.Add(full_path, controller);
                controller.RunForm();
            }
            catch(Exception e)
            {
                SendExceptionEvent(e);
            }
        }
        
        /// <summary>
        /// Hide Windows Form
        /// </summary>
        public static void HideForm(string assembly_path, string form_name)
        {
            try
            {
                string full_path = assembly_path + "/" + form_name;
                if (!m_controllers.ContainsKey(full_path))
                    return;
                GuiController controller = m_controllers[full_path];
                controller.DisposeForm();
                m_controllers.Remove(full_path);
            }
            catch(Exception ex)
            {
                SendExceptionEvent(ex);
            }
        }

        /// <summary>
        /// Send event
        /// </summary>
        /// <param name="el_name">name of control</param>
        /// <param name="id">Event type</param>
        /// <param name="lparam">long value</param>
        /// <param name="dparam">double value</param>
        /// <param name="sparam">string value</param>
        public static void SendEvent(string el_name, int id, long lparam, double dparam, string sparam)
        {
            try
            {
                foreach (var kvp in m_controllers)
                {
                    GuiController controller = kvp.Value;
                    if (controller.IsDiposed)
                    {
                        m_controllers.Remove(kvp.Key);
                        return;
                    }
                    if (!controller.m_controls.ContainsKey(el_name))
                        continue;
                    Control control = null;
                    if (!controller.m_controls.TryGetValue(el_name, out control))
                        return;
                    GuiEventType event_type = (GuiEventType)id;
                    switch (event_type)
                    {
                        case GuiEventType.TextChange:
                            control.Invoke((MethodInvoker)delegate { control.Text = sparam; });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                SendExceptionEvent(ex);
            }
        }
        /// <summary>
        /// Get event
        /// </summary>
        /// <param name="event_n">Number of event</param>
        /// <param name="el_name">element name</param>
        /// <param name="id">Event type</param>
        /// <param name="lparam">long value</param>
        /// <param name="dparam">double value</param>
        /// <param name="sparam">string value</param>
        public static void GetEvent(int event_n, ref string el_name, ref int id, ref long lparam, ref double dparam, ref string sparam)
        {
            GuiEvent e = m_global_events[event_n];
            el_name = e.el_name;
            id = (int)e.id;
            lparam = e.lparam;
            dparam = e.dparam;
            sparam = e.sparam;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int EventsTotal()
        {
            return m_global_events.Count;
        }
        #endregion
    }

}
