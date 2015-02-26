﻿using System;
using System.Globalization;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using GIS;
using GIS.Common;
using LibCommon;
using LibCommonControl;
using LibEntity;

namespace sys3
{
    public partial class CollapsePillarsManagement : SocketHelper
    {
        /// <summary>
        ///     构造方法
        /// </summary>
        public CollapsePillarsManagement()
        {
            InitializeComponent();

            //设置窗体属性
            FormDefaultPropertiesSetter.SetManagementFormDefaultProperties(this, Const_GM.COLLAPSEPILLARE_MANAGEMENT);
        }

        private void RefreshData()
        {
            gcCollapsePillars.DataSource = CollapsePillarsEnt.FindAll();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollapsePillarsManagement_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        /// <summary>
        ///     添加按钮响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnAdd_Click(object sender, EventArgs e)
        {
            var c = new CollapsePillarsEntering();
            if (DialogResult.OK == c.ShowDialog())
            {
                RefreshData();
            }
        }

        /// <summary>
        ///     修改按钮响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnModify_Click(object sender, EventArgs e)
        {
            var c = new CollapsePillarsEntering((CollapsePillarsEnt) gridView1.GetFocusedRow());
            if (DialogResult.OK == c.ShowDialog())
            {
                RefreshData();
            }
        }

        /// <summary>
        ///     删除按钮响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnDel_Click(object sender, EventArgs e)
        {
            if (!Alert.confirm(Const.DEL_CONFIRM_MSG)) return;
            var collapsePillarsEnt = (CollapsePillarsEnt) gridView1.GetFocusedRow();
            DeleteyXLZ(collapsePillarsEnt.Id.ToString(CultureInfo.InvariantCulture));
            collapsePillarsEnt.Delete();
            RefreshData();
        }

        /// <summary>
        ///     刷新按钮响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        /// <summary>
        ///     退出按钮响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnExit_Click(object sender, EventArgs e)
        {
            //关闭窗体
            Close();
        }

        /// <summary>
        ///     导出按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnExport_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                gcCollapsePillars.ExportToXls(saveFileDialog1.FileName);
            }
        }

        /// <summary>
        ///     打印按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnPrint_Click(object sender, EventArgs e)
        {
            DevUtil.DevPrint(gcCollapsePillars, "陷落柱信息报表");
        }

        /// <summary>
        ///     图显按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMap_Click(object sender, EventArgs e)
        {
            ILayer pLayer = DataEditCommon.GetLayerByName(DataEditCommon.g_pMap, LayerNames.LAYER_ALIAS_MR_XianLuoZhu1);
            if (pLayer == null)
            {
                MessageBox.Show(@"未发现陷落柱图层！");
                return;
            }
            var pFeatureLayer = (IFeatureLayer) pLayer;
            string str = "";
            string bid = ((CollapsePillarsEnt) gridView1.GetFocusedRow()).Id.ToString(CultureInfo.InvariantCulture);
            if (bid != "")
            {
                if (true)
                    str = "bid='" + bid + "'";
                //else
                //    str += " or bid='" + bid + "'";
            }
            var list = MyMapHelp.FindFeatureListByWhereClause(pFeatureLayer, str);
            if (list.Count > 0)
            {
                MyMapHelp.Jump(MyMapHelp.GetGeoFromFeature(list));
                DataEditCommon.g_pMap.ClearSelection();
                foreach (var t in list)
                {
                    DataEditCommon.g_pMap.SelectFeature(pLayer, t);
                }
                WindowState = FormWindowState.Normal;
                Location = DataEditCommon.g_axTocControl.Location;
                Width = DataEditCommon.g_axTocControl.Width;
                Height = DataEditCommon.g_axTocControl.Height;
                DataEditCommon.g_pMyMapCtrl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null,
                    DataEditCommon.g_pAxMapControl.Extent);
            }
            else
            {
                Alert.alert("图元丢失");
            }
        }

        #region 删除陷落柱图元

        /// <summary>
        ///     删除陷落柱图元
        /// </summary>
        /// <param name="sCollapseId"></param>
        private void DeleteyXLZ(string sCollapseId)
        {
            //1.获得当前编辑图层
            var drawspecial = new DrawSpecialCommon();
            const string sLayerAliasName = LayerNames.LAYER_ALIAS_MR_XianLuoZhu1; //“默认_陷落柱_1”图层
            IFeatureLayer featureLayer = drawspecial.GetFeatureLayerByName(sLayerAliasName);
            if (featureLayer == null)
            {
                MessageBox.Show(@"未找到" + sLayerAliasName + @"图层,无法删除陷落柱图元。");
                return;
            }

            //2.删除原来图元，重新绘制新图元
            DataEditCommon.DeleteFeatureByBId(featureLayer, sCollapseId);
        }

        #endregion
    }
}