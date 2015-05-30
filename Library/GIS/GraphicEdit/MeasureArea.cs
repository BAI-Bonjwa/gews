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

namespace GIS
{
    /// <summary>
    /// �������
    /// </summary>
    [Guid("feab3d27-b02e-4add-af87-223d73e34a15")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicEdit.MeasureArea")]
    public sealed class MeasureArea : BaseTool
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
        private INewPolygonFeedback m_NewPolygonFeedback = null;
        private IPointCollection m_ptCollection; //��¼�ڵ�
        private MeasureResult m_frmMeasureResult = null;
        private IPolygon m_TracePolygon = null; //�����Ĺ켣��        
        private IGroupElement m_Elements = null; //���ڱ�������˹��ܲ���������Element
        private IGroupElement m_TraceElement = null; //���켣��
        private IGroupElement m_VertexElement = null; //���
        private IGroupElement m_LabelElement = null; // ������
        private ISimpleFillSymbol m_simpleFillSymbol = new SimpleFillSymbolClass();

        public MeasureArea()
        {
            //�������Զ���
            base.m_category = "�༭"; 
            base.m_caption = "�������";  
            base.m_message = "�������";  
            base.m_toolTip = "�������"; 
            base.m_name = "MeasureArea";  
            try
            {
                base.m_bitmap = Resources.MeasureAreaTool16;
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), "Resources." + GetType().Name + ".cur");
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
        

        public override void OnClick()
        {
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
            
            if (m_frmMeasureResult == null || m_frmMeasureResult.IsDisposed)
                m_frmMeasureResult = new MeasureResult(m_hookHelper);
            m_frmMeasureResult.Show();
            m_frmMeasureResult.TopMost = true;
            m_frmMeasureResult.tsmiDistanceUnit.Visible = false;
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        void Init()
        {
            //��ʼ��
            m_Elements = new GroupElementClass();
            m_TraceElement = new GroupElementClass();
            m_VertexElement = new GroupElementClass();
            m_LabelElement = new GroupElementClass();

            //��ʼ��,����ӵ�GraphicsContainer
            IGraphicsContainer graphicsContainer = m_hookHelper.ActiveView as IGraphicsContainer;
            graphicsContainer.AddElement(m_Elements as IElement, 0);
            graphicsContainer.AddElement(m_TraceElement as IElement, 0);
            graphicsContainer.AddElement(m_VertexElement as IElement, 0);
            graphicsContainer.AddElement(m_LabelElement as IElement, 0);

            //��ӵ�m_Elements��
            graphicsContainer.MoveElementToGroup(m_VertexElement as IElement, m_Elements);
            graphicsContainer.MoveElementToGroup(m_LabelElement as IElement, m_Elements);
            graphicsContainer.MoveElementToGroup(m_TraceElement as IElement, m_Elements);
        }
        

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {            
            if (Button == 2)
                return;
            IPoint pt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pt);
            IGraphicsContainer graphicContainer = m_hookHelper.ActiveView.GraphicsContainer;
            IEnvelope pEnvBounds = null;

            //��ȡ��һ�ι켣�ߵķ�Χ,�Ա�ȷ��ˢ�·�Χ
            try
            {
                if (m_TracePolygon != null)
                {
                    m_TracePolygon.QueryEnvelope(pEnvBounds);
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
            if (m_NewPolygonFeedback == null)
            {
                //�Ƴ�element
                RemoveElements();
                //ˢ��
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                Application.DoEvents();

                m_NewPolygonFeedback = new NewPolygonFeedbackClass();

                //�����ȵõ�symbol,������symbol
                ISimpleLineSymbol simpleLineSymbol = m_NewPolygonFeedback.Symbol as ISimpleLineSymbol;
                simpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                simpleLineSymbol.Width = 2;
                simpleLineSymbol.Color = TransColorToAEColor(Color.Blue);

                m_simpleFillSymbol.Outline = simpleLineSymbol;

                m_NewPolygonFeedback.Display = m_hookHelper.ActiveView.ScreenDisplay;
                m_NewPolygonFeedback.Start(pt);
            }
            else
            {
                m_NewPolygonFeedback.AddPoint(pt);
            }


            if (m_ptCollection == null)
            {
                m_ptCollection = new PolylineClass();
            }
            //��¼�ڵ�
            object obj = Type.Missing;
            m_ptCollection.AddPoint(pt, ref obj, ref obj);

            #endregion
            
            #region ���ƽ��

            try
            {
                IElement vertexElement = createElement_x(pt);
                //
                graphicContainer = m_hookHelper.ActiveView as IGraphicsContainer;

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
                if (m_ptCollection.PointCount >= 2)
                {
                    IPoint fromPt = m_ptCollection.get_Point(m_ptCollection.PointCount - 2); //�����ڶ�����
                    IPoint toPt = m_ptCollection.get_Point(m_ptCollection.PointCount - 1); //����һ����
                    ILine line = new LineClass();
                    line.PutCoords(fromPt, toPt);

                    #region ���ƹ켣��

                    try
                    {
                        object missing = Type.Missing;
                        ISegmentCollection segColl = new PolylineClass();
                        segColl.AddSegment(line as ISegment, ref missing, ref missing);

                        IPolyline polyline = new PolylineClass();
                        polyline = segColl as IPolyline;
                        IElement traceElement = createElement_x(polyline);   
                   
                        //graphicContainer = m_hookHelper.ActiveView as IGraphicsContainer;
                        m_TraceElement.AddElement(traceElement);

                        m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, traceElement, pEnvBounds);

                    }
                    catch
                    { }
                    #endregion

                    #region ���㵥�ߵĳ���,���������ʾ�ڵ����е�ƫ������
                    //try
                    //{
                    //    double angle = line.Angle;
                    //    if ((angle > (Math.PI / 2) && angle < (Math.PI)) || (angle > -Math.PI && angle < -(Math.PI / 2))) // ����90��С�ڵ���180
                    //        angle += Math.PI;

                    //    //��ע��Yֵƫ����
                    //    double d_OffsetY = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.FromPoints(9);

                    //    //��ע��
                    //    double d_CenterX = (fromPt.X + toPt.X) / 2;
                    //    double d_CenterY = (fromPt.Y + toPt.Y) / 2 + d_OffsetY; //����ƫ��

                    //    IPoint labelPt = new PointClass();
                    //    labelPt.PutCoords(d_CenterX, d_CenterY);

                    //    ITextElement txtElement = CreateTextElement(line.Length.ToString("0.00"));

                    //    IElement labelelement = txtElement as IElement;
                    //    labelelement.Geometry = labelPt;
                    //    object oElement = (object)labelelement;

                    //    //���ݽǶ���ת
                    //    TransformByRotate(ref oElement, labelPt, angle);

                    //    ////��ӵ�GraphicsContainer
                    //    //g.AddElement(labelelement, 0);

                    //    ////�Ƶ�m_LabelElement����
                    //    //g.MoveElementToGroup(labelelement, m_LabelElement);

                    //    //��ӵ���
                    //    m_LabelElement.AddElement(labelelement);

                    //    //ˢ��
                    //    m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, labelelement, pEnvBounds);
                    //}
                    //catch
                    //{ }
                    #endregion
                }
            }
            catch
            { }

            m_frmMeasureResult.PolygonResultChange();
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
                    case esriGeometryType.esriGeometryPolygon:   
                        IFillShapeElement pPolygonElement;
                        ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
                        IPolygon pPolygon = new PolygonClass();
                        IRubberBand pRubberBand = new RubberPolygonClass();
                        ISimpleLineSymbol pSimpleLineSymbol = new SimpleLineSymbolClass();
                        pPolygonElement = new PolygonElementClass();

                        //if (e.button == 1)
                        //{
                        //   pPolygon=(IPolygon) pRubberBand.TrackNew(axMapControl1.ActiveView.ScreenDisplay, null);
                        //}
                        pSimpleLineSymbol.Width = 2;
                        pSimpleLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                        pSimpleLineSymbol.Color = TransColorToAEColor(Color.Blue);

                        //pSimpleFillSymbol.Color = GetRGBColor(11, 200, 145);
                        pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSNull;
                        pSimpleFillSymbol.Outline = pSimpleLineSymbol;
                        IElement pElement = (IElement)pPolygonElement;
                        pElement.Geometry = geometry;
                        pPolygonElement.Symbol = pSimpleFillSymbol;

                        element = (IElement)pPolygonElement;
                        break;
                    case esriGeometryType.esriGeometryPolyline://Polyline��
                        ISimpleLineSymbol simpleLineSymbol = m_NewPolygonFeedback.Symbol as ISimpleLineSymbol;

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

        private IRgbColor GetRGBColor(int red, int green, int blue)
        {
            IRgbColor rGBColor = new RgbColorClass();
            rGBColor.Red = red;
            rGBColor.Green = green;
            rGBColor.Blue = blue;
            return rGBColor;
        }
        #endregion

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

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            IPoint pt = m_hookHelper.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            pt = GIS.GraphicEdit.SnapSetting.getSnapPoint(pt);
            if (m_NewPolygonFeedback == null)
                return;            
            m_NewPolygonFeedback.MoveTo(pt);


            if (m_ptCollection.PointCount == 0)
                return;
            double d_Total = 0;
            double d_segment = 0;
            double d_Area = 0;//20140219 lyf

            IPoint lastPt = m_ptCollection.get_Point(m_ptCollection.PointCount - 1);
            ILine line = new LineClass();
            line.PutCoords(lastPt, pt);
            //�ھ���
            d_segment = line.Length;
            m_frmMeasureResult.Segment = d_segment;

            ///20140219 lyf ��ǰ�����ʼ����룬�����ܳ���
            IPoint firstPt = m_ptCollection.get_Point(0);
            ILine tempLastLine = new LineClass();
            tempLastLine.PutCoords(pt, firstPt);

            try
            {
                IPolyline polyline = m_ptCollection as IPolyline;
                if (polyline.IsEmpty)
                {

                    d_Total = d_segment;
                    d_Area = 0;
                }
                else
                {
                    d_Total = polyline.Length + d_segment+tempLastLine.Length;
                    d_Area = CaculateArea(pt);
                }

            }
            catch
            {

            }

            //��ֵ���ܳ���
            m_frmMeasureResult.Total = d_Total;
            m_frmMeasureResult.Area = d_Area;
            m_frmMeasureResult.PolygonResultChange();
        }
               

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="CurPoint"></param>
        /// <returns></returns>
        private double CaculateArea(IPoint CurPoint)
        {
            IPointCollection tempCollection = new MultipointClass();

            object missing = Type.Missing;
            for (int i = 0; i < m_ptCollection.PointCount; i++)
            {
                IPoint dPoint = m_ptCollection.get_Point(i);
                tempCollection.AddPoint(dPoint, ref  missing, ref  missing);
            }

            tempCollection.AddPoint(CurPoint, ref missing, ref missing);
            tempCollection.AddPoint(tempCollection.get_Point(0), ref missing, ref  missing);
            int Count = tempCollection.PointCount;

            double x1, x2, y1, y2;
            double tempArea = 0.0;
            for (int i = 0; i < Count - 1; i++)
            {
                x1 = Convert.ToDouble(tempCollection.get_Point(i).X);
                y1 = Convert.ToDouble(tempCollection.get_Point(i).Y);
                x2 = Convert.ToDouble(tempCollection.get_Point(i + 1).X);
                y2 = Convert.ToDouble(tempCollection.get_Point(i + 1).Y);
                tempArea += (x1 * y2 - x2 * y1);
            }

            tempArea = Math.Abs(tempArea) / 2;
            return tempArea;

        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            
        }

        public override void OnDblClick()
        {
            if (m_NewPolygonFeedback == null)
                return;

            IEnvelope pEnvBounds = null;
            //��ȡ��һ�ι켣�ߵķ�Χ,�Ա�ȷ��ˢ�·�Χ
            try
            {
                if (m_TracePolygon != null)
                {
                    m_TracePolygon.QueryEnvelope(pEnvBounds);
                    pEnvBounds.Expand(4, 4, true); //���ο�����������4��(����2������),Ŀ����Ϊ�˱�֤�г����ˢ������
                }
                else
                    pEnvBounds = m_hookHelper.ActiveView.Extent;
            }
            catch
            {
                pEnvBounds = m_hookHelper.ActiveView.Extent;
            }

            //�����������μ���ͼ��ʱ,˫����������
            try
            {
                m_TracePolygon = m_NewPolygonFeedback.Stop();
                if (m_TracePolygon == null)
                    return;

                IElement traceElement = createElement_x(m_TracePolygon);
                m_TraceElement.AddElement(traceElement);
                m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, traceElement, pEnvBounds);
                
            }
            catch
            { }
            finally
            {
                Recycle();
            }
        }

        /// <summary>
        /// ���ձ���
        /// </summary>
        public void Recycle()
        {
            m_NewPolygonFeedback = null;
            m_ptCollection.RemovePoints(0, m_ptCollection.PointCount);
            m_ptCollection = null;
            m_TracePolygon = null;//20140218 lyf
            m_hookHelper.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_hookHelper.ActiveView.Extent);
        }

        #endregion
    }
}
