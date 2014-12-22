using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using GIS.Common;
using GIS.Properties;

namespace GIS.View
{
    /// <summary>
    /// �ֲ���ͼ
    /// </summary>
    [Guid("689684f5-abdd-48ff-9652-802c15253f0e")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.View.LocalView")]
    public sealed class LocalView : BaseTool
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
        private AxMapControl m_axMapControl = null;

        public LocalView()
        {
            //�������Զ���
            base.m_category = "��ͼ"; 
            base.m_caption = "�ֲ���ͼ"; 
            base.m_message = "����ѡ��ֲ���ʾ";
            base.m_toolTip = "�ֲ���ͼ";
            base.m_name = "LocalView";   
            try
            {
                //string bitmapResourceName = GetType().Name + ".bmp";
                //base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                //base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
                base.m_bitmap = Resources.LocalView;  
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

        /// <summary>
        /// ����¼�
        /// </summary>
        public override void OnClick()
        {
            m_axMapControl = DataEditCommon.g_pAxMapControl;
            if (m_axMapControl == null) 
                DataEditCommon.g_pAxMapControl.CurrentTool = null;
        }
        
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        { 
            IGeometry geometry = null;
            IEnvelope pEnv;
            IActiveView pActiveView = m_axMapControl.ActiveView;
            IMap pMap = m_axMapControl.Map;
            pEnv = m_axMapControl.TrackRectangle();
            if (pEnv.IsEmpty == true)
            {
                ESRI.ArcGIS.esriSystem.tagRECT rect;
                rect.bottom = Y+ 5;
                rect.top = Y - 5;
                rect.left = X- 5;
                rect.right = X + 5;
                pActiveView.ScreenDisplay.DisplayTransformation.TransformRect(pEnv, ref rect, 4);
                pEnv.SpatialReference = pActiveView.FocusMap.SpatialReference;
            }
            geometry = pEnv as IGeometry;
            m_axMapControl.Map.SelectByShape(geometry, null, false);

            GetFeaturesByShape(geometry);
                        
            this.m_axMapControl.Map.ClearSelection();
            this.m_axMapControl.ActiveView.Extent = pEnv;
            m_axMapControl.ActiveView.Refresh();//ˢ��
            
        }

        /// <summary>
        /// ��������Χѡ��Ҫ��
        /// </summary>
        /// <param name="pGeometry"></param>
        private void GetFeaturesByShape(IGeometry pGeometry)
        {    
            IMap pMap = m_hookHelper.FocusMap;
            UID puid = new UID();
            puid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";//IFeatureLayer
            IEnumLayer enumLayer = pMap.get_Layers(puid, true);
            enumLayer.Reset();
            ILayer player;
            player = enumLayer.Next();
            IFeatureLayer featureLayer = null;            
            while (player != null)
            {
                featureLayer = player as IFeatureLayer;
                string layerName = featureLayer.Name;
                string feaSelectSet = GetLayerFeatureSelectSet(pGeometry, featureLayer);
               
                //�½� IFeatureLayerDefinition �ӿ�ʵ��
                IFeatureLayerDefinition featureLayerDef = featureLayer as IFeatureLayerDefinition;
                string sWhereClause;
                if (feaSelectSet == "")
                {
                    ///�����ǰͼ����ѡ��Ҫ����������������Ҫ��OID��
                    ///�ø�ͼ������Ҫ�ز��ɼ�
                    string sLyrAllFeatureIDs = GetLayerAllFeatures(featureLayer);
                    if (sLyrAllFeatureIDs == "")
                    {
                        player = enumLayer.Next();
                        continue;
                    }
                    else
                        sWhereClause = featureLayer.FeatureClass.OIDFieldName + " Not In (" + sLyrAllFeatureIDs + " )";
                }
                else
                {
                    sWhereClause = featureLayer.FeatureClass.OIDFieldName + " In (" + feaSelectSet + " )";//����ɸѡ����
                }
                featureLayerDef.DefinitionExpression = sWhereClause;

                player = enumLayer.Next();
            }
        }

        /// <summary>
        /// ���ͼ��������Ҫ��
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <returns>Ҫ��OID��ɵ��ַ���</returns>
        private string GetLayerAllFeatures(IFeatureLayer featureLayer)
        {
            try
            {
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = "";
                if (featureLayer.FeatureClass.FeatureCount(queryFilter) == 0) return "";

                IFeatureCursor pFeatCursor = featureLayer.FeatureClass.Search(null, false);
                IFeature pFeature = pFeatCursor.NextFeature();
                string sLayerFeatureIDS = "";
                while (pFeature != null)
                {
                    string sFeatureID = pFeature.OID.ToString();
                    sLayerFeatureIDS = sLayerFeatureIDS + sFeatureID + ",";

                    pFeature = pFeatCursor.NextFeature();
                }

                sLayerFeatureIDS = sLayerFeatureIDS.TrimEnd(",".ToCharArray());

                if (pFeatCursor != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);

                return sLayerFeatureIDS;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// ���ͼ��ѡ�е�Ҫ�ؼ�
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="featureLayer"></param>
        /// <returns>Ҫ��OID��ɵ��ַ���</returns>
        private string GetLayerFeatureSelectSet(IGeometry geometry, IFeatureLayer featureLayer)
        {
            try
            {
                //�ռ��˹���
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = geometry;
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;//���ÿռ��˹���ϵ
                //���ѡ��Ҫ��
                IFeatureSelection pFSelection = featureLayer as IFeatureSelection;
                pFSelection.SelectFeatures(pSpatialFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                ISelectionSet pSelectionset = pFSelection.SelectionSet;
                ICursor pCursor;
                pSelectionset.Search(null, true, out pCursor);
                IFeatureCursor pFeatCursor = pCursor as IFeatureCursor;
                IFeature pFeature = pFeatCursor.NextFeature();
                List<string> lstFeatureIDs = new List<string>();
                string sLayerFeatureIDS = "";
                while (pFeature != null)
                {                    
                    string sFeatureID = pFeature.OID.ToString();//Ӧ��Ҫ�ص�OID�ֶ�
                    if (!lstFeatureIDs.Contains(sFeatureID))
                    {
                        lstFeatureIDs.Add(sFeatureID);
                        sLayerFeatureIDS = sLayerFeatureIDS + sFeatureID + ",";
                    }

                    pFeature = pFeatCursor.NextFeature();
                }

                sLayerFeatureIDS = sLayerFeatureIDS.TrimEnd(",".ToCharArray());

                if (pCursor != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
                if (pFeatCursor != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatCursor);

                return sLayerFeatureIDS;
            }
            catch
            {
                return "";
            }
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
