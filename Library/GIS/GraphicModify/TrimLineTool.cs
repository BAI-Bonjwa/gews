using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace GIS
{
    /// <summary>
    /// ��Ҫ�زü�����
    /// </summary>
    [Guid("1cf085cd-df23-4549-8fca-f680218f5fd3")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicModify.TrimLineTool")]
    public sealed class TrimLineTool : BaseTool
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
            ControlsCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;
        private int m_MouseDownCount;
        private IFeature m_FeatureTrim;
        private IFeature m_FeatureRef;
        private IEngineEditor m_engineEditor = null;
        private IMapControl3 m_pMapControl;
        private IGeometry mGeo = null;//ѡ��Ҫ�ü���ʱ�Ļ���ͼ�Σ����ڴ���ߺ�ɾ��ѡ�е��Ƕ�

        public TrimLineTool()
        {
            base.m_category = "�ü�"; //localizable text 
            base.m_caption = "�ü�";  //localizable text 
            base.m_message = "����ѡ�񹤾�ѡ��ο��ߣ����ñ�����ѡ�д��ü���Ҫ�أ����ɲü�";  //localizable text
            base.m_toolTip = "�ü���";  //localizable text
            base.m_name = "TrimLineTool";   //unique id, non-localizable (e.g. "MyCategory_MyTool")
            try
            {
                m_engineEditor = Activator.CreateInstance<EngineEditorClass>();
                //m_engineEditor = GIS.Common.DataEditCommon.g_engineEditor;
                base.m_bitmap = Properties.Resources.EditingTrimTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "Resources.Cross.cur"));
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
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            m_hookHelper.Hook = hook;

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

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            if (m_hookHelper.FocusMap.SelectionCount != 1)
            {
                MessageBox.Show(@"����ѡ��һ���ο���Ҫ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GIS.GraphicEdit.FeatureSelect featSelectTool = new GraphicEdit.FeatureSelect();
                featSelectTool.OnCreate(m_hookHelper.Hook);
                m_pMapControl.CurrentTool = featSelectTool;

                return;
            }
            m_FeatureRef = GetRefPolyline();  //��ȡѡ��Ĳο���
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button != 1)
                return;

            IPoint pMousePnt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMousePnt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMousePnt);
            IGeometry pgeo = GIS.Common.DataEditCommon.g_pMyMapCtrl.TrackRectangle() as IGeometry;
            if (pgeo.IsEmpty)
                return;
            mGeo = pgeo;
            m_hookHelper.FocusMap.SelectByShape(pgeo, null, false);
            

            m_FeatureTrim = GetTrimLine();
            if (m_FeatureTrim != null)
            {
                if (TrimLine(m_FeatureTrim, m_FeatureRef, pMousePnt))   //��ʼ�ü�,����ϳɹ�����ɾ��ѡ�����
                {
                    m_hookHelper.FocusMap.SelectByShape(pgeo, null, false);
                    m_FeatureTrim = GetTrimLine();
                    if (m_engineEditor != null)
                    {
                        m_engineEditor.StartOperation();
                        m_FeatureTrim.Delete();
                        m_engineEditor.StopOperation("Delete Features");
                    }
                }
            }
            m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection | esriViewDrawPhase.esriViewGeography, null, m_hookHelper.ActiveView.Extent);
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pMovePt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pMovePt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pMovePt);
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {

        }
        #endregion


        /// <summary>
        /// ��ȡѡ��Ĳο���
        /// </summary>
        /// <returns></returns>
        private IFeature GetRefPolyline()
        {
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

        //��ȡѡ��Ĵ��ü���
        private IFeature GetTrimLine()
        {
            IEnumFeature enumFeature;
            IFeature feature;

            if (m_FeatureRef == null)
            {
                m_FeatureTrim = null;
                MessageBox.Show(@"����ѡ��ü��ߵĲο���", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                return null;
            }
        }

        /// <summary>
        /// ���ݲο���Ҫ�أ���ѡ�����Ҫ�ؽ��вü�
        /// </summary>
        /// <param name="featTrim">���ü�����Ҫ��</param>
        /// <param name="featRef">�ο���Ҫ��</param>
        /// <returns>�ɹ�����True</returns>
        public bool TrimLine(IFeature featTrim, IFeature featRef, IPoint pMousePnt)
        {
            IFeatureEdit pFeatEdit = null;
            ESRI.ArcGIS.esriSystem.ISet pSet = null;
            IFeature pFeature = null;

            try
            {
                //��û�н����ʱ�����κβ���
                IPolyline pTrimLine = featTrim.ShapeCopy as IPolyline;
                IPointCollection pIntelsectCol = GetIntersection(featRef.ShapeCopy, pTrimLine);
                if (pIntelsectCol == null || pIntelsectCol.PointCount < 1)
                {
                    System.Windows.Forms.MessageBox.Show("�ο�����Ҫ�ü�����û�н��㣬�޷���ɲü���");
                    return false;
                }
                if (pIntelsectCol.PointCount > 1)  //����1������Ҳ������
                {
                    System.Windows.Forms.MessageBox.Show("�ο�����Ҫ�ü����߽������1�����޷���ɲü���");
                    return false;
                }
                IPoint pIntersectPt = pIntelsectCol.Point[0];     //�ü�����ο��ߵĽ���

                pFeatEdit = featTrim as IFeatureEdit;
                
                pSet = pFeatEdit.Split(pIntersectPt);
                pSet.Reset();

                //pFeature = (IFeature)pSet.Next();
                //while (pFeature != null)
                //{
                //    IPolyline5 polylineTemp = pFeature.ShapeCopy as IPolyline5;
                //    IRelationalOperator relaOperator = pMousePnt as IRelationalOperator;
                //    if (relaOperator.Within(polylineTemp))//����������㣬��ɾ��
                //    {
                //        if (m_engineEditor != null)
                //        {
                //            m_engineEditor.StartOperation();
                //            pFeature.Delete();
                //            m_engineEditor.StopOperation("Delete Features");
                //        }
                //    }
                //    pFeature = (IFeature)pSet.Next();
                //}


                //ISegmentCollection trimSegCol = featTrim.ShapeCopy as ISegmentCollection;
                //ISegmentCollection refSegCol = featRef.ShapeCopy as ISegmentCollection;

                ////�õ�ѡ�е��������������segment
                //ISegment seg = null;
                //int indexSegment = GetSelectSegment(featTrim, pMousePnt, ref seg);   
                //if (seg == null)
                //    return false;

                ////�õ���segment�ཻ�����н���
                //ISegmentCollection pLineSegCol = new PolylineClass();
                //pLineSegCol.AddSegment(seg);

                //IPolyline pSegLine = pLineSegCol as IPolyline;



                //IPoint pLeftPnt = null;
                //IPoint pRightPnt = null;
                //GetColseIntersect(pIntelsectCol, pMousePnt, seg, out pLeftPnt, out pRightPnt);
                //if (pLeftPnt == null || pRightPnt == null)
                //    return false;

                //List<IFeature> lstFeats = null;

                //IFeature pNewFeatOne = null;
                //IFeature pNewFeatTwo = null;

                ////�޼�����ʼ��
                //if (indexSegment == 0 && pLeftPnt.X == pLineSegCol.Segment[0].FromPoint.X && pLeftPnt.Y == pLineSegCol.Segment[0].FromPoint.Y)
                //{
                //    lstFeats = new List<IFeature>();

                //    pFeatEdit = featTrim as IFeatureEdit;
                //    pSet = pFeatEdit.Split(pRightPnt);
                //    pSet.Reset();

                //    pFeature = (IFeature)pSet.Next();
                //    while (pFeature != null)
                //    {
                //        lstFeats.Add(pFeature);
                //        pFeature = (IFeature)pSet.Next();
                //    }

                //    pFeature = GetCloseFeature(m_hookHelper.FocusMap, pMousePnt, lstFeats);

                //    m_engineEditor.StartOperation();
                //    pFeature.Delete();
                //    m_engineEditor.StopOperation("Delete Features");

                //    for (int j = 0; j < lstFeats.Count; j++)
                //    {
                //        if (!lstFeats[j].Equals(pFeature))
                //            pNewFeatOne = lstFeats[j];
                //    }
                //}
                ////�޼���ĩ��
                //else if (indexSegment == pLineSegCol.SegmentCount - 1 && pRightPnt.X == pLineSegCol.Segment[pLineSegCol.SegmentCount - 1].ToPoint.X)
                //{
                //    lstFeats = new List<IFeature>();

                //    pFeatEdit = featTrim as IFeatureEdit;
                //    pSet = pFeatEdit.Split(pLeftPnt);
                //    pSet.Reset();

                //    pFeature = (IFeature)pSet.Next();
                //    while (pFeature != null)
                //    {
                //        lstFeats.Add(pFeature);
                //        pFeature = (IFeature)pSet.Next();
                //    }

                //    pFeature = GetCloseFeature(m_hookHelper.FocusMap, pMousePnt, lstFeats);

                //    m_engineEditor.StartOperation();
                //    pFeature.Delete();
                //    m_engineEditor.StopOperation("Delete Features");

                //    for (int j = 0; j < lstFeats.Count; j++)
                //    {
                //        if (!lstFeats[j].Equals(pFeature))
                //            pNewFeatOne = lstFeats[j];
                //    }
                //}
                //else   //�޼�������λ��
                //{
                //    lstFeats = new List<IFeature>();

                //    pFeatEdit = featTrim as IFeatureEdit;
                //    pSet = pFeatEdit.Split(pLeftPnt);
                //    pSet.Reset();

                //    pFeature = (IFeature)pSet.Next();
                //    while (pFeature != null)
                //    {
                //        lstFeats.Add(pFeature);
                //        pFeature = (IFeature)pSet.Next();
                //    }

                //    pFeature = GetCloseFeature(m_hookHelper.FocusMap, pMousePnt, lstFeats);
                //    for (int j = 0; j < lstFeats.Count; j++)
                //    {
                //        if (!lstFeats[j].Equals(pFeature))
                //            pNewFeatOne = lstFeats[j];
                //    }


                //    lstFeats = new List<IFeature>();

                //    pFeatEdit = pFeature as IFeatureEdit;
                //    pSet = pFeatEdit.Split(pRightPnt);
                //    pSet.Reset();

                //    pFeature = (IFeature)pSet.Next();
                //    while (pFeature != null)
                //    {
                //        lstFeats.Add(pFeature);
                //        pFeature = (IFeature)pSet.Next();
                //    }

                //    pFeature = GetCloseFeature(m_hookHelper.FocusMap, pMousePnt, lstFeats);

                //    m_engineEditor.StartOperation();
                //    pFeature.Delete();
                //    m_engineEditor.StopOperation("Delete Features");

                //    for (int j = 0; j < lstFeats.Count; j++)
                //    {
                //        if (!lstFeats[j].Equals(pFeature))
                //            pNewFeatTwo = lstFeats[j];
                //    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Trim Line");
                return false;
            }
        }

        public int GetSelectSegment(IFeature pFeature, IPoint pPoint, ref ISegment pSegment)
        {
            int index = -1;
            pSegment = null;

            if (pFeature == null || pPoint == null)
                return index;

            ISegmentCollection pSegCol = pFeature.ShapeCopy as ISegmentCollection;
            ISegment pSeg;
            double dDistance = 0;
            double dTempDist = 0;

            IPoint pPt1;
            IPoint pPt2;
            ILine tmpLine = null;
            IPoint pProjectPt;

            for (int i = 0; i < pSegCol.SegmentCount; i++)
            {
                pSeg = pSegCol.Segment[i];
                if (pSeg.GeometryType == esriGeometryType.esriGeometryLine)
                {
                    if (dDistance == 0 && pSegment == null)
                    {
                        tmpLine = new LineClass();
                        tmpLine.PutCoords(pSeg.FromPoint, pSeg.ToPoint);
                        //�㵽ֱ�ߵľ���
                        dDistance = Math.Abs(GetPointToLineDistance(pPoint, tmpLine));
                        pSegment = pSeg;
                        index = i;
                    }
                    else
                    {
                        tmpLine.PutCoords(pSeg.FromPoint, pSeg.ToPoint);
                        if (pSeg.FromPoint.X < pSeg.ToPoint.X)
                        {
                            pPt1 = pSeg.FromPoint;
                            pPt2 = pSeg.ToPoint;
                        }
                        else
                        {
                            pPt2 = pSeg.FromPoint;
                            pPt1 = pSeg.ToPoint;
                        }

                        //���ͶӰ�㲻��ֱ���ϣ��մ���ֱ�߲��ܹ���������
                        pProjectPt = GetProjectionPoint(pPoint, tmpLine);
                        dTempDist = Math.Abs(GetPointToLineDistance(pPoint, tmpLine));
                        if (dDistance > dTempDist)
                        {
                            dDistance = dTempDist;
                            pSegment = pSeg;
                            index = i;
                        }
                    }
                }
                else if (pSeg.GeometryType == esriGeometryType.esriGeometryCircularArc)
                {
                    ICircularArc tmpArc = pSeg as ICircularArc;
                    if (dDistance == 0 && pSegment == null)
                    {
                        dDistance = Math.Abs(Math.Sqrt(Math.Pow(pPoint.X - tmpArc.CenterPoint.X, 2) + Math.Pow(pPoint.Y - tmpArc.CenterPoint.Y, 2)));
                        pSegment = pSeg;
                        index = i;
                    }
                    else
                    {
                        dTempDist = Math.Abs(Math.Sqrt(Math.Pow(pPoint.X - tmpArc.CenterPoint.X, 2) + Math.Pow(pPoint.Y - tmpArc.CenterPoint.Y, 2)));
                        if (dDistance > dTempDist)
                        {
                            pSegment = pSeg;
                            index = i;
                        }
                    }
                }
            }
            return index;
        }


        /// <summary>
        /// ����㵽ֱ�ߵľ���
        /// </summary>
        /// <param name="pPoint"></param>
        /// <param name="pLine"></param>
        /// <returns></returns>
        public double GetPointToLineDistance(IPoint pPoint, ILine pLine)
        {
            if (pPoint == null || pLine == null)
                return -1;

            IPoint pProjectPt = GetProjectionPoint(pPoint, pLine);
            if (pProjectPt == null)
                return 0;

            double dDistance = Math.Sqrt(Math.Pow(pProjectPt.X - pPoint.X, 2) + Math.Pow(pProjectPt.Y - pPoint.Y, 2));
            return dDistance;
        }

        //�����֪����ֱ���ϵ�ͶӰ��(����ӵĹ���)
        public IPoint GetProjectionPoint(IPoint pPoint, ILine pLine)
        {
            if (pPoint == null || pLine == null)
                return null;

            double k = GetSlope(pLine.FromPoint, pLine.ToPoint);
            IPoint pPt = new PointClass();
            if (k == -9999)
                pPt.PutCoords(pLine.FromPoint.X, pPoint.Y);
            else if (k == 0)
                pPt.PutCoords(pPoint.X, pLine.FromPoint.Y);
            else
            {
                if ((pPoint.Y - pLine.FromPoint.Y) - (pPoint.X - pLine.FromPoint.X) * k == 0)  //����ֱ����
                    pPt.PutCoords(pPoint.X, pPoint.Y);
                else
                {
                    pPt.X = (pPoint.X + k * k * pLine.FromPoint.X + k * (pPoint.Y - pLine.FromPoint.Y)) / (1 + k * k);
                    pPt.Y = k * (pPt.X - pLine.FromPoint.X) + pLine.FromPoint.Y;
                }
            }

            return pPt;
        }

        //��ֱ�ߵ�б��
        public double GetSlope(IPoint pPt1, IPoint pPt2)
        {
            if (pPt1 == null || pPt2 == null)
                return -8888;

            if (pPt1.X == pPt2.X)        //��ֱ��X��
                return -9999;
            else if (pPt1.Y == pPt2.Y)   //ƽ����X��
                return 0;
            else   //�������
                return (pPt2.Y - pPt1.Y) / (pPt2.X - pPt1.X);
        }

        //�õ������ཻ����Ľ����
        private IPointCollection GetIntersection(IGeometry pGeo, IPolyline pPolyline)
        {
            ITopologicalOperator topoOper = (ITopologicalOperator)pGeo;
            topoOper.Simplify();

            IGeometry pResultGeo = topoOper.Intersect(pPolyline, esriGeometryDimension.esriGeometry0Dimension);
            if (pResultGeo == null)
                return null;

            if (pResultGeo is IPointCollection)
            {
                IPointCollection pPtCol = pResultGeo as IPointCollection;
                if (pPtCol.PointCount > 0)
                    return pPtCol;
                else
                    return null;
            }
            return null;
        }

        /// <summary>
        /// ��������֮��ľ���
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        private double GetTwoPointDistance(IPoint pt1, IPoint pt2)
        {
            double dDist = -1;

            if (pt1 == null || pt2 == null)
                return dDist;
            else
            {
                dDist = Math.Sqrt(Math.Pow(pt2.X - pt1.X, 2) + Math.Pow(pt2.Y - pt1.Y, 2));
                return dDist;
            }
        }


        private void GetColseIntersect(IPointCollection pIntersectCol, IPoint pInputPnt, ISegment pSeg, out IPoint pLeftPnt, out IPoint pRightPnt)
        {
            pLeftPnt = null;
            pRightPnt = null;

            if (pIntersectCol.PointCount < 1)
                return;


            IPoint pIntersectPnt;
            double dNearDist;
            double dCurDist;
            int nIntersectCount = pIntersectCol.PointCount;

            Dictionary<int, IPoint> dictPoints = new Dictionary<int, IPoint>();
            Dictionary<int, double> dictDistance = new Dictionary<int, double>();



            dictPoints.Add(0, pSeg.FromPoint);
            dictDistance.Add(0, 0);

            dNearDist = GetTwoPointDistance(pInputPnt, pSeg.FromPoint);
            dictPoints.Add(1, pInputPnt);
            dictDistance.Add(1, dNearDist);

            for (int i = 0; i < nIntersectCount; i++)
            {
                pIntersectPnt = pIntersectCol.Point[i];
                dCurDist = GetTwoPointDistance(pIntersectPnt, pSeg.FromPoint);

                dictPoints.Add(i + 2, pIntersectPnt);
                dictDistance.Add(i + 2, dCurDist);

                int nLastSwapPos = -1;
                for (int j = i + 1; j < 1; j--)
                {
                    //�������򽻻�����
                    if (dCurDist < dictDistance[j])
                    {
                        IPoint tmpPnt = dictPoints[j];
                        double tmpDist = dictDistance[j];

                        dictPoints[j] = pIntersectPnt;
                        dictDistance[j] = dCurDist;

                        if (nLastSwapPos == -1)
                        {
                            dictPoints[j + 1] = tmpPnt;
                            dictDistance[j + 1] = tmpDist;
                        }
                        else
                        {
                            dictPoints[nLastSwapPos] = tmpPnt;
                            dictDistance[nLastSwapPos] = tmpDist;
                        }
                        nLastSwapPos = j;    //��¼�ϴν�����λ��
                    }
                }
            }

            dictPoints.Add(nIntersectCount + 2, pSeg.ToPoint);
            dictDistance.Add(nIntersectCount + 2, dictDistance[nIntersectCount + 1] + 1);

            int nInputPtPos = 0;
            int k = 0;
            for (k = 0; k < nIntersectCount + 3; k++)
            {
                if (dictPoints[k] == pInputPnt)
                {
                    nInputPtPos = k;
                    break;
                }
            }

            if (k > 0 && k < nIntersectCount + 1)
            {
                pLeftPnt = dictPoints[nInputPtPos - 1];
                pRightPnt = dictPoints[nInputPtPos + 1];
            }

        }

        //�õ���꿿���ĵ���
        public IFeature GetCloseFeature(IMap pMap, IPoint pPoint, List<IFeature> lstFeatures)
        {
            if (pPoint == null || lstFeatures == null || lstFeatures.Count < 1)
                return null;

            IFeature pFeat;
            IGeometry pGeo;
            ICurve pCurve = null;

            IPoint pInPoint = null;
            bool bAsRatio = false;
            IPoint pNearPoint = null;
            double dDistOnCurve = 0;
            double dNearDist = 0;
            bool bRight = false;
            IFeature pNearFeature = null;

            double dSearchDist;
            ISelectionEnvironment pSelEnv = new SelectionEnvironmentClass();
            dSearchDist = Common.DataEditCommon.ConvertPixelDistanceToMapDistance(pMap as IActiveView, (double)pSelEnv.SearchTolerance);

            for (int i = 0; i < lstFeatures.Count; i++)
            {
                pFeat = lstFeatures[i];
                pGeo = pFeat.Shape;
                if (pGeo.GeometryType == esriGeometryType.esriGeometryPolyline)
                {
                    pCurve = new PolylineClass();
                    pCurve = pGeo as IPolyline;
                }
                else if (pGeo.GeometryType == esriGeometryType.esriGeometryPolygon)
                {
                    pCurve = new PolygonClass();
                    pCurve = pGeo as IPolygon;
                }

                if (pCurve == null)
                    return null;

                pCurve.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, pInPoint, bAsRatio, pNearPoint, dDistOnCurve, dNearDist, bRight);
                if (dNearDist < dSearchDist)
                {
                    dSearchDist = dNearDist;
                    pNearFeature = pFeat;
                }
            }

            return pNearFeature;
        }

    }
}
