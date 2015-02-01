﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GIS;
using GIS.Common;
using LibBusiness;
using LibCommon;
using LibEntity;

namespace sys3
{
    public partial class PitshaftInfoEntering : Form
    {
        /** 业务逻辑类型：添加/修改  **/
        private readonly string _bllType = "add";
        private readonly int _iPk;
        private Rectangle _PropertyName;

        /// <summary>
        ///     构造方法
        /// </summary>
        public PitshaftInfoEntering()
        {
            InitializeComponent();
            // 设置窗体默认属性
            FormDefaultPropertiesSetter.SetEnteringFormDefaultProperties(this, Const_GM.INSERT_PITSHAFT_INFO);
            // 加载井筒类型信息
            LoadPitshaftTypeInfo();
        }

        /// <summary>
        ///     带参数的构造方法
        /// </summary>
        /// <param name="strPrimaryKey">主键</param>
        /// <param name="strTitle"></param>
        public PitshaftInfoEntering(string strPrimaryKey, string strTitle)
        {
            InitializeComponent();
            // 设置业务类型
            _bllType = "update";
            // 主键
            _iPk = Convert.ToInt32(strPrimaryKey);
            // 设置窗体默认属性
            FormDefaultPropertiesSetter.SetEnteringFormDefaultProperties(this, strTitle);
            // 加载井筒类型信息
            LoadPitshaftTypeInfo();
            // 设置井筒信息
            // 通过主键获取断层信息
            Pitshaft pitshaft = Pitshaft.Find(_iPk);

            if (pitshaft != null)
            {
                // 井筒名称
                txtPitshaftName.Text = pitshaft.PitshaftName;
                // 井筒类型
                cobPitshaftType.SelectedValue = pitshaft.PitshaftType.PitshaftTypeId;
                // 井口标高
                txtWellheadElevation.Text = pitshaft.WellheadElevation.ToString(CultureInfo.InvariantCulture);
                // 井底标高
                txtWellbottomElevation.Text = pitshaft.WellbottomElevation.ToString(CultureInfo.InvariantCulture);
                // 井筒坐标X
                txtPitshaftCoordinateX.Text =
                  pitshaft.PitshaftCoordinateX.ToString(CultureInfo.InvariantCulture);
                // 井筒坐标Y
                txtPitshaftCoordinateY.Text = pitshaft.PitshaftCoordinateY.ToString(CultureInfo.InvariantCulture);
                // 图形坐标X
                txtFigureCoordinateX.Text = pitshaft.FigureCoordinateX.ToString(CultureInfo.InvariantCulture);
                // 图形坐标Y
                txtFigureCoordinateY.Text = pitshaft.FigureCoordinateY.ToString(CultureInfo.InvariantCulture);
                // 图形坐标Z
                txtFigureCoordinateZ.Text = pitshaft.FigureCoordinateZ.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        ///     加载井筒类型信息
        /// </summary>
        private void LoadPitshaftTypeInfo()
        {
            PitshaftType[] pitshaftTypes = PitshaftType.FindAll();

            cobPitshaftType.DataSource = pitshaftTypes;
            cobPitshaftType.DisplayMember = "PitshaftTypeName";
            cobPitshaftType.ValueMember = "PitshaftTypeId";
            cobPitshaftType.SelectedIndex = -1;
        }

        /// <summary>
        ///     提  交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // 验证
            if (!check())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;

            // 创建井筒实体
            var pitshaftEntity = new Pitshaft();
            // 井筒名称
            pitshaftEntity.PitshaftName = txtPitshaftName.Text.Trim();
            // 井筒类型
            int iPitshaftTypeId = 0;
            if (int.TryParse(Convert.ToString(cobPitshaftType.SelectedValue), out iPitshaftTypeId))
            {
                pitshaftEntity.PitshaftType.PitshaftTypeId = iPitshaftTypeId;
            }
            // 井口标高
            double dWellheadElevation = 0;
            if (double.TryParse(txtWellheadElevation.Text.Trim(), out dWellheadElevation))
            {
                pitshaftEntity.WellheadElevation = dWellheadElevation;
            }
            // 井底标高
            double dWellbottomElevation = 0;
            if (double.TryParse(txtWellbottomElevation.Text.Trim(), out dWellbottomElevation))
            {
                pitshaftEntity.WellbottomElevation = dWellbottomElevation;
            }
            // 井筒坐标X
            double dPitshaftCoordinateX = 0;
            if (double.TryParse(txtPitshaftCoordinateX.Text.Trim(), out dPitshaftCoordinateX))
            {
                pitshaftEntity.PitshaftCoordinateX = Math.Round(dPitshaftCoordinateX, 3);
            }
            // 井筒坐标Y
            double dPitshaftCoordinateY = 0;
            if (double.TryParse(txtPitshaftCoordinateY.Text.Trim(), out dPitshaftCoordinateY))
            {
                pitshaftEntity.PitshaftCoordinateY = Math.Round(dPitshaftCoordinateY, 3);
            }
            // 图形坐标X
            double dFigureCoordinateX = 0;
            if (double.TryParse(txtFigureCoordinateX.Text.Trim(), out dFigureCoordinateX))
            {
                pitshaftEntity.FigureCoordinateX = Math.Round(dFigureCoordinateX, 3);
            }
            // 图形坐标Y
            double dFigureCoordinateY = 0;
            if (double.TryParse(txtFigureCoordinateY.Text.Trim(), out dFigureCoordinateY))
            {
                pitshaftEntity.FigureCoordinateY = Math.Round(dFigureCoordinateY, 3);
            }
            // 图形坐标Z
            double dFigureCoordinateZ = 0;
            if (double.TryParse(txtFigureCoordinateZ.Text.Trim(), out dFigureCoordinateZ))
            {
                pitshaftEntity.FigureCoordinateZ = dFigureCoordinateZ;
            }

            bool bResult = false;
            if (_bllType == "add")
            {
                // BID
                pitshaftEntity.BindingId = IDGenerator.NewBindingID();
                pitshaftEntity.Save();

                DrawJingTong(pitshaftEntity);
            }
            else
            {
                // 主键
                pitshaftEntity.PitshaftId = _iPk;
                // 井筒信息修改
                pitshaftEntity.Save();


                //20140428 lyf 
                //获取井筒BID，为后面修改绘制井筒赋值所用
                string sBID = "";
                sBID = Pitshaft.Find(_iPk).BindingId;
                pitshaftEntity.BindingId = sBID;
                //修改图元
                ModifyJingTong(pitshaftEntity);
            }

            // 添加/修改成功的场合
            Close();
        }

        /// <summary>
        ///     取  消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // 关闭窗口
            Close();
        }

