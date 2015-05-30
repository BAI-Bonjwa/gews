using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using GIS.Properties;
using ESRI.ArcGIS.esriSystem;

namespace GIS
{
    /// <summary>
    /// �������
    /// </summary>
    [Guid("010190b6-f6f7-44d9-acd9-4c7d3ccf5ee9")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicEdit.MeasureDistance")]
    public sealed class MeasureDistance : BaseTool
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
        private INewLineFeedback m_NewLineFeedback = null;
        private IPointCollection m_ptColl; //��¼�ڵ�
        private MeasureResult _MsgInfo = null;
        private IPolyline m_TraceLine = null; //�����Ĺ켣��        //
        private IGroupElement m_Elements = null; //���ڱ�������˹��ܲ���������Element
        private IGroupElement m_TraceElement = null; //���켣��
        private IGroupElement m_VertexElement = null; //���
        private IGroupElement m_LabelElement = null; // ������

        public MeasureDistance()
        {
            //�������Զ���
            base.m_category = "�༭";
            base.m_caption = "��������";
            base.m_message = "��������";
            base.m_toolTip = "��������";
            base.m_name = "MeasureDistance";
            try
            {
                base.m_bitmap = Resources.MeasureTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "Resources." + GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        public MeasureResult MsgInfo
        {
            set
            {
                _MsgInfo = value;
                _MsgInfo.FormClosing += msgInfo_FromClosing;
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

        void Init()
        {
            //��ʼ��
            m_Elements = new GroupElementClass();
            m_TraceElement = new GroupElementClass();
            m_VertexElement = new GroupElementClass();
            m_LabelElement = new GroupElementClass();

            //��ʼ��,����ӵ�GraphicsContainer
            IGraphicsContainer g = m_hookHelper.ActiveView as IGraphicsContainer;
            g.AddElement(m_Elements as IElement, 0);
            g.AddElement(m_TraceElement as IElement, 0);
            g.AddElement(m_VertexElement as IElement, 0);
            g.AddElement(m_LabelElement as IElement, 0);

            //��ӵ�m_Elements��
            g.MoveElementToGroup(m_VertexElement as IElement, m_Elements);
            g.MoveElementToGroup(m_LabelElement as IElement, m_Elements);
            g.MoveElementToGroup(m_TraceElement as IElement, m_Elements);
        }

        public override void OnClick()
        {
            //��ʼ��
            foreach (System.Windows.Forms.Form form in Application.OpenForms)
            {
                if (form.Name.Equals("MeasureResult"))
                {
                    MeasureResult m_form = (MeasureResult)form;
                    m_form.closeauto = true;
                    form.Close();
                    break;
                }
            }
            Init();

            ///20140218 lyf ���������ʾ����            
            if (_MsgInfo == null || _MsgInfo.IsDisposed)
                _MsgInfo = new MeasureResult(m_hookHelper);
            _MsgInfo.Show();
            _MsgInfo.TopMost = true;
            _MsgInfo.tsmiAreaUnit.Visible = false;
        }

        void msgInfo_FromClosing(object sender, FormClosingEventArgs e)
        {
            //DeleteAllElements(m_hookHelper);
            _MsgInfo = null;
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 2)
                return;
            IPoint pt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pt);
            IGraphicsContainer g = m_hookHelper.ActiveView.GraphicsContainer;
            IEnvelope pEnvBounds = null;

            //��ȡ��һ�ι켣�ߵķ�Χ,�Ա�ȷ��ˢ�·�Χ
            try
            {
                if (m_TraceLine != null)
                {
                    m_TraceLine.QueryEnvelope(pEnvBounds);
                    pEnvBounds.Expand(4, 4, true); //���ο�����������4��(����2������),Ŀ����Ϊ�˱�֤�г����ˢ������
                }
                else
                    pEnvBounds = m_hookHelper.ActiveView.Extent;
            }
            catch
            {
                pEnvBounds = m_hookHelper.ActiveView.Extent;
            }

            #region ��������
            if (m_NewLineFeedback == null)
            {
                //�Ƴ�element
                RemoveElements();
                //ˢ��
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                Application.DoEvents();

                m_NewLineFeedback = new NewLineFeedbackClass();
                m_NewLineFeedback.Display = m_hookHelper.ActiveView.ScreenDisplay;
                //�����ȵõ�symbol,������symbol
                ISimpleLineSymbol simpleLineSymbol = m_NewLineFeedback.Symbol as ISimpleLineSymbol;
                simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                simpleLineSymbol.Width = 2;
                simpleLineSymbol.Color = TransColorToAEColor(Color.Blue);

                m_NewLineFeedback.Start(pt);
            }
            else
            {
                m_NewLineFeedback.AddPoint(pt);
            }

            if (m_ptColl == null)
            {
                m_ptColl = new PolylineClass();
            }
            //��¼�ڵ�
            object obj = Type.Missing;
            m_ptColl.AddPoint(pt, ref obj, ref obj);

            #endregion

            #region ���ƽ��

            try
            {
                IElement vertexElement = createElement_x(pt);
                //
                g = m_hookHelper.ActiveView as IGraphicsContainer;

                //g.AddElement(vertexElement, 0);
                //g.MoveElementToGroup(vertexElement, m_VertexElement);

                m_VertexElement.AddElement(vertexElement);
                //ˢ��
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, vertexElement, pEnvBounds);

            }
            catch
            { }
            #endregion

            try
            {
                if (m_ptColl.PointCount >= 2)
                {
                    IPoint fromPt = m_ptColl.get_Point(m_ptColl.PointCount - 2); //�����ڶ�����
                    IPoint toPt = m_ptColl.get_Point(m_ptColl.PointCount - 1); //����һ����
                    ILine line = new LineClass();
                    line.PutCoords(fromPt, toPt);

                    #region ���ƹ켣��

                    try
                    {
                        object missing = Type.Missing;
                        ISegmentCollection segColl = new PolylineClass();
                        segColl.AddSegment(line as ISegment, ref missing, ref missing);
                        IElement traceElement = createElement_x(segColl as IPolyline);
                        //
                        g = m_hookHelper.ActiveView as IGraphicsContainer;

                        //g.AddElement(traceElement, 0);
                        //g.MoveElementToGroup(traceElement, m_TraceElement);

                        m_TraceElement.AddElement(traceElement);

                        m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, traceElement, pEnvBounds);

                    }
                    catch
                    { }
                    #endregion

                    #region ���㵥�ߵĳ���,���������ʾ�ڵ����е�ƫ������
                    try
                    {
                        double angle = line.Angle;
                        if ((angle > (Math.PI / 2) && angle < (Math.PI)) || (angle > -Math.PI && angle < -(Math.PI / 2))) // ����90��С�ڵ���180
                            angle += Math.PI;

                        //��ע��Yֵƫ����
                        double d_OffsetY = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(9);

                        //��ע��

                        double d_CenterX = (fromPt.X + toPt.X) / 2;
                        double d_CenterY = (fromPt.Y + toPt.Y) / 2 + d_OffsetY; //����ƫ��

                        IPoint labelPt = new PointClass();
                        labelPt.PutCoords(d_CenterX, d_CenterY);
                        IUnitConverter unitConverter = new UnitConverterClass();
                        double segmentLength = unitConverter.ConvertUnits(line.Length, _MsgInfo.inUnit.pUnit, _MsgInfo.outUnit.pUnit);
                        ITextElement txtElement = CreateTextElement(segmentLength.ToString("0.00"));

                        IElement labelelement = txtElement as IElement;
                        labelelement.Geometry = labelPt;
                        object oElement = (object)labelelement;

                        //���ݽǶ���ת
                        TransformByRotate(ref oElement, labelPt, angle);

                        ////��ӵ�GraphicsContainer
                        //g.AddElement(labelelement, 0);

                        ////�Ƶ�m_LabelElement����
                        //g.MoveElementToGroup(labelelement, m_LabelElement);

                        //��ӵ���
                        m_LabelElement.AddElement(labelelement);

                        //ˢ��

                        m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, labelelement, pEnvBounds);
                    }
                    catch
                    { }
                    #endregion
                }


            }
            catch
            { }

            _MsgInfo.LineResultChange();
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pt);
            if (m_NewLineFeedback == null)
                return;
            m_NewLineFeedback.MoveTo(pt);

