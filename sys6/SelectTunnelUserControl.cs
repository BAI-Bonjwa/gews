﻿// ******************************************************************
// 概  述：巷道选择自定义控件
// 作  者：伍鑫
// 创建日期：2014/02/25
// 版本号：V1.0
// 版本信息:
// V1.0 新建
// ******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibBusiness;
using LibCommon;
using LibCommonControl;
using LibCommonForm;
using LibEntity;

namespace UnderTerminal
{
    public partial class SelectTunnelUserControl : BaseControl
    {
        // 矿井编号
        private int _iMineId;
        // 水平编号
        private int _iHorizontalId;
        // 采区编号
        private int _iMiningAreaId;
        // 工作面编号
        private int _iWorkingFaceId;
        // 巷道编号，无效巷道ID均使用
        private int _iTunnelId = Const.INVALID_ID;

        private string _sTunnelName;

        //是否启用过滤
        private bool _isFilterOn;

        private int _filterType = 0;

        private TunnelFilter.TunnelFilterRules _tunnelFilterRules;

        //列名
        private string _columnName;

        //参数值
        private string _columnValue;

        //声明巷道名称更改委托
        public delegate void TunnelNameChangedEventHandler(object sender, TunnelEventArgs e);
        //巷道名称更改事件
        public event TunnelNameChangedEventHandler TunnelNameChanged;
        //在其他类当中如果需要对巷道名称改变事件进行处理可按下列方式实现：
        //1,在其他类当中定义事件处理函数，如：void InheritTunnelNameChanged(object sender, TunnelEventArgs e);
        //2,注册方法,如：_selectTunnelUserControl.TunnelNameChanged += new TunnelNameChangedEventHandler(InheritTunnelNameChanged);

        /// <summary>
        /// 获取选择的巷道的ID，如果没有获取到巷道ID，则返回0。
        /// </summary>
        public int ITunnelId
        {
            get { return _iTunnelId; }
            set { _iTunnelId = value; }
        }

        public string STunnelName
        {
            get
            {
                return _sTunnelName;
            }

            set { _sTunnelName = value; }
        }

        public SelectTunnelUserControl()
        {
            InitializeComponent();
            //// 加载矿井信息
            loadMineName();
        }

        /// <summary>
        /// 巷道过滤设置(巷道类型过滤)
        /// </summary>
        /// <param name="columnValue">过滤参数值</param>
        //public void SetFilterOn(TunnelFilter.TunnelType columnValue)
        //{
        //    _isFilterOn = true;
        //    _columnName = TunnelInfoDbConstNames.TUNNEL_TYPE;
        //    _columnValue = columnValue.ToString();
        //    _filterType = 0;
        //}

        /// <summary>
        /// 巷道过滤设置（规则过滤）
        /// </summary>
        /// <param name="tunnelFilterRules"></param>
        public void SetFilterOn(TunnelFilter.TunnelFilterRules tunnelFilterRules)
        {
            _isFilterOn = true;
            _filterType = 1;
            _tunnelFilterRules = tunnelFilterRules;
        }

        //public void SetButtonEnable(bool enable)
        //{
        //    btnMineName.Enabled = enable;
        //    btnHorizontalName.Enabled = enable;
        //    btnMiningAreaName.Enabled = enable;
        //    btnWorkingFaceName.Enabled = enable;
        //    btnTunnelName.Enabled = enable;
        //}

        /// <summary>
        /// 返回全部ID
        /// </summary>
        /// <returns></returns>
        public int[] getSelectedValueArr()
        {
            int[] intArr = new int[5];
            intArr[0] = _iMineId;
            intArr[1] = _iHorizontalId;
            intArr[2] = _iMiningAreaId;
            intArr[3] = _iWorkingFaceId;
            intArr[4] = _iTunnelId;

            return intArr;
        }

        /// <summary>
        /// 设置控件中选中内容，数组大小为5,元素内容为对应的ID
        /// </summary>
        /// <param name="intArr">存储所选矿井编号，水平编号，采区编号，工作面编号的数组</param>
        public void setCurSelectedID(int[] intArr)
        {
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

            // 加载巷道信息
            loadTunnelName();
            // 设置默认
            this.lstTunnelName.SelectedValue = intArr[4];
            _iTunnelId = intArr[4];

        }

        #region 加载矿井信息
        /// <summary>
        /// 加载矿井信息
        /// </summary>
        public void loadMineName()
        {
            lstMineName.DataSource = null;
            lstHorizontalName.DataSource = null;
            lstMiningAreaName.DataSource = null;
            lstWorkingFaceName.DataSource = null;
            DataBindUtil.LoadMineName(lstMineName);
            loadHorizontalName();
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
            this.lstHorizontalName.DataSource = null;
            _iHorizontalId = Const.INVALID_ID;
            this.lstMiningAreaName.DataSource = null;
            _iMiningAreaId = Const.INVALID_ID;
            this.lstWorkingFaceName.DataSource = null;
            _iWorkingFaceId = Const.INVALID_ID;
            this.lstTunnelName.DataSource = null;
            _iTunnelId = Const.INVALID_ID;

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

                    this.lstHorizontalName.SelectedIndex = 0;
                }
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
            this.lstTunnelName.DataSource = null;

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

                    this.lstHorizontalName.SelectedIndex = 0;