        /// <summary>
        ///     验证画面入力数据
        /// </summary>
        /// <returns>验证结果：true 通过验证, false未通过验证</returns>
        private bool check()
        {
            // 判断<井筒名称>是否录入
            if (!Check.isEmpty(txtPitshaftName, Const_GM.PITSHAFT_NAME))
            {
                return false;
            }

            // 判断<井筒名称>是否包含特殊字符
            if (!Check.checkSpecialCharacters(txtPitshaftName, Const_GM.PITSHAFT_NAME))
            {
                return false;
            }

            // 判断井筒名称是否重复
            if (_bllType == "add")
            {
                // 判断井筒名称是否存在
                if (Pitshaft.ExistsByPitshaftName(txtPitshaftName.Text.Trim()))
                {
                    return false;
                }
            }
            else
            {
                /* 修改的时候，首先要获取UI输入的名称到DB中去检索，
                如果检索件数 > 0 并且该断层ID还不是传过来的主键，那么视为输入了已存在的名称 */
                if (Pitshaft.ExistsByPitshaftName(txtPitshaftName.Text.Trim()))
                {
                    txtPitshaftName.BackColor = Const.ERROR_FIELD_COLOR;
                    Alert.alert(Const_GM.PITSHAFT_NAME_EXIST_MSG); // 井筒名称已存在，请重新录入！
                    txtPitshaftName.Focus();
                    return false;
                }
            }

            // 判断<井筒类型>是否选择
            if (!Check.isEmpty(cobPitshaftType, Const_GM.PITSHAFT_TYPE))
            {
                return false;
            }

            // 判断<井口标高>是否录入
            if (!Check.isEmpty(txtWellheadElevation, Const_GM.WELLHEAD_ELEVATION))
            {
                return false;
            }

            // 判断<井口标高>是否为数字
            if (!Check.IsNumeric(txtWellheadElevation, Const_GM.WELLHEAD_ELEVATION))
            {
                return false;
            }

            // 判断<井底标高>是否录入
            if (!Check.isEmpty(txtWellbottomElevation, Const_GM.WELLBOTTOM_ELEVATION))
            {
                return false;
            }

            // 判断<井底标高>是否为数字
            if (!Check.IsNumeric(txtWellbottomElevation, Const_GM.WELLBOTTOM_ELEVATION))
            {
                return false;
            }

            // 判断<井筒坐标X>是否录入
            if (!Check.isEmpty(txtPitshaftCoordinateX, Const_GM.PITSHAFT_COORDINATE_X))
            {
                return false;
            }

            // 判断<井筒坐标X>是否为数字
            if (!Check.IsNumeric(txtPitshaftCoordinateX, Const_GM.PITSHAFT_COORDINATE_X))
            {
                return false;
            }

            // 判断<井筒坐标Y>是否录入
            if (!Check.isEmpty(txtPitshaftCoordinateY, Const_GM.PITSHAFT_COORDINATE_Y))
            {
                return false;
            }

            // 判断<井筒坐标Y>是否为数字
            if (!Check.IsNumeric(txtPitshaftCoordinateY, Const_GM.PITSHAFT_COORDINATE_Y))
            {
                return false;
            }

            // 判断<图形坐标X>是否录入
            if (!Check.isEmpty(txtFigureCoordinateX, Const_GM.FIGURE_COORDINATE_X))
            {
                return false;
            }

            // 判断<图形坐标X>是否为数字
            if (!Check.IsNumeric(txtFigureCoordinateX, Const_GM.FIGURE_COORDINATE_X))
            {
                return false;
            }

            // 判断<图形坐标Y>是否录入
            if (!Check.isEmpty(txtFigureCoordinateY, Const_GM.FIGURE_COORDINATE_Y))
            {
                return false;
            }

            // 判断<图形坐标Y>是否为数字
            if (!Check.IsNumeric(txtFigureCoordinateY, Const_GM.FIGURE_COORDINATE_Y))
            {
                return false;
            }

            // TODO:图形坐标Z暂时设为非必须录入，暂时保留
            //// 判断<图形坐标Z>是否录入
            //if (!Check.isEmpty(this.txtFigureCoordinateZ, Const_GM.FIGURE_COORDINATE_Z))
            //{
            //    return false;
            //}

            // 判断<图形坐标Z>是否为数字
            if (!Check.IsNumeric(txtFigureCoordinateZ, Const_GM.FIGURE_COORDINATE_Z))
            {
                return false;
            }

            //****************************************************

            // 验证通过
            return true;
        }

