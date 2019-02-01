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
        public int id;
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
        /// Пользовательскую форму, вызванную из MetaTrader необходимо запускать в асинхронном режиме,
        /// что бы обеспечить отзывчивость интерфейса.
        /// </summary>
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
        /// После того, как эксперт закончит работу с формой, необходимо завершить процесс ее выполнения.
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
            }
            catch(Exception ex)
            {
                SendExceptionEvent(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="el_name"></param>
        /// <param name="id"></param>
        /// <param name="lparam"></param>
        /// <param name="dparam"></param>
        /// <param name="sparam"></param>
        public static void SendEvent(string el_name, int id, long lparam, double dparam, string sparam)
        {
            foreach(var kvp in m_controllers)
            {
                if (!kvp.Value.m_controls.ContainsKey(el_name))
                    continue;
                Control control = kvp.Value.m_controls[el_name];
                GuiEventType event_type = (GuiEventType)id;
                switch (event_type)
                {
                    case GuiEventType.TextChange:
                        control.Invoke((MethodInvoker)delegate { control.Text = sparam; });
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event_n"></param>
        /// <param name="el_name"></param>
        /// <param name="id"></param>
        /// <param name="lparam"></param>
        /// <param name="dparam"></param>
        /// <param name="sparam"></param>
        public static void GetEvent(int event_n, ref string el_name, ref int id, ref long lparam, ref double dparam, ref string sparam)
        {
            GuiEvent e = m_global_events[event_n];
            el_name = e.el_name;
            id = e.id;
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