                    loadMiningAreaName();
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
            _iMiningAreaId = Const.INVALID_ID;
            this.lstWorkingFaceName.DataSource = null;
            _iWorkingFaceId = Const.INVALID_ID;
            this.lstTunnelName.DataSource = null;
            _iTunnelId = Const.INVALID_ID;

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
            this.lstTunnelName.DataSource = null;

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

                    this.lstMiningAreaName.SelectedIndex = 0;

                    loadWorkingFaceName();
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
            _iWorkingFaceId = Const.INVALID_ID;
            this.lstTunnelName.DataSource = null;
            _iTunnelId = Const.INVALID_ID;

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
            this.lstTunnelName.DataSource = null;

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
            this.lstTunnelName.DataSource = null;
            _iTunnelId = Const.INVALID_ID;

            if (this.lstWorkingFaceName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("工作面编号：" + this.lstWorkingFaceName.SelectedValue.ToString());

                // 工作面编号
                int iWorkingFaceId = Convert.ToInt32(this.lstWorkingFaceName.SelectedValue);
                _iWorkingFaceId = iWorkingFaceId;

                // 获取巷道信息
                Tunnel[] tunnels = Tunnel.FindAllByWorkingFaceId(iWorkingFaceId);

                // 检索件数
                int iSelCnt = tunnels.Length;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定巷道信息
                    this.lstTunnelName.DataSource = tunnels;
                    this.lstTunnelName.DisplayMember = TunnelInfoDbConstNames.TUNNEL_NAME;
                    this.lstTunnelName.ValueMember = TunnelInfoDbConstNames.ID;

                    this.lstTunnelName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 加载巷道信息
        /// <summary>
        /// 加载巷道信息
        /// </summary>
        private void loadTunnelName()
        {
            this.lstTunnelName.DataSource = null;

            if (this.lstWorkingFaceName.SelectedItems.Count > 0)
            {
                //MessageBox.Show("工作面编号：" + this.lstWorkingFaceName.SelectedValue.ToString());

                // 工作面编号
                int iWorkingFaceId = Convert.ToInt32(this.lstWorkingFaceName.SelectedValue);
                _iWorkingFaceId = iWorkingFaceId;
                // 获取巷道信息
                Tunnel[] tunnels = Tunnel.FindAllByWorkingFaceId(iWorkingFaceId);

                // 检索件数
                int iSelCnt = tunnels.Length;
                // 检索件数 > 0 的场合
                if (iSelCnt > 0)
                {
                    // 绑定巷道信息
                    this.lstTunnelName.DataSource = tunnels;
                    this.lstTunnelName.DisplayMember = TunnelInfoDbConstNames.TUNNEL_NAME;
                    this.lstTunnelName.ValueMember = TunnelInfoDbConstNames.ID;

                    this.lstTunnelName.SelectedIndex = -1;
                }
            }
        }
        #endregion

        #region 巷道名称选择事件
        /// <summary>
        /// 巷道名称选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstTunnelName_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.lstTunnelName.SelectedItems.Count > 0)
            {
                // 巷道编号
                int iTunnelId = Convert.ToInt32(this.lstTunnelName.SelectedValue);
                _iTunnelId = iTunnelId;

                //MessageBox.Show("巷道编号：" + this.lstTunnelName.SelectedValue.ToString());
                System.Data.DataRowView drv = (System.Data.DataRowView)this.lstTunnelName.Items[this.lstTunnelName.SelectedIndex];
                STunnelName = (string)drv.Row.ItemArray[1]; // 巷道名称

                //调用事件方法，以便外部能够响应巷道名称改变事件。
                try
                {
                    if (TunnelNameChanged != null)
                    {
                        TunnelEventArgs arg = new TunnelEventArgs(_iTunnelId);
                        TunnelNameChanged(this, arg);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("未正常注册TunnelNameChanged事件: " + ex.Message);
                }
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

        /// <summary>
        /// 巷道名称Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTunnelName_Click(object sender, EventArgs e)
        {
            if (this.lstWorkingFaceName.SelectedItems.Count > 0)
            {
                int[] intArr = new int[4];
                intArr[0] = Convert.ToInt32(this.lstMineName.SelectedValue);
                intArr[1] = Convert.ToInt32(this.lstHorizontalName.SelectedValue);
                intArr[2] = Convert.ToInt32(this.lstMiningAreaName.SelectedValue);
                intArr[3] = Convert.ToInt32(this.lstWorkingFaceName.SelectedValue);

                int iTunnelId = Convert.ToInt32(this.lstTunnelName.SelectedValue);

                if (this.lstTunnelName.SelectedItems.Count > 0)
                {
                    var tunnelInfoEntering = new TunnelInfoEntering(Tunnel.Find(iTunnelId));
                    if (DialogResult.OK == tunnelInfoEntering.ShowDialog())
                    {
                        // 绑定巷道信息
                        loadTunnelName();
                    }
                }
                else
                {
                    LibCommonForm.TunnelInfoEntering tunnelInfoEntering = new LibCommonForm.TunnelInfoEntering(intArr);
                    if (DialogResult.OK == tunnelInfoEntering.ShowDialog())
                    {
                        // 绑定巷道信息
                        loadTunnelName();
                    }
                }
            }
            else
            {
                Alert.alert("请先选择所在工作面名称！");

            }

        }

        private void SelectTunnelUserControl_Load(object sender, EventArgs e)
        {

        }

    }

    public class TunnelEventArgs : EventArgs
    {
        //巷道ID
        private int _tunnelID;
        //构造函数
        public TunnelEventArgs(int tunnelID)
        {
            _tunnelID = tunnelID;
        }
    }
}