            if (m_ptColl.PointCount == 0)
                return;
            double d_Total = 0;
            double d_segment = 0;

            IPoint lastPt = m_ptColl.get_Point(m_ptColl.PointCount - 1);
            ILine line = new LineClass();
            line.PutCoords(lastPt, pt);
            //�ھ���
            d_segment = line.Length;
            _MsgInfo.Segment = d_segment;
            try
            {
                IPolyline polyline = m_ptColl as IPolyline;
                if (polyline.IsEmpty)
                {

                    d_Total = d_segment;
                }
                else
                {
                    d_Total = polyline.Length + d_segment;
                }

            }
            catch
            {

            }
            //��ֵ���ܳ���
            _MsgInfo.Total = d_Total;

            _MsgInfo.LineResultChange();
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
        }

        public override void OnDblClick()
        {
            if (m_NewLineFeedback == null)
                return;


            //�����������μ���ͼ��ʱ,˫����������
            try
            {
                m_TraceLine = m_NewLineFeedback.Stop();
                if (m_TraceLine == null)
                    return;
            }
            catch
            { }
            finally
            {
                Recycle();
            }
        }
        #endregion

        //����
        public void Recycle()
        {
            m_NewLineFeedback = null;
            m_ptColl.RemovePoints(0, m_ptColl.PointCount);
            m_ptColl = null;
            m_TraceLine = null;//20140218 lyf
            m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_hookHelper.ActiveView.Extent);
        }

        /// <summary>
        /// ������ɾ������Ԫ��
        /// </summary>
        /// <param name="groupElement"></param>
        void RemoveElementFromGroupElement(IGroupElement groupElement)
        {
            if (groupElement == null || groupElement.ElementCount == 0)
                return;
            try
            {
                IGraphicsContainer g = m_hookHelper.ActiveView.GraphicsContainer;
                for (int index = 0; index < groupElement.ElementCount; index++)
                {
                    IElement tmp_Ele = groupElement.get_Element(index);
                    if (tmp_Ele is IGroupElement)
                        RemoveElementFromGroupElement(tmp_Ele as IGroupElement);
                    else
                    {
                        try
                        {
                            groupElement.DeleteElement(tmp_Ele);
                        }
                        catch
                        {

                        }
                        finally
                        {
                            tmp_Ele = null;
                        }
                    }
                }
                //groupElement.ClearElements();
            }
            catch
            { }
            finally
            {
                //ˢ��
                IEnvelope pEnvBounds = null;

                //��ȡ��һ�ι켣�ߵķ�Χ,�Ա�ȷ��ˢ�·�Χ
                try
                {
                    if (m_TraceLine != null)
                    {
                        m_TraceLine.QueryEnvelope(pEnvBounds);
                        pEnvBounds.Expand(4, 4, true); //���ο�����������4��(����2������),Ŀ����Ϊ�˱�֤�г����ˢ������
                    }
                    else
                        pEnvBounds = m_hookHelper.ActiveView.Extent;
                }
                catch
                {
                    pEnvBounds = m_hookHelper.ActiveView.Extent;
                }
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pEnvBounds);
            }
        }
        /// <summary>
        /// �Ƴ��ڵ�,��ע�͹켣��Element
        /// </summary>
        void RemoveElements()
        {
            try
            {

                //ˢ��һ��
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_hookHelper.ActiveView.Extent);
                IGraphicsContainer g = m_hookHelper.ActiveView.GraphicsContainer;
                #region 1-new
                //RemoveElementFromGroupElement(m_Elements);
                #endregion
                #region 2
                m_LabelElement.ClearElements();
                m_VertexElement.ClearElements();
                m_TraceElement.ClearElements();
                #endregion
            }
            catch
            {

            }
            finally
            {

                //ˢ��һ��
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_hookHelper.ActiveView.Extent);
            }
        }
        /// <summary>
        /// ɾ�����������ص�Ԫ��
        /// </summary>
        public void DeleteAllElements(IHookHelper hookHelper)
        {
            //m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            IGraphicsContainer g = hookHelper.ActiveView.GraphicsContainer;

            //RemoveElementFromGroupElement(m_Elements);
            try
            {
                //g.DeleteElement(m_Elements as IElement);
                g.DeleteAllElements();
            }
            catch
            { }
            finally
            {
                m_TraceElement = null;
                m_LabelElement = null;
                m_VertexElement = null;
                m_Elements = null;
                //�����ˢ��һ��
                hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                hookHelper.ActiveView.Refresh();
            }
        }

        /// <summary>
        /// ��ϵͳ��ɫת��ΪIColor
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        ESRI.ArcGIS.Display.IColor TransColorToAEColor(Color color)
        {
            IRgbColor rgb = new RgbColorClass();
            rgb.RGB = color.R + color.G * 256 + color.B * 65536;
            return rgb as IColor;
        }

        /// <summary>
        /// ��ָ���ĽǶ���ת
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="originPt"></param>
        /// <param name="rotate"></param>
        void TransformByRotate(ref object obj, IPoint originPt, double rotate)
        {
            if (obj == null && originPt == null)
                return;
            try
            {
                ITransform2D transform2D = obj as ITransform2D;
                if (transform2D == null)
                    return;
                transform2D.Rotate(originPt, rotate);

            }
            catch
            { }
        }

        #region ����Element
        /// <summary>
        /// ����һ��TextElement
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        ITextElement CreateTextElement(string text)
        {
            //����һ��TextSymbol
            ITextSymbol txtSymbol = new TextSymbolClass();

            //��������
            Font dispFont = new Font("Arial", 10, FontStyle.Regular);
            txtSymbol.Font = (stdole.IFontDisp)ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(dispFont);

            //��������
            txtSymbol.Color = TransColorToAEColor(Color.Red); //��ɫ

            //����һ��TextElement
            ITextElement txtElement = new TextElementClass();
            txtElement.Symbol = txtSymbol;
            txtElement.Text = text;

            return txtElement;
        }

        /// <summary>
        /// ���Ƽ���ͼ��
        /// </summary>
        /// <param name="geoType"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        ESRI.ArcGIS.Carto.IElement createElement_x(ESRI.ArcGIS.Geometry.IGeometry geometry)
        {
            IElement element = null;
            try
            {
                switch (geometry.GeometryType)
                {
                    case esriGeometryType.esriGeometryPolyline://Polyline��
                        ISimpleLineSymbol simpleLineSymbol = m_NewLineFeedback.Symbol as ISimpleLineSymbol;

                        ILineElement lineElement = new LineElementClass();
                        lineElement.Symbol = simpleLineSymbol as ILineSymbol;
                        element = lineElement as IElement;
                        element.Geometry = geometry;
                        break;
                    case esriGeometryType.esriGeometryPoint:
                        //���ý�����
                        IRgbColor pRGB = new RgbColorClass();
                        pRGB.Red = 255;
                        pRGB.Green = 0;
                        pRGB.Blue = 0;

                        ISimpleMarkerSymbol pSimpleMarkSymbol = new SimpleMarkerSymbolClass();
                        pSimpleMarkSymbol.Color = pRGB as IColor;
                        pSimpleMarkSymbol.Size = 2;
                        pSimpleMarkSymbol.Style = esriSimpleMarkerStyle.esriSMSSquare;

                        IMarkerElement pMarkerElement = new MarkerElementClass();
                        pMarkerElement.Symbol = pSimpleMarkSymbol as IMarkerSymbol;
                        element = pMarkerElement as IElement;
                        element.Geometry = geometry as IGeometry;
                        break;
                }
            }
            catch
            { }
            return element;
        }
        #endregion

    }
}
