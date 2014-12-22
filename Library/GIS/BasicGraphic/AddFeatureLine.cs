using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using GIS.Properties;
using GIS.Common;
using ESRI.ArcGIS.esriSystem;
using System.Reflection;

namespace GIS.BasicGraphic
{
    /// <summary>
    /// ���ƶ����
    /// </summary>
    [Guid("e7d49585-242c-4e3c-914e-98647d22dab9")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.BasicGraphic.AddFeatureLine")]
    public sealed class AddFeatureLine : BaseTool
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
        private IMapControl3 m_pMapControl;
        private ILayer m_pCurrentLayer;
        private IMap m_pMap;
        private IDisplayFeedback m_pFeedback;
        private bool m_bInUse;
        private IPointCollection m_pPointCollection;
        private IGeometryCollection m_GeometryCollection = null;
        private IArray m_ElementArray = new ArrayClass();
        private bool m_IsFirstPoint;
        public AddFeatureLine()
        {
            //�������Զ���
            base.m_category = "����ͼԪ����"; 
            base.m_caption = "���ƶ��߶�";  
            base.m_message = "���ƶ��߶Σ�˫��������";  
            base.m_toolTip = "���ƶ��߶�";  
            base.m_name = "AddFeatureLine";   
            try
            {
                base.m_bitmap = Resources.ElementLine16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream("Resources.AddFeatureLine.cur"));
                //base.m_cursor = new System.Windows.Forms.Cursor(Application.StartupPath + @"\Resources\AddFeatureLine.cur");
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
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
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
                    if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public override void OnKeyDown(int keyCode, int Shift)
        {
            if (keyCode == (int)Keys.Escape)
            {
                m_IsFirstPoint = true;
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
                m_pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }
        /// <summary>
        /// ����¼�
        /// </summary>
        public override void OnClick()
        {
            m_pMapControl = DataEditCommon.g_pMyMapCtrl;

            DataEditCommon.InitEditEnvironment();
            DataEditCommon.CheckEditState();
            ///��ñ༭Ŀ��ͼ��            
            m_pCurrentLayer = DataEditCommon.g_pLayer;
            IFeatureLayer featureLayer = m_pCurrentLayer as IFeatureLayer;
            if (featureLayer == null)
            {
                MessageBox.Show(@"��ѡ�����ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                return;
            }
            else
            {
                if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                {
                    MessageBox.Show(@"��ѡ����״ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                    return;
                }
            }

            m_pMap = m_hookHelper.FocusMap;
            m_IsFirstPoint = true;
        }
        
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            //����������
            if (Button == 1)
                if (m_IsFirstPoint == true)
                {
                    NewFeatureMouseDown(pMovePt);
                    m_IsFirstPoint = false;
                    
                }
                else
                {
                    NewFeatureMouseDown(pMovePt);
                }           
        }
        public override void OnDblClick()
        {
            m_IsFirstPoint = true;
            NewFeatureEnd();
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            NewFeatureMouseMove(pMovePt);
            DataEditCommon.g_pAxMapControl.Focus();
        }

        /// <summary>
        /// �½�������ӵ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void NewFeatureMouseDown(IPoint pPoint)
        {
            INewPolygonFeedback pPolyFeed;
            INewLineFeedback pLineFeed;
            try
            {
                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                if (pFeatureLayer.FeatureClass == null) return;
                IActiveView pActiveView = (IActiveView)m_pMap;

                /// ������¿�ʼ�����Ķ�������Ӧ�Ĵ���һ���µ�Feedback����
                /// �������Ѵ��ڵ�Feedback�����мӵ�
                if (!m_bInUse)
                {
                    m_pMap.ClearSelection();  //�����ͼѡ�ж���
                    m_bInUse = true;
                    m_pFeedback = new NewLineFeedbackClass();
                    pLineFeed = (INewLineFeedback)m_pFeedback;
                    pLineFeed.Start(pPoint);
                    if (m_pFeedback != null)
                        m_pFeedback.Display = pActiveView.ScreenDisplay;
                }
                else
                {
                    if (m_pFeedback is INewMultiPointFeedback)
                    {
                        object obj = Missing.Value;
                        m_pPointCollection.AddPoint(pPoint, ref obj, ref obj);
                    }
                    else if (m_pFeedback is INewLineFeedback)
                    {
                        pLineFeed = (INewLineFeedback)m_pFeedback;
                        pLineFeed.AddPoint(pPoint);
                    }
                    else if (m_pFeedback is INewPolygonFeedback)
                    {
                        pPolyFeed = (INewPolygonFeedback)m_pFeedback;
                        pPolyFeed.AddPoint(pPoint);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// �½��������������ƶ�����,����TrackЧ��
        /// ��Map.MouseMove�¼��е��ñ�����
        /// </summary>
        /// <param name="x">���X���꣬��Ļ����</param>
        /// <param name="y">���Y���꣬��Ļ����</param>
        public void NewFeatureMouseMove(IPoint pt)
        {
            if ((!m_bInUse) || (m_pFeedback == null)) return;

            m_pFeedback.MoveTo(pt);
        }

        /// <summary>
        /// ����½�����ȡ�û��ƵĶ��󣬲���ӵ�ͼ����
        /// ������Map.DblClick��Map.MouseDown(Button = 2)�¼��е��ñ�����
        /// </summary>
        public void NewFeatureEnd()
        {
            IGeometry pGeom = null;
            IPointCollection pPointCollection;
            object obj = Type.Missing;
            try
            {
                if (m_pFeedback is INewMultiPointFeedback)
                {
                    INewMultiPointFeedback pMPFeed = (INewMultiPointFeedback)m_pFeedback;
                    pMPFeed.Stop();
                    pGeom = (IGeometry)m_pPointCollection;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PointClass() as IGeometryCollection;
                    }

                    m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                }
                else if (m_pFeedback is INewLineFeedback)
                {
                    INewLineFeedback pLineFeed = (INewLineFeedback)m_pFeedback;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PolylineClass() as IGeometryCollection;
                    }

                    IPolyline pPolyLine = pLineFeed.Stop();

                    pPointCollection = (IPointCollection)pPolyLine;
                    if (pPointCollection.PointCount < 2)
                        MessageBox.Show("�������������ڵ�");
                    else
                        pGeom = (IGeometry)pPointCollection;

                    m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                }
                else if (m_pFeedback is INewPolygonFeedback)
                {
                    INewPolygonFeedback pPolyFeed = (INewPolygonFeedback)m_pFeedback;

                    if (m_GeometryCollection == null)
                    {
                        m_GeometryCollection = new PolygonClass() as IGeometryCollection;
                    }

                    IPolygon pPolygon;
                    pPolygon = pPolyFeed.Stop();
                    if (pPolygon != null)
                    {
                        pPointCollection = (IPointCollection)pPolygon;
                        if (pPointCollection.PointCount < 3)
                            MessageBox.Show("�������������ڵ�");
                        else
                            pGeom = (IGeometry)pPointCollection;

                        m_GeometryCollection.AddGeometryCollection(pGeom as IGeometryCollection);
                    }
                }

                CreateFeature(m_GeometryCollection as IGeometry);
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
            }
            catch (Exception e)
            {
                m_pFeedback = null;
                m_bInUse = false;
                m_GeometryCollection = null;
                Console.WriteLine(e.Message.ToString());
            }
        }

        /// <summary>
        /// ���ݵ㴴��Ҫ��
        /// </summary>
        /// <param name="pGeom"></param>
        private void CreateFeature(IGeometry pGeom)
        {
            try
            {
                if (pGeom == null) return;
                if (m_pCurrentLayer == null) return;

                IWorkspaceEdit pWorkspaceEdit = DataEditCommon.g_CurWorkspaceEdit;// GetWorkspaceEdit();
                IFeatureLayer pFeatureLayer = (IFeatureLayer)m_pCurrentLayer;
                IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

                pWorkspaceEdit.StartEditOperation();
                IFeature pFeature = pFeatureClass.CreateFeature();

                // ����Z/Mֵ
                DrawCommon.HandleZMValue(pFeature, pGeom, 0);

                pFeature.Shape = pGeom;
                pFeature.Store();
                pWorkspaceEdit.StopEditOperation();
                m_GeometryCollection = null;
                m_pMap.SelectFeature(m_pCurrentLayer, pFeature);
                m_pMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics | esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
        #endregion
    }
}
