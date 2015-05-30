using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using GIS.Properties;
using GIS.Common;

namespace GIS.GraphicModify
{
    /// <summary>
    /// ɾ��ͼԪ
    /// </summary>
    [Guid("82d6f2d4-3446-4502-83c5-37282e0ed889")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicModify.DeleteFeature")]
    public sealed class DeleteFeature : BaseCommand
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
        public DeleteFeature()
        {
            //�������Զ���
            base.m_category = "�޸�"; 
            base.m_caption = "ɾ��ͼ��";   
            base.m_message = "ɾ����ѡͼ��";  
            base.m_toolTip = "ɾ��ͼ��";  
            base.m_name = "DeleteFeature";   

            try
            {
                base.m_bitmap = Resources.GenericBlackDelete16;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods
        public override bool Enabled
        {
            get
            {
                IFeatureLayer m_featureLayer = DataEditCommon.g_pLayer as IFeatureLayer;
                if (m_featureLayer == null) return false;
                IFeatureSelection m_featureSelection = m_featureLayer as IFeatureSelection;
                ESRI.ArcGIS.Geodatabase.ISelectionSet m_selectionSet = m_featureSelection.SelectionSet;//QI��ISelectionSet
                if (m_selectionSet.Count < 1)
                {
                    return false;
                }
                return true;
            }
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="hook">����ʵ��</param>
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                if (m_hookHelper.ActiveView == null)
                    m_hookHelper = null;
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

        /// <summary>
        /// ����¼�
        /// </summary>
        public override void OnClick()
        {
            IFeature pFeature;
            IEnumFeature pEnumFeature;

            // Get a cursor on selected features
            IFeatureCursor cursor = null;

            pEnumFeature = DataEditCommon.g_pAxMapControl.Map.FeatureSelection as IEnumFeature;

            IFeatureLayer feaLayer = DataEditCommon.g_pLayer as IFeatureLayer;

            //pEnumFeature = DataEditCommon.g_engineEditor.EditSelection;
            //int selectionCnt = DataEditCommon.g_engineEditor.SelectionCount;
            //// û��ѡ���κ�ͼ��
            //if (selectionCnt <= 0)
            //{
            //    return;
            //}

            if(pEnumFeature==null)
            {
                return;
            }
            pEnumFeature.Reset();
            pFeature = pEnumFeature.Next();
            if (pFeature == null)
            {
                System.Windows.Forms.MessageBox.Show("����ѡ��Ҫɾ����ͼԪ��");
                return;
            }
            DataEditCommon.InitEditEnvironment();
            DataEditCommon.CheckEditState();
            DataEditCommon.g_engineEditor.StartOperation();
            //DataEditCommon.g_CurWorkspaceEdit.StartEditOperation();
            do
            {
                int iFieldBID = pFeature.Fields.FindField(GIS_Const.FIELD_OBJECTID);//ͼ���ж�Ӧ��ID�ֶ�
                string sObjId = pFeature.get_Value(iFieldBID).ToString();

                //pFeature.Delete();
                //RefreshModifyFeature((IObject)pFeature);

                DataEditCommon.DeleteFeatureByObjectId(feaLayer, sObjId);
                RefreshModifyFeature((IObject)pFeature);

                pFeature = pEnumFeature.Next();
            }
            while (pFeature != null);
            //DataEditCommon.g_CurWorkspaceEdit.StopEditOperation();
            DataEditCommon.g_engineEditor.StopOperation("Delete Feature");
            DataEditCommon.g_pMap.ClearSelection();
            DataEditCommon.g_pMyMapCtrl.ActiveView.Refresh();
        }

        /// <summary>
        /// ˢ���޸�Ҫ��
        /// </summary>
        /// <param name="pObject"></param>
        private void RefreshModifyFeature(IObject pObject)
        {
            IInvalidArea pRefreshArea;
            pRefreshArea =new InvalidArea();
            pRefreshArea.Display = m_hookHelper.ActiveView.ScreenDisplay;
            pRefreshArea.Add(pObject);
            pRefreshArea.Invalidate(-2);
 
        }

        #endregion
    }
}
