using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using GIS.Common;

namespace GIS.SpecialGraphic
{
    /// <summary>
    /// ���ݵ��ߵ���Ƶ��ߵ�����
    /// </summary>
    [Guid("ad63d139-288c-48b5-9637-c5b87e1f8df1")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.SpecialGraphic.DrawTunnels")]
    public class DrawTunnels
    {
        /// <summary>
        /// ���ݵ�������Ƶ�Ҫ��
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="point"></param>
        public void CreatePoint(IFeatureLayer featureLayer, IPoint point, string ID)
        {
            try
            {
                IFeatureClass featureClass = featureLayer.FeatureClass;
                IGeometry geometry = point;

                if (featureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    IDataset dataset = (IDataset)featureClass;
                    IWorkspace workspace = dataset.Workspace;
                    IWorkspaceEdit workspaceEdit = workspace as IWorkspaceEdit;
                    DataEditCommon.CheckEditState();
                    workspaceEdit.StartEditOperation();
                    IFeature feature = featureClass.CreateFeature();

                    DrawCommon.HandleZMValue(feature, geometry);//����ͼ��Zֵ����

                    feature.Shape = point;
                    int iFieldID = feature.Fields.FindField(GIS_Const.FIELD_BID);
                    feature.Value[iFieldID] = ID;
                    feature.Store();
                    workspaceEdit.StopEditOperation();

                    IEnvelope envelop = point.Envelope;
                    DataEditCommon.g_pMyMapCtrl.ActiveView.Extent = envelop;
                    DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, null);
                }
                else
                {
                    MessageBox.Show(@"��ѡ���ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                return;
            }
        }


        /// <summary>
        /// ���ݵ㼯���������Ҫ��
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="lstPoint"></param>
        public void CreateLine(IFeatureLayer featureLayer, List<IPoint> lstPoint, int ID)
        {
            //try
            //{
                IFeatureClass featureClass = featureLayer.FeatureClass;
                if (featureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    IPointCollection multipoint = new MultipointClass();
                    if (lstPoint.Count < 2)
                    {
                        MessageBox.Show(@"��ѡ���������������ϵ�����", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    ISegmentCollection pPath = new PathClass();
                    ILine pLine;
                    ISegment pSegment;
                    object o = Type.Missing;
                    for (int i = 0; i < lstPoint.Count - 1; i++)
                    {
                        pLine = new LineClass();
                        pLine.PutCoords(lstPoint[i], lstPoint[i + 1]);
                        pSegment = pLine as ISegment;
                        pPath.AddSegment(pSegment, ref o, ref o);
                    }
                    IGeometryCollection pPolyline = new PolylineClass();
                    pPolyline.AddGeometry(pPath as IGeometry, ref o, ref o);

                    IDataset dataset = (IDataset)featureClass;
                    IWorkspace workspace = dataset.Workspace;
                    IWorkspaceEdit workspaceEdit = workspace as IWorkspaceEdit;

                    workspaceEdit.StartEditing(true);
                    workspaceEdit.StartEditOperation();

                    IFeature feature = featureClass.CreateFeature();

                    IGeometry geometry = pPolyline as IGeometry;
                    DrawCommon.HandleZMValue(feature, geometry);//����ͼ��Zֵ����

                    feature.Shape = pPolyline as PolylineClass;
                    int iFieldID = feature.Fields.FindField(GIS_Const.FIELD_BID);
                    feature.Value[iFieldID] = ID.ToString();
                    feature.Store();
                    workspaceEdit.StopEditOperation();
                    workspaceEdit.StopEditing(false);

                    IEnvelope envelop = feature.Shape.Envelope;
                    DataEditCommon.g_pMyMapCtrl.ActiveView.Extent = envelop;
                    DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, null);
                }
                else
                {
                    MessageBox.Show(@"��ѡ����ͼ�㡣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            //}
            //catch
            //{
            //    return;
            //}
        }


        /// <summary>
        /// ����Ҫ��ID���Ҷ�ӦҪ��
        /// </summary>
        /// <param name="feaLayer"></param>
        /// <param name="featureID"></param>
        /// <returns></returns>
        public IFeature FindFeatureByID(IFeatureLayer feaLayer, string featureID)
        {
            try
            {
                //����ͼ���ҵ���ӦҪ��
                IFeature pFeature = null;
                IFeatureCursor feaCursor = null;
                feaCursor = feaLayer.FeatureClass.Search(null, true);
                pFeature = feaCursor.NextFeature();
                while (pFeature != null)
                {
                    int iFieldID = pFeature.Fields.FindField("ID");//ͼ���ж�Ӧ��ID�ֶ�
                    string sFieldIDValue = pFeature.get_Value(iFieldID).ToString();

                    //�����ڸ�Ҫ�أ��򷵻ش�Ҫ��
                    if (sFieldIDValue == featureID)
                    {
                        return pFeature;
                    }

                    pFeature = feaCursor.NextFeature();
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ɾ��ָ��Ҫ��
        /// </summary>
        /// <param name="feaLayer"></param>
        /// <param name="featureID"></param>
        public void DeleteFeature(IFeatureLayer feaLayer, string featureID)
        {
            //����1��ɾ��Ҫ��
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = "ID" + "='" + featureID + "'";
            //Get table and row
            ITable esriTable = (ITable)feaLayer.FeatureClass;
            esriTable.DeleteSearchedRows(queryFilter);

            //����2��ɾ��Ҫ��
            ////��ñ༭�����ռ�
            //IDataset pDataset = null;
            //IWorkspace pWorkspace = null;
            //IWorkspaceEdit pWorkspaceEdit = null;
            //pDataset = (IDataset)feaLayer.FeatureClass;
            //pWorkspace = pDataset.Workspace;
            //pWorkspaceEdit = pWorkspace as IWorkspaceEdit;

            ////����ͼ���ҵ���ӦҪ��
            //IFeature pFeature = null;
            //IFeatureCursor feaCursor = null;
            //IQueryFilter queryFilter = new QueryFilterClass();
            //queryFilter.WhereClause = "ID" + "='" + featureID + "'";
            //feaCursor = feaLayer.FeatureClass.Search(queryFilter, true);
            //pFeature = feaCursor.NextFeature();
            //while (pFeature != null)
            //{
            //    //int iFieldID = pFeature.Fields.FindField("ID");//ͼ���ж�Ӧ��ID�ֶ�
            //    //string sFieldIDValue = pFeature.get_Value(iFieldID).ToString();
            //    pFeature.Delete();
            //    ////�����ڸ�Ҫ�أ���ɾ����Ҫ��
            //    //if (sFieldIDValue == featureID)
            //    //{
            //    //    pWorkspaceEdit.StartEditing(false);
            //    //    pWorkspaceEdit.StartEditOperation();
            //    //    pFeature.Delete();
            //    //    pWorkspaceEdit.StopEditOperation();
            //    //    pWorkspaceEdit.StopEditing(true);

            //    //    break;
            //    //}

            //    pFeature = feaCursor.NextFeature();
            //}

            //System.Runtime.InteropServices.Marshal.ReleaseComObject(feaCursor);           

        }
    }
}
