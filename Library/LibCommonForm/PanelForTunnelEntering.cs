﻿// ******************************************************************
// 概  述：巷道选择共同Panel
// 作成者：伍鑫
// 作成日：2014/02/25
// 版本号：1.0
// ******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibBusiness;
using LibCommonForm;
using LibCommon;
using LibCommonControl;

namespace LibCommonForm
{
    public partial class PanelForTunnelEntering : BaseForm
    {
        // 矿井编号
        private static int _iMineId;
        // 水平编号
        private static int _iHorizontalId;
        // 采区编号
        private static int _iMiningAreaId;
        // 工作面编号
        private static int _iWorkingFaceId;

        public static int IWorkingFaceId
        {
            get { return PanelForTunnelEntering._iWorkingFaceId; }
            set { PanelForTunnelEntering._iWorkingFaceId = value; }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public PanelForTunnelEntering(MainFrm mainFrm)
        {
            this.MainForm = mainFrm;
            InitializeComponent();
            // 加载矿井信息
            loadMineName();
        }

        /// <summary>
        /// 带参数的构造方法
        /// </summary>
        /// <param name="intArr">存储所选矿井编号，水平编号，采区编号，工作面编号的数组</param>
        public PanelForTunnelEntering(int[] intArr, MainFrm mainFrm)
        {
            this.MainForm = mainFrm;

            InitializeComponent();
            // 加载矿井信息
            loadMineName();
            // 设置默认
            this.lstMineName.SelectedValue = intArr[0];
            _iMineId = intArr[0];
            // 加载水平信息
            loadHorizontalName();
            // 设置默认
            this.lstHorizontalName.SelectedValue = intArr[1];
            _iHorizontalId = intArr[1];

            // 加载采区信息
            loadMiningAreaName();
            // 设置默认
            this.lstMiningAreaName.SelectedValue = intArr[2];
            _iMiningAreaId = intArr[2];

            // 加载工作面信息
            loadWorkingFaceName();
            // 设置默认
            this.lstWorkingFaceName.SelectedValue = intArr[3];
            _iWorkingFaceId = intArr[3];
        }

        #region 加载矿井信息
        /// <summary>
        /// 加载矿井信息
        /// </summary>
        private void loadMineName()
        {
            DataBindUtil.LoadMineName(lstMineName);
        }
        #endregion

        #region 矿井名称选择事件
        /// <summary>
        /// 矿井名称选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstMineName_MouseUp(object sender, MouseEventArgs e)
        {
            // 矿井编号
            int iMineId = Convert.ToInt32(this.lstMineName.SelectedValue);
            _iMineId = iMineId;

            // 获取水平信息
            DataSet ds = HorizontalBLL.selectHorizontalInfoByMineId(iMineId);

            // 检索件数
            int iSelCnt = ds.Tables[0].Rows.Count;
            // 检索件数 > 0 的场合
            if (iSelCnt > 0)
            {
                // 绑定水平信息
                this.lstHorizontalName.DataSource = ds.Tables[0];
                this.lstHorizontalName.DisplayMember = HorizontalDbConstNames.HORIZONTAL_NAME;
                this.lstHorizontalName.ValueMember = HorizontalDbConstNames.HORIZONTAL_ID;

                this.lstHorizontalName.SelectedIndex = -1;
            }
        }
        #endregion

        #region 加载水平信息
        /// <summary>
        /// 加载水平信息
        /// </summary>
        private void loadHorizontalName()
        {
            this.lstHorizontalName.DataSource = null;
            this.lstMiningAreaName.DataSource = null;
            this.lstWorkingFaceName.DataSource = null;

            if (this.lstMineName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("矿井编号：" + this.lstMineName.SelectedValue.ToString());

                // 矿井编号
                int iMineId = Convert.ToInt32(this.lstMineName.SelectedValue);
                _iMineId = iMineId;

                // 获取水平信息
                DataSet ds = HorizontalBLL.selectHorizontalInfoByMineId(iMineId);

                // 检索件数
                int iSelCnt = ds.Tables[0].Rows.Count;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定水平信息
                    this.lstHorizontalName.DataSource = ds.Tables[0];
                    this.lstHorizontalName.DisplayMember = HorizontalDbConstNames.HORIZONTAL_NAME;
                    this.lstHorizontalName.ValueMember = HorizontalDbConstNames.HORIZONTAL_ID;

                    this.lstHorizontalName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 水平名称选择事件
        /// <summary>
        /// 水平名称选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstHorizontalName_MouseUp(object sender, MouseEventArgs e)
        {
            this.lstMiningAreaName.DataSource = null;
            this.lstWorkingFaceName.DataSource = null;

            if (this.lstHorizontalName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("水平编号：" + this.lstHorizontalName.SelectedValue.ToString());

                // 水平编号
                int iHorizontalId = Convert.ToInt32(this.lstHorizontalName.SelectedValue);
                _iHorizontalId = iHorizontalId;

                // 获取采区信息
                DataSet ds = MiningAreaBLL.selectMiningAreaInfoByHorizontalId(iHorizontalId);

                // 检索件数
                int iSelCnt = ds.Tables[0].Rows.Count;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定采区信息
                    this.lstMiningAreaName.DataSource = ds.Tables[0];
                    this.lstMiningAreaName.DisplayMember = MiningAreaDbConstNames.MININGAREA_NAME;
                    this.lstMiningAreaName.ValueMember = MiningAreaDbConstNames.MININGAREA_ID;

                    this.lstMiningAreaName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 加载采区信息
        /// <summary>
        /// 加载采区信息
        /// </summary>
        private void loadMiningAreaName()
        {
            this.lstMiningAreaName.DataSource = null;
            this.lstWorkingFaceName.DataSource = null;

            if (this.lstHorizontalName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("水平编号：" + this.lstHorizontalName.SelectedValue.ToString());

                // 水平编号
                int iHorizontalId = Convert.ToInt32(this.lstHorizontalName.SelectedValue);
                _iHorizontalId = iHorizontalId;

                // 获取采区信息
                DataSet ds = MiningAreaBLL.selectMiningAreaInfoByHorizontalId(iHorizontalId);

                // 检索件数
                int iSelCnt = ds.Tables[0].Rows.Count;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定采区信息
                    this.lstMiningAreaName.DataSource = ds.Tables[0];
                    this.lstMiningAreaName.DisplayMember = MiningAreaDbConstNames.MININGAREA_NAME;
                    this.lstMiningAreaName.ValueMember = MiningAreaDbConstNames.MININGAREA_ID;

                    this.lstMiningAreaName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 采区名称选择事件
        /// <summary>
        /// 采区名称选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstMiningAreaName_MouseUp(object sender, MouseEventArgs e)
        {
            this.lstWorkingFaceName.DataSource = null;

            if (this.lstMiningAreaName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("采区编号：" + this.lstMiningAreaName.SelectedValue.ToString());

                // 采区编号
                int iMiningAreaId = Convert.ToInt32(this.lstMiningAreaName.SelectedValue);
                _iMiningAreaId = iMiningAreaId;

                // 获取工作面信息
                DataSet ds = WorkingFaceBLL.selectWorkingFaceInfoByMiningAreaId(iMiningAreaId);

                // 检索件数
                int iSelCnt = ds.Tables[0].Rows.Count;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定工作面信息
                    this.lstWorkingFaceName.DataSource = ds.Tables[0];
                    this.lstWorkingFaceName.DisplayMember = WorkingFaceDbConstNames.WORKINGFACE_NAME;
                    this.lstWorkingFaceName.ValueMember = WorkingFaceDbConstNames.WORKINGFACE_ID;

                    this.lstWorkingFaceName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 加载工作面信息
        /// <summary>
        /// 加载工作面信息
        /// </summary>
        private void loadWorkingFaceName()
        {
            this.lstWorkingFaceName.DataSource = null;

            if (this.lstMiningAreaName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("采区编号：" + this.lstMiningAreaName.SelectedValue.ToString());

                // 采区编号
                int iMiningAreaId = Convert.ToInt32(this.lstMiningAreaName.SelectedValue);
                _iMiningAreaId = iMiningAreaId;

                // 获取工作面信息
                DataSet ds = WorkingFaceBLL.selectWorkingFaceInfoByMiningAreaId(iMiningAreaId);

                // 检索件数
                int iSelCnt = ds.Tables[0].Rows.Count;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定工作面信息
                    this.lstWorkingFaceName.DataSource = ds.Tables[0];
                    this.lstWorkingFaceName.DisplayMember = WorkingFaceDbConstNames.WORKINGFACE_NAME;
                    this.lstWorkingFaceName.ValueMember = WorkingFaceDbConstNames.WORKINGFACE_ID;

                    this.lstWorkingFaceName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 工作面名称选择事件
        /// <summary>
        /// 工作面名称选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstWorkingFaceName_MouseUp(object sender, MouseEventArgs e)
        {

            if (this.lstWorkingFaceName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("工作面编号：" + this.lstWorkingFaceName.SelectedValue.ToString());

                // 工作面编号
                int iWorkingFaceId = Convert.ToInt32(this.lstWorkingFaceName.SelectedValue);
                _iWorkingFaceId = iWorkingFaceId;
            }
        }
        #endregion

        /// <summary>
        /// 矿井名称Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMineName_Click(object sender, EventArgs e)
        {
            CommonManagement commonManagement = new CommonManagement(1, 999);
            if (DialogResult.OK == commonManagement.ShowDialog())
            {
                // 绑定矿井信息
                loadMineName();
            }
        }

        /// <summary>
        /// 水平名称Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHorizontalName_Click(object sender, EventArgs e)
        {
            if (this.lstMineName.SelectedItems.Count > 0)
            {
                CommonManagement commonManagement = new CommonManagement(2, _iMineId);
                if (DialogResult.OK == commonManagement.ShowDialog())
                {
                    // 绑定水平信息
                    loadHorizontalName();
                }
            }
            else
            {
                Alert.alert("请先选择所在矿井名称！");
            }
        }

        /// <summary>
        /// 采区名称Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMiningAreaName_Click(object sender, EventArgs e)
        {
            if (this.lstHorizontalName.SelectedItems.Count > 0)
            {
                CommonManagement commonManagement = new CommonManagement(3, _iHorizontalId);
                if (DialogResult.OK == commonManagement.ShowDialog())
                {
                    // 绑定采区信息
                    loadMiningAreaName();
                }
            }
            else
            {
                Alert.alert("请先选择所在水平名称！");
            }
        }

        /// <summary>
        /// 工作面名称Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWorkingFaceName_Click(object sender, EventArgs e)
        {
            if (this.lstMiningAreaName.SelectedItems.Count > 0)
            {
                CommonManagement commonManagement = new CommonManagement(4, _iMiningAreaId);
                if (DialogResult.OK == commonManagement.ShowDialog())
                {
                    // 绑定工作面信息
                    loadWorkingFaceName();
                }
            }
            else
            {
                Alert.alert("请先选择所在采区名称！");
            }
        }
    }
}
