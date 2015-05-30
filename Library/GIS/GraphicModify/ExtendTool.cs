using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace GIS
{
    /// <summary>
    /// �߼��༭���ߣ���Ҫ�����죨��ָ������Ҫ�����쵽�ο��ߣ�
    /// </summary>
    [Guid("9733b54d-d92d-4b6e-ba9b-3b88ac205ada")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicModify.ExtendTool")]
    public sealed class ExtendTool : BaseTool
    {
        private IHookHelper m_hookHelper = null;
        private IMapControl3 m_pMapControl;
        private int m_MouseDownCount;
        private IFeature m_FeatureExtend;
        private IFeature m_FeatureRef;
        private IEngineEditor m_engineEditor = null;

        public ExtendTool()
        {
            base.m_category = "GraphicModify"; //localizable text 
            base.m_caption = "������";  //localizable text 
            base.m_message = "��ѡ�вο��ߣ���ѡ�д�������Ҫ�أ����ɽ����������������ο���";  //localizable text
            base.m_toolTip = "������";  //localizable text
            base.m_name = "ExtendTool";   //unique id, non-localizable (e.g. "MyCategory_MyTool")
            try
            {
                base.m_bitmap = Properties.Resources.EditingExtendTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.Cross.cur"));

                m_engineEditor = Activator.CreateInstance<EngineEditorClass>();
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
                return base.Checked;
            }
        }
        public override bool Enabled
        {
            get
            {
                IFeatureLayer m_featureLayer = GIS.Common.DataEditCommon.g_pLayer as IFeatureLayer;
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
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
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

                if (m_hookHelper.Hook is IToolbarControl)
                {
                    IToolbarControl toolbarControl = m_hookHelper.Hook as IToolbarControl;
                    m_pMapControl = toolbarControl.Buddy as IMapControl3;
                }
                else if (m_hookHelper.Hook is IMapControl3)
                {
                    m_pMapControl = m_hookHelper.Hook as IMapControl3;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            m_MouseDownCount = 0;
            if (m_hookHelper.FocusMap.SelectionCount != 1)
            {
                MessageBox.Show(@"����ѡ��һ���ο���Ҫ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            m_FeatureRef = GetRefPolyline();
            //m_hookHelper.FocusMap.ClearSelection();
            //m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, m_hookHelper.ActiveView.Extent);
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            try
            {
                if (Button == 1)
                {
                    IPoint pPnt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                    ISelectionEnvironment pSelEnv = new SelectionEnvironmentClass();
                    pSelEnv.SearchTolerance = 3;
                    pSelEnv.CombinationMethod = esriSelectionResultEnum.esriSelectionResultAdd;  //����µ�ѡ��Ҫ��
                    m_hookHelper.FocusMap.SelectByShape(m_pMapControl.TrackRectangle(), pSelEnv, false);
                    m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                    //GIS.GraphicEdit.FeatureSelect featSelectTool = new GraphicEdit.FeatureSelect();
                    //featSelectTool.OnCreate(m_hookHelper.Hook);
                    //m_pMapControl.CurrentTool = featSelectTool;


                    m_FeatureExtend = GetExtendLine();

                    if (m_FeatureExtend != null && m_FeatureRef != null)
                    {
                        bool bSuccess = ExtendLine(m_FeatureExtend, m_FeatureRef);
                        if (bSuccess)
                        {
                            m_hookHelper.FocusMap.ClearSelection();
                            m_hookHelper.ActiveView.Refresh();
                        }
                        else
                        {
                            MessageBox.Show(@"�޷�����ǰ��ѡ��Ҫ�����쵽�ο���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else if (Button == 2)
                {
                    //switch (m_MouseDownCount)
                    //{
                    //    case 0:
                    //        if (m_hookHelper.FocusMap.SelectionCount == 1)
                    //        {
                    //            m_FeatureRef =  GetRefPolyline();
                    //            m_MouseDownCount = 1;
                    //        }
                    //        break;

                    //    case 1:
                    //        if (m_hookHelper.FocusMap.SelectionCount > 1)
                    //        {
                    //            m_FeatureExtend= GetExtendLine();
                    //            m_MouseDownCount = 2;
                    //        }
                    //        break;

                    //    case 2:
                    //        m_MouseDownCount = 0;

                    //        //if (m_FeatureExtend != null && m_FeatureRef != null)
                    //        //{
                    //        //   bool bSuccess = ExtendLine(m_FeatureExtend, m_FeatureRef);
                    //        //    m_hookHelper.FocusMap.ClearSelection();
                    //        //    m_hookHelper.ActiveView.Refresh();
                    //        //}
                    //        //else
                    //        //{
                    //        //    MessageBox.Show("����ѡ����Ҫ�������Ҫ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //        //}
                    //        break;
                    //}
                }
            }
            catch { }

        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add ExtendTool.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            if (Button == 2)
            {
                //m_FeatureRef = GetRefPolyline();
                //m_FeatureExtend = GetExtendLine();

                //if (m_FeatureExtend != null && m_FeatureRef != null)
                //{
                //    bool bSuccess = ExtendLine(m_FeatureExtend, m_FeatureRef);
                //    if (bSuccess)
                //    {
                //        m_hookHelper.FocusMap.ClearSelection();
                //        m_hookHelper.ActiveView.Refresh();
                //    }
                //    else
                //    {
                //        MessageBox.Show("�޷�����ǰ��ѡ��Ҫ�����쵽�ο���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("��ѡ����Ҫ�������Ҫ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
            }
        }
        #endregion

        /// <summary>
        /// ��ȡѡ��Ĳο���
        /// </summary>
        /// <returns></returns>
        private IFeature GetRefPolyline()
        {
            //if (m_hookHelper.FocusMap.SelectionCount != 1)
            //    return;
            IEnumFeature enumFeature;
            IFeature feature;

            enumFeature = m_hookHelper.ActiveView.Selection as IEnumFeature;
            enumFeature.Reset();
            feature = enumFeature.Next();
            if (feature != null && feature.ShapeCopy.GeometryType == esriGeometryType.esriGeometryPolyline)
                return feature;
            else
                return null;
        }

        //��ȡѡ��Ĵ�������
        private IFeature GetExtendLine()
        {
            IEnumFeature enumFeature;
            IFeature feature;

            if (m_FeatureRef == null)
            {
                m_MouseDownCount = 0;
                m_FeatureExtend = null;
                MessageBox.Show(@"����ѡ�������ߵĲο���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            else
            {
                enumFeature = m_hookHelper.ActiveView.Selection as IEnumFeature;
                enumFeature.Reset();
                feature = enumFeature.Next();
                while (feature != null)
                {
                    if (feature.ShapeCopy.GeometryType == esriGeometryType.esriGeometryPolyline && feature.OID != m_FeatureRef.OID)
                        return feature;
                    feature = enumFeature.Next();
                }
                MessageBox.Show(@"��ѡ����Ҫ�������Ҫ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }


        /// <summary>
        /// ���ݲο���Ҫ�أ���ѡ�����Ҫ�ؽ�������
        /// </summary>
        /// <param name="featExtend">���������Ҫ��</param>
        /// <param name="featRef">�ο���Ҫ��</param>
        /// <returns>�ɹ�����True</returns>
        public bool ExtendLine(IFeature featExtend, IFeature featRef)
        {
            try
            {
                ISegmentCollection extendSegCol = featExtend.ShapeCopy as ISegmentCollection;
                ISegmentCollection refSegCol = featRef.Shape as ISegmentCollection;

                ICurve extendCurve = extendSegCol as ICurve;
                ICurve refCurve = refSegCol as ICurve;

                bool bExtensionPerformed = false;
                IConstructCurve constructCurve = new PolylineClass();
                constructCurve.ConstructExtended(extendCurve, refCurve, 0, ref bExtensionPerformed);

                if (bExtensionPerformed)   //����ɹ�
                {
                    IPolyline resultPolyline = constructCurve as IPolyline;
                    if (resultPolyline != null)
                    {
                        m_engineEditor.StartOperation();

                        featExtend.Shape = resultPolyline as IGeometry;
                        featExtend.Store();

                        m_engineEditor.StopOperation("Extend Features");
                        m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                        m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, resultPolyline.Envelope);

                        return bExtensionPerformed;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Extend Line");
                return false;
            }
        }
    }
}