        private void btnQD_Click(object sender, EventArgs e)
        {
            DataEditCommon.PickUpPoint(txtPitshaftCoordinateX, txtPitshaftCoordinateY);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataEditCommon.PickUpPoint(txtFigureCoordinateX, txtFigureCoordinateY);
        }

        #region 绘制井筒图元

        /// <summary>
        ///     绘制井筒图元
        /// </summary>
        /// <param name="pitshaftEntity"></param>
        private void DrawJingTong(Pitshaft pitshaftEntity)
        {
            ////1.获得当前编辑图层
            //DrawSpecialCommon drawspecial = new DrawSpecialCommon();
            //string sLayerAliasName = LibCommon.LibLayerNames.DEFALUT_JINGTONG;//“默认_井筒”图层
            //IFeatureLayer featureLayer = drawspecial.GetFeatureLayerByName(sLayerAliasName);
            //if (featureLayer == null)
            //{
            //    MessageBox.Show("未找到" + sLayerAliasName + "图层,无法绘制井筒图元。");
            //    return;
            //}

            ////2.绘制井筒
            //double X = Convert.ToDouble(this.txtFigureCoordinateX.Text.ToString());
            //double Y = Convert.ToDouble(this.txtFigureCoordinateY.Text.ToString());
            //IPoint pt = new PointClass();
            //pt.X = X;
            //pt.Y = Y;

            //double dZ = 0;
            //if (!string.IsNullOrEmpty(this.txtFigureCoordinateZ.Text))
            //{
            //    double.TryParse(this.txtFigureCoordinateZ.Text, out dZ);
            //}
            //pt.Z = dZ;

            ////标注内容
            //string strH =(Convert.ToDouble(this.txtWellheadElevation.Text.ToString())+
            //    Convert.ToDouble(this.txtWellbottomElevation.Text.ToString())).ToString();//井口标高+井底标高
            //string strX = SplitStr(this.txtPitshaftCoordinateX.Text.ToString());
            //string strY = SplitStr(this.txtPitshaftCoordinateY.Text.ToString());
            ////string strName =this.cobPitshaftType.SelectedValue.ToString()+"："+
            ////    this.txtPitshaftName.Text.ToString();
            //string strName = this.cobPitshaftType.SelectedValue.ToString() + "：" +
            //   this.txtPitshaftName.Text.ToString();

            //GIS.SpecialGraphic.DrawJT drawJT = new GIS.SpecialGraphic.DrawJT(strX, strY, strH, strName);
            ////dfs
            //DataEditCommon.InitEditEnvironment();
            //DataEditCommon.CheckEditState();
            //DataEditCommon.g_CurWorkspaceEdit.StartEditOperation();
            ////dfe
            //IFeature feature = featureLayer.FeatureClass.CreateFeature();         
            //IGeometry geometry = pt;
            //DataEditCommon.ZMValue(feature, geometry);   //几何图形Z值处理
            ////drawspecial.ZMValue(feature, geometry);//几何图形Z值处理
            //feature.Shape = pt;
            ////要素ID字段赋值（对应属性表中BindingID）
            //int iFieldID = feature.Fields.FindField(GIS_Const.FIELD_BID);
            //feature.Value[iFieldID] = pitshaftEntity.BindingId.ToString();
            //feature.Store();
            //DataEditCommon.g_CurWorkspaceEdit.StopEditOperation();
            //string strValue = feature.get_Value(feature.Fields.FindField(GIS_Const.FIELD_OBJECTID)).ToString();
            //DataEditCommon.SpecialPointRenderer(featureLayer, GIS_Const.FIELD_OBJECTID, strValue, drawJT.m_Bitmap);

            /////3.显示井筒图层
            //if (featureLayer.Visible == false)
            //    featureLayer.Visible = true;

            ////缩放到新增的线要素，并高亮该要素
            //GIS.Common.DataEditCommon.g_pMyMapCtrl.ActiveView.Extent = feature.Shape.Envelope;
            //GIS.Common.DataEditCommon.g_pMyMapCtrl.ActiveView.Extent.Expand(2.5, 2.5, true);
            //GIS.Common.DataEditCommon.g_pMyMapCtrl.Map.SelectFeature(featureLayer, feature);
            //GIS.Common.DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);

            ////DataEditCommon.g_pMyMapCtrl.ActiveView.Refresh();

            double X = Convert.ToDouble(txtFigureCoordinateX.Text);
            double Y = Convert.ToDouble(txtFigureCoordinateY.Text);
            IPoint pt = new PointClass();
            pt.X = X;
            pt.Y = Y;

            double dZ = 0;
            if (!string.IsNullOrEmpty(txtFigureCoordinateZ.Text))
            {
                double.TryParse(txtFigureCoordinateZ.Text, out dZ);
            }
            pt.Z = dZ;
            ILayer pLayer = DataEditCommon.GetLayerByName(DataEditCommon.g_pMap, LayerNames.DEFALUT_JINGTONG);
            if (pLayer == null)
            {
                MessageBox.Show("未找到井筒图层,无法绘制井筒图元。");
                return;
            }
            var pFeatureLayer = (IFeatureLayer)pLayer;
            IGeometry geometry = pt;
            var list = new List<ziduan>();
            list.Add(new ziduan("bid", pitshaftEntity.BindingId));
            list.Add(new ziduan("mc", pitshaftEntity.PitshaftName));
            list.Add(new ziduan("addtime", DateTime.Now.ToString()));
            list.Add(new ziduan("jkgc", pitshaftEntity.WellheadElevation.ToString()));
            list.Add(new ziduan("jdgc", pitshaftEntity.WellbottomElevation.ToString()));
            list.Add(new ziduan("yt", cobPitshaftType.Text));
            list.Add(new ziduan("x", pitshaftEntity.PitshaftCoordinateX.ToString()));
            list.Add(new ziduan("y", pitshaftEntity.PitshaftCoordinateY.ToString()));
            list.Add(new ziduan("h", (pitshaftEntity.WellbottomElevation + pitshaftEntity.WellheadElevation).ToString()));

            IFeature pfeature = DataEditCommon.CreateNewFeature(pFeatureLayer, geometry, list);
            if (pfeature != null)
            {
                MyMapHelp.Jump(pt);
                DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(
                    esriViewDrawPhase.esriViewGeography | esriViewDrawPhase.esriViewForeground, null, null);
            }
        }

