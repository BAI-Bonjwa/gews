using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using GIS.Properties;

namespace GIS.GraphicEdit
{
    /// <summary>
    /// ��ѡ��ѡ����
    /// </summary>
    [Guid("7f50a2ec-c17c-40e6-810b-4e64ebe4c766")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicEdit.FeatureSelect")]
    public sealed class FeatureSelect : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;
        private ICommand m_command = null;
        private IFeatureLayer m_featureLayer = null;

        public FeatureSelect()
        {
            //�������Զ���
            base.m_category = "�༭";
            base.m_caption = "ѡ��ͼԪ";
            base.m_message = "ͨ����������ק����ʽѡ��ͼԪ";
            base.m_toolTip = "ѡ��ͼԪ";
            base.m_name = "FeatureSelect";
            try
            {
                base.m_bitmap = Resources.SelectionSelectTool16;
                //base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.FeatureSelect.cur"));

                m_command = new ESRI.ArcGIS.Controls.ControlsSelectFeaturesToolClass();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods
        public override bool Checked
        {
            get
            {
                if (GIS.Common.DataEditCommon.g_pMyMapCtrl.CurrentTool == (ITool)m_command)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="hook">����ʵ��</param>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                m_command.OnCreate(hook);
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;
        }


        public override void OnClick()
        {
            //ʵ��FeatureSelect.OnClick
            Common.DataEditCommon.InitEditEnvironment();
            Common.DataEditCommon.CheckEditState();
            m_featureLayer = Common.DataEditCommon.g_pLayer as IFeatureLayer;
            if (m_featureLayer == null)
            {
                MessageBox.Show(@"��ѡ��ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Common.DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                return;
            }
            Common.DataEditCommon.g_engineEditLayers.SetTargetLayer(m_featureLayer, 0);
            Common.DataEditCommon.g_pMyMapCtrl.CurrentTool = (ITool)m_command;
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {

        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {

        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {

        }
        #endregion
    }
}
