using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using GIS.Properties;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using GIS.Common;
using ESRI.ArcGIS.Geodatabase;

namespace GIS.BasicGraphic
{
    /// <summary>
    /// ���Ƶ�
    /// </summary>
    [Guid("fb25b65a-b37c-4ea8-afa2-e64e31e62da1")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.BasicGraphic.AddPoint")]
    public sealed class AddPoint : BaseTool
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
        private IFeatureLayer m_featureLayer = null;
        
        public AddPoint()
        {
            //�������Զ���
            base.m_category = "����ͼԪ����"; 
            base.m_caption = "���Ƶ�"; 
            base.m_message = "���Ƶ�";  
            base.m_toolTip = "���Ƶ�";  
            base.m_name = "AddPoint";   
            try
            {
                
                base.m_bitmap = Resources.EditingPointTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.AddPoint.cur"));
                //�༭��ͼ����
                //m_command = new ESRI.ArcGIS.Controls.ControlsEditingSketchTool();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

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
        public override bool Enabled
        {
            get
            {
                IFeatureLayer featureLayer = DataEditCommon.g_pLayer as IFeatureLayer;
                if (featureLayer == null)
                {
                    return false;
                }
                else
                {
                    if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// ����¼�
        /// </summary>
        public override void OnClick()
        {
            DataEditCommon.InitEditEnvironment();//��ʼ���༭����
            DataEditCommon.CheckEditState();//���༭״̬�������༭
            ///��ñ༭Ŀ��ͼ��
            m_featureLayer = DataEditCommon.g_pLayer as IFeatureLayer;
            if (m_featureLayer == null)
            {
                MessageBox.Show(@"��ѡ�����ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                return;
            }
            else
            {
                if (m_featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPoint)
                {
                    MessageBox.Show(@"��ѡ���״ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                    return;
                }
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            IFeature pFeature= DataEditCommon.CreateUndoRedoFeature(m_featureLayer, pMovePt);
            m_hookHelper.FocusMap.SelectFeature(m_featureLayer, pFeature);
            m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
        }
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
        }

        #endregion
    }
}