        /// <summary>
        ///     修改井筒
        /// </summary>
        /// <param name="pitshaftEntity"></param>
        private void ModifyJingTong(Pitshaft pitshaftEntity)
        {
            //1.获得当前编辑图层
            var drawspecial = new DrawSpecialCommon();
            string sLayerAliasName = LayerNames.DEFALUT_JINGTONG; //“默认_井筒”图层
            IFeatureLayer featureLayer = drawspecial.GetFeatureLayerByName(sLayerAliasName);
            if (featureLayer == null)
            {
                MessageBox.Show("未找到" + sLayerAliasName + "图层,无法修改井筒图元。");
                return;
            }

            //2.删除原来图元，重新绘制新图元
            bool bIsDeleteOldFeature = DataEditCommon.DeleteFeatureByWhereClause(featureLayer,
                "BID='" + pitshaftEntity.BindingId + "'");
            //if (bIsDeleteOldFeature)
            {
                //绘制井筒
                DrawJingTong(pitshaftEntity);
            }
        }

        /// <summary>
        ///     坐标长度处理
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string SplitStr(string str)
        {
            string resultStr;

            if (!str.Contains('.')) return str;

            string[] strArr = str.Split('.');
            if (strArr[1].Length > 3)
            {
                resultStr = strArr[0] + "." + strArr[1].Substring(0, 3);
            }
            else
            {
                resultStr = str;
            }
            return resultStr;
        }

        #endregion
    }
}