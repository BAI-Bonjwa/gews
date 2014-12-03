using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using GIS.Properties;
using GIS.Common;

namespace GIS
{
    /// <summary>
    /// ����Բ��
    /// </summary>
    [Guid("45de721f-712f-4aba-b902-dea6f4d14953")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.BasicGraphic.AddArc")]
    public sealed class AddArc : BaseTool
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

        private IMap m_pMap;
        private ILayer m_pCurrentLayer;

        private ISimpleLineSymbol m_LineSym;
        private ICircularArc m_pCircleArc;
        private INewLineFeedback m_pLineFeedback;
        private IPoint m_pFirstPoint = null;
        private IPoint m_pSecondPoint = null;
        private int m_lMouseDownCount;
        private bool m_bIsLineFeat;
        private bool m_bCreated;


        public AddArc()
        {
            //�������Զ���
            base.m_category = "����ͼԪ����";
            base.m_caption = "����Բ��";
            base.m_message = "�����������Բ��";
            base.m_toolTip = "����Բ��";
            base.m_name = "AddCircularArc";
            try
            {
                base.m_bitmap = Resources.EditingMidpointArcSegmentTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.AddCircularArc.cur"));
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
        public override void OnKeyDown(int keyCode, int Shift)
        {
            if (keyCode == (int)Keys.Escape)
            {
                m_pLineFeedback = null;
                m_lMouseDownCount = 0;
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
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
                    if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline && featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon || featureLayer.FeatureClass.FeatureType == esriFeatureType.esriFTAnnotation)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public override bool Checked
        {
            get
            {
                return base.Checked;
            }
        }
        /// <summary>
        /// ����¼�
        /// </summary>
        public override void OnClick()
        {
            DataEditCommon.InitEditEnvironment();
            DataEditCommon.CheckEditState();
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
                //if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline)
                //{
                //    MessageBox.Show("��ѡ����״ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                //    return;
                //}
                if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline && featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon || featureLayer.FeatureClass.FeatureType == esriFeatureType.esriFTAnnotation)
                {
                    MessageBox.Show(@"��ѡ����״����״ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DataEditCommon.g_pMyMapCtrl.CurrentTool = null;
                    return;
                }
            }

            if (featureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                m_bIsLineFeat = true;
            else
                m_bIsLineFeat = false;

            m_pMap = m_hookHelper.FocusMap;
            m_lMouseDownCount = 0;
        }
        
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;

            m_bCreated = false;
            m_lMouseDownCount = m_lMouseDownCount + 1;
            if (m_lMouseDownCount == 1)
            {
                m_pFirstPoint = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                m_pFirstPoint = GIS.GraphicEdit.SnapSetting.getSnapPoint(m_pFirstPoint);
                SetNewLineFeedBack();
                m_pLineFeedback.Start(m_pFirstPoint);
            }
            else if (m_lMouseDownCount == 2)
            {
                m_pSecondPoint = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                m_pSecondPoint = GIS.GraphicEdit.SnapSetting.getSnapPoint(m_pSecondPoint);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
            if (m_lMouseDownCount == 1)
            {
                m_pLineFeedback.MoveTo(pMovePt);
            }
            else if (m_lMouseDownCount == 2)
            {
                m_bCreated = false;

                IEnvelope pEnv = new EnvelopeClass();
                pEnv.UpperLeft = m_pFirstPoint;
                pEnv.LowerRight = m_pSecondPoint;

                IConstructCircularArc pEllipArc = new CircularArcClass();
                pEllipArc.ConstructThreePoints(m_pFirstPoint, m_pSecondPoint, pMovePt, true);
                m_pCircleArc = pEllipArc as ICircularArc;
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            }
            DataEditCommon.g_pAxMapControl.Focus();
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;

            if (m_lMouseDownCount > 2)
            {
                m_lMouseDownCount = 0;
                ISegmentCollection pSegColl=null;
                IFeatureLayer featureLayer = m_pCurrentLayer as IFeatureLayer;

                if (featureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)   //������Ҫ��
                {
                    IPolyline pPolyline;
                    pSegColl = new PolylineClass();
                    pSegColl.AddSegment(m_pCircleArc as ISegment);
                    pPolyline = pSegColl as IPolyline;
                    if (pPolyline.Length < 0.001)
                        return;
                    IFeature pFeature = DataEditCommon.CreateUndoRedoFeature(featureLayer, pPolyline);
                    m_hookHelper.FocusMap.SelectFeature(m_pCurrentLayer, pFeature);
                    m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics | esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewBackground, null, null);
                }
                else   //������Ҫ��
                {
                    IPolygon pPolygon;
                    pSegColl = new PolygonClass();
                    pSegColl.AddSegment(m_pCircleArc as ISegment);
                    pPolygon = pSegColl as IPolygon;
                    pPolygon.Close();
                    if (pPolygon.Length < 0.001)
                        return;
                    IFeature pFeature = DataEditCommon.CreateUndoRedoFeature(featureLayer, pPolygon);
                    m_hookHelper.FocusMap.SelectFeature(m_pCurrentLayer, pFeature);
                    m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics | esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewBackground, null, null);
                }

                //�ֲ�ˢ��
                //IInvalidArea pInvalidArea = new InvalidAreaClass();
                //pInvalidArea.Add(pSegColl);
                //pInvalidArea.Display = m_hookHelper.ActiveView.ScreenDisplay;
                //pInvalidArea.Invalidate((short)esriScreenCache.esriAllScreenCaches);

                m_bCreated = true;
            }
        }

        public override void Refresh(int hDC)
        {
            //ˢ��
            if (!m_bCreated)
                DrawArc(m_pCircleArc);
        }

        /// <summary>
        /// ����Բ��
        /// </summary>
        /// <param name="pCircleArc"></param>
        private void DrawArc(ICircularArc pCircleArc)
        {
            if (pCircleArc == null) return;

            ISegmentCollection pSegColl;
            IPolyline pPolyline;
            pSegColl = new PolylineClass();
            pSegColl.AddSegment(pCircleArc as ISegment);
            pPolyline = pSegColl as IPolyline;

            IRgbColor pColor = new RgbColor();
            pColor.Red = 0;
            pColor.Green = 0;
            pColor.Blue = 0;

            m_LineSym = new SimpleLineSymbol();
            m_LineSym.Color = pColor;
            m_LineSym.Width = 1;
            m_LineSym.Style = esriSimpleLineStyle.esriSLSSolid;

            ISymbol pSym = m_LineSym as ISymbol;
            pSym.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            m_pLineFeedback.Symbol = pSym;

            m_hookHelper.ActiveView.ScreenDisplay.StartDrawing(m_hookHelper.ActiveView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
            m_hookHelper.ActiveView.ScreenDisplay.SetSymbol(pSym);
            m_hookHelper.ActiveView.ScreenDisplay.DrawPolyline(pPolyline);
            m_hookHelper.ActiveView.ScreenDisplay.FinishDrawing();
        }

        
        #endregion

        /// <summary>
        /// ���û��ƹ����е��߷���
        /// </summary>
        private void SetNewLineFeedBack()
        {
            m_pLineFeedback = new NewLineFeedback();
            IRgbColor pColor = new RgbColor();
            pColor.Red = 0;
            pColor.Green = 0;
            pColor.Blue = 0;

            m_LineSym = new SimpleLineSymbol();
            m_LineSym.Color = pColor;
            m_LineSym.Width = 1;
            m_LineSym.Style = esriSimpleLineStyle.esriSLSSolid;

            ISymbol pSym = m_LineSym as ISymbol;
            pSym.ROP2 = esriRasterOpCode.esriROPNotXOrPen;
            m_pLineFeedback.Symbol = pSym;

            m_hookHelper.ActiveView.ScreenDisplay.StartDrawing(m_hookHelper.ActiveView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
            m_hookHelper.ActiveView.ScreenDisplay.SetSymbol(pSym);
            m_hookHelper.ActiveView.ScreenDisplay.FinishDrawing();

            m_pLineFeedback.Display = m_hookHelper.ActiveView.ScreenDisplay;
        }
    }
}
