﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using GIS;
using GIS.Common;
using LibCommon;
using LibEntity;
using LibSocket;

namespace sys3
{
    public partial class FaultageInfoManagement : Form
    {
        public FaultageInfoManagement()
        {
            InitializeComponent();
            // 设置窗体默认属性
            FormDefaultPropertiesSetter.SetManagementFormDefaultProperties(this, Const_GM.MANAGE_FAULTAGE_INFO);
        }

        private void RefreshData()
        {
            gcFaultage.DataSource = Faultage.FindAll();
        }

        /// <summary>
        ///     添加（必须实装）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            var faultageInfoEnteringForm = new FaultageInfoEntering();
            var result = faultageInfoEnteringForm.ShowDialog();

            if (result == DialogResult.OK || result == DialogResult.Cancel)
            {
                RefreshData();
            }
        }

        /// <summary>
        ///     修改（必须实装）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (gridView1.GetFocusedRow() == null)
            {
                Alert.alert("请选择要修改的信息");
                return;
            }
            var faultageInfoEnteringForm = new FaultageInfoEntering((Faultage) gridView1.GetFocusedRow());
            if (faultageInfoEnteringForm.ShowDialog() == DialogResult.OK)
            {
                RefreshData();
            }
        }

        /// <summary>
        ///     删除按钮（必须实装）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Alert.confirm(Const_GM.DEL_CONFIRM_MSG_FAULTAGE)) return;
            //var faultage = (Faultage)gridView1.GetFocusedRow();
            var selectedIndex = gridView1.GetSelectedRows();
            foreach (var faultage in selectedIndex.Select(i => (Faultage) gridView1.GetRow(i)))
            {
                DeleteJLDCByBID(new[] {faultage.BindingId});
                faultage.Delete();
            }
            SendMessengToServer();
            RefreshData();
        }

        /// <summary>
        ///     根据揭露断层绑定ID删除揭露断层图元
        /// </summary>
        /// <param name="sfpFaultageBidArray">要删除揭露断层的绑定ID</param>
        private void DeleteJLDCByBID(ICollection<string> sfpFaultageBidArray)
        {
            if (sfpFaultageBidArray.Count == 0) return;

            //1.获得当前编辑图层
            var drawspecial = new DrawSpecialCommon();
            const string sLayerAliasName = LayerNames.DEFALUT_EXPOSE_FAULTAGE; //“默认_揭露断层”图层
            var featureLayer = drawspecial.GetFeatureLayerByName(sLayerAliasName);
            if (featureLayer == null)
            {
                MessageBox.Show(@"未找到" + sLayerAliasName + @"图层,无法删除揭露断层图元。");
                return;
            }

            //2.删除揭露断层图元
            foreach (var sfpFaultageBid in sfpFaultageBidArray)
            {
                DataEditCommon.DeleteFeatureByBId(featureLayer, sfpFaultageBid);
            }
        }

        /// <summary>
        ///     退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            // 关闭窗口
            Close();
        }

        /// <summary>
        ///     导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnExport_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                gcFaultage.ExportToXls(saveFileDialog1.FileName);
            }
        }

        /// <summary>
        ///     打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnPrint_Click(object sender, EventArgs e)
        {
            DevUtil.DevPrint(gcFaultage, "揭露断层信息报表");
        }

        /// <summary>
        ///     刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            var faultage = (Faultage) gridView1.GetFocusedRow();
            var bid = faultage.BindingId;
            var pLayer = DataEditCommon.GetLayerByName(DataEditCommon.g_pMap, LayerNames.DEFALUT_EXPOSE_FAULTAGE);
            if (pLayer == null)
            {
                MessageBox.Show(@"未发现揭露断层图层！");
                return;
            }
            var pFeatureLayer = (IFeatureLayer) pLayer;
            var str = "";
            //for (int i = 0; i < iSelIdxsArr.Length; i++)
            //{
            if (bid != "")
            {
                if (true)
                    str = "bid='" + bid + "'";
                //else
                //    str += " or bid='" + bid + "'";
            }
            //}
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

        private void SendMessengToServer()
        {
            Log.Debug("更新服务端断层Map------开始");
            // 通知服务端回采进尺已经添加
            var msg = new GeologyMsg(0, 0, "", DateTime.Now, COMMAND_ID.UPDATE_GEOLOG_DATA);
            SocketUtil.SendMsg2Server(msg);
            Log.Debug("服务端断层Map------完成" + msg);
        }

        private void FaultageInfoManagement_Load(object sender, EventArgs e)
        {
            RefreshData();
        }
    }
}