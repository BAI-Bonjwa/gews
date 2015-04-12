﻿using System;
using System.Globalization;
using System.Windows.Forms;
using LibBusiness;
using LibBusiness.CommonBLL;
using LibCommon;
using LibEntity;
using LibSocket;

namespace LibPanels
{
    public partial class MineDataSimple : Form
    {
        //******定义变量***********
        private GasInfoEntering _gasData = new GasInfoEntering(); //瓦斯
        private GeologicStructureInfoEntering _geologicStructure = new GeologicStructureInfoEntering();
        private ManagementInfoEntering _management = new ManagementInfoEntering(); //管理
        private object _obj = null;
        private VentilationInfoEntering _ventilationInfoEntering = new VentilationInfoEntering(); //通风
        private CoalExistence _ceEntity = new CoalExistence(); //煤层赋存实体
        private CoalExistenceInfoEntering _coalExistenceInfoEntering = new CoalExistenceInfoEntering(); //煤层赋存
        private int formHeight = 247;
        private GasData gdEntity = new GasData(); //瓦斯实体
        private GeologicStructure _geologicStructureEntity = new GeologicStructure();
        private Management mEntity = new Management(); //管理实体
        private LibEntity.MineData mineDataEntity = new LibEntity.MineData();
        private Ventilation viEntity = new Ventilation(); //通风实体

        //*************************

        public MineDataSimple()
        {
            InitializeComponent();
        }

        public MineDataSimple(Tunnel tunnel)
        {
            selectTunnelSimple1.SelectedTunnel = tunnel;
        }

        /// <summary>
        ///     设置班次名称
        /// </summary>
        private void SetWorkTimeName()
        {
            string strWorkTimeName = "";
            string sysDateTime = DateTime.Now.ToString("HH:mm:ss");

            strWorkTimeName = MineDataSimpleBLL.selectWorkTimeNameByWorkTimeGroupIdAndSysTime(rbtn38.Checked ? 1 : 2, sysDateTime);

            if (!string.IsNullOrEmpty(strWorkTimeName))
            {
                cboWorkTime.Text = strWorkTimeName;
            }
        }


        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //验证
            if (!check())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;

            //通用信息
            mineDataEntity.Tunnel = selectTunnelSimple1.SelectedTunnel;
            mineDataEntity.CoordinateX = txtCoordinateX.Text == "" ? 0 : Convert.ToDouble(txtCoordinateX.Text);

            mineDataEntity.CoordinateY = txtCoordinateY.Text == "" ? 0 : Convert.ToDouble(txtCoordinateY.Text);
            mineDataEntity.CoordinateZ = txtCoordinateZ.Text == "" ? 0 : Convert.ToDouble(txtCoordinateZ.Text);
            mineDataEntity.Datetime = dtpDateTime.Value;

            mineDataEntity.WorkStyle = rbtn38.Checked ? Const_MS.WORK_TIME_38 : Const_MS.WORK_TIME_46;
            mineDataEntity.WorkTime = cboWorkTime.Text;
            mineDataEntity.TeamName = cboTeamName.Text;
            mineDataEntity.Submitter = cboSubmitter.Text;

            bool bResult = false;
            if (_ventilationInfoEntering.WindowState != FormWindowState.Minimized)         //提交通风特有信息
            {
                bResult = submitV();
            }
            if (_coalExistenceInfoEntering.WindowState != FormWindowState.Minimized)         //提交煤层赋存特有信息
            {
                bResult = submitC();
            }
            if (_gasData.WindowState != FormWindowState.Minimized)         //提交瓦斯特有信息
            {
                bResult = submitG();
            }
            if (_management.WindowState != FormWindowState.Minimized)         //提交管理特有信息
            {
                bResult = submitM();

            }
            if (_geologicStructure.WindowState != FormWindowState.Minimized)     //提交地质构造特有信息
            {
                bResult = submitGeologicStructure();
            }
            //关闭窗体
            if (!bResult) return;
            _ventilationInfoEntering.Close();
            _coalExistenceInfoEntering.Close();
            _gasData.Close();
            _management.Close();
            Close();
        }

        /// <summary>
        ///     提交通风特有信息
        /// </summary>
        private bool submitV()
        {
            //共通实体转化为通风实体
            viEntity = mineDataEntity.ChangeToVentilationInfoEntity();
            //是否有无风区域
            viEntity.IsNoWindArea = _ventilationInfoEntering.VentilationEntity.IsNoWindArea;
            //是否有微风区域
            viEntity.IsLightWindArea = _ventilationInfoEntering.VentilationEntity.IsLightWindArea;
            //是否有风流反向区域
            viEntity.IsReturnWindArea = _ventilationInfoEntering.VentilationEntity.IsReturnWindArea;
            //是否通风断面小于设计断面的2/3
            viEntity.IsSmall = _ventilationInfoEntering.VentilationEntity.IsSmall;
            //是否工作面风量低于计划风量，风速与《煤矿安全规程》规定不符
            viEntity.IsFollowRule = _ventilationInfoEntering.VentilationEntity.IsFollowRule;

            viEntity.FaultageArea = _ventilationInfoEntering.VentilationEntity.FaultageArea;

            viEntity.AirFlow = _ventilationInfoEntering.VentilationEntity.AirFlow;

            bool bResult = false;
            if (Text == new LibPanels(MineDataPanelName.Ventilation).panelFormName)
            {
                viEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送添加通风信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    Ventilation.TableName, OPERATION_TYPE.ADD, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送添加通风信息的Socket信息完成");
            }
            else if (Text == new LibPanels(MineDataPanelName.Ventilation_Change).panelFormName)
            {
                viEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送修改通风信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    Ventilation.TableName, OPERATION_TYPE.UPDATE, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送修改通风信息的Socket信息完成");
            }
            return bResult;
        }

        /// <summary>
        ///     提交煤层赋存特有信息
        /// </summary>
        private bool submitC()
        {
            //共通实体转化为煤层赋存实体
            _ceEntity = mineDataEntity.changeToCoalExistenceEntity();
            //是否层理紊乱
            _ceEntity.IsLevelDisorder = _coalExistenceInfoEntering.coalExistenceEntity.IsLevelDisorder;
            //煤厚变化
            _ceEntity.CoalThickChange = _coalExistenceInfoEntering.coalExistenceEntity.CoalThickChange;
            //软分层（构造煤）厚度
            _ceEntity.TectonicCoalThick = _coalExistenceInfoEntering.coalExistenceEntity.TectonicCoalThick;
            //软分层（构造煤）层位是否发生变化
            _ceEntity.IsLevelChange = _coalExistenceInfoEntering.coalExistenceEntity.IsLevelChange;
            //煤体破坏类型
            _ceEntity.CoalDistoryLevel = _coalExistenceInfoEntering.coalExistenceEntity.CoalDistoryLevel;
            //是否煤层走向、倾角突然急剧变化
            _ceEntity.IsTowardsChange = _coalExistenceInfoEntering.coalExistenceEntity.IsTowardsChange;
            //工作面煤层是否处于分叉、合层状态
            _ceEntity.IsCoalMerge = _coalExistenceInfoEntering.coalExistenceEntity.IsCoalMerge;
            //煤层是否松软
            _ceEntity.IsCoalSoft = _coalExistenceInfoEntering.coalExistenceEntity.IsCoalSoft;

            _ceEntity.Datetime = DateTime.Now;
            try
            {
                _ceEntity.SaveAndFlush();
                if (Text == new LibPanels(MineDataPanelName.CoalExistence).panelFormName)
                {
                    var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                        LibEntity.CoalExistence.TableName, OPERATION_TYPE.ADD, dtpDateTime.Value);
                    SocketUtil.SendMsg2Server(msg);
                }
                else if (Text == new LibPanels(MineDataPanelName.CoalExistence_Change).panelFormName)
                {
                    var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                        LibEntity.CoalExistence.TableName, OPERATION_TYPE.UPDATE, dtpDateTime.Value);
                    SocketUtil.SendMsg2Server(msg);
                }
                return true;
            }
            catch (Exception ex)
            {
                Alert.alert(ex.ToString());
                return false;
            }
        }



        /// <summary>
        ///     提交瓦斯特有信息
        /// </summary>
        private bool submitG()
        {
            gdEntity = mineDataEntity.ChangeToGasDataEntity();
            gdEntity.PowerFailure = _gasData.GasDataEntity.PowerFailure;
            gdEntity.DrillTimes = _gasData.GasDataEntity.DrillTimes;
            gdEntity.GasTimes = _gasData.GasDataEntity.GasTimes;
            gdEntity.TempDownTimes = _gasData.GasDataEntity.TempDownTimes;
            gdEntity.CoalBangTimes = _gasData.GasDataEntity.CoalBangTimes;
            gdEntity.CraterTimes = _gasData.GasDataEntity.CraterTimes;
            gdEntity.StoperTimes = _gasData.GasDataEntity.StoperTimes;
            //瓦斯浓度
            gdEntity.GasThickness = _gasData.GasDataEntity.GasThickness;
            bool bResult = false;
            if (Text == new LibPanels(MineDataPanelName.GasData).panelFormName)
            {
                gdEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送添加瓦斯信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    GasData.TableName, OPERATION_TYPE.ADD, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送添加瓦斯信息的Socket信息完成");
            }
            else if (Text == new LibPanels(MineDataPanelName.GasData_Change).panelFormName)
            {
                Log.Debug("发送修改瓦斯信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    GasData.TableName, OPERATION_TYPE.UPDATE, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                gdEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送修改瓦斯信息的Socket信息完成");
            }
            return bResult;
        }

        /// <summary>
        ///     提交管理特有信息
        /// </summary>
        private bool submitM()
        {
            mEntity = mineDataEntity.changeToManagementEntity();
            mEntity.IsGasErrorNotReport = _management.managementEntity.IsGasErrorNotReport;
            mEntity.IsWfNotReport = _management.managementEntity.IsWfNotReport;
            mEntity.IsStrgasNotDoWell = _management.managementEntity.IsStrgasNotDoWell;
            mEntity.IsRwmanagementNotDoWell = _management.managementEntity.IsRwmanagementNotDoWell;
            mEntity.IsVfBrokenByPeople = _management.managementEntity.IsVfBrokenByPeople;
            mEntity.IsElementPlaceNotGood = _management.managementEntity.IsElementPlaceNotGood;
            mEntity.IsReporterFalseData = _management.managementEntity.IsReporterFalseData;
            mEntity.IsDrillWrongBuild = _management.managementEntity.IsDrillWrongBuild;
            mEntity.IsDrillNotDoWell = _management.managementEntity.IsDrillNotDoWell;
            mEntity.IsOpNotDoWell = _management.managementEntity.IsOpNotDoWell;
            mEntity.IsOpErrorNotReport = _management.managementEntity.IsOpErrorNotReport;
            mEntity.IsPartWindSwitchError = _management.managementEntity.IsPartWindSwitchError;
            mEntity.IsSafeCtrlUninstall = _management.managementEntity.IsSafeCtrlUninstall;
            mEntity.IsCtrlStop = _management.managementEntity.IsCtrlStop;
            mEntity.IsGasNotDowWell = _management.managementEntity.IsGasNotDowWell;
            mEntity.IsMineNoChecker = _management.managementEntity.IsMineNoChecker;
            bool bResult = false;
            if (Text == new LibPanels(MineDataPanelName.Management).panelFormName)
            {
                mEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送添加管理信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    Management.TableName, OPERATION_TYPE.ADD, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送添加管理信息的Socket信息完成");
            }
            else if (Text == new LibPanels(MineDataPanelName.Management_Change).panelFormName)
            {
                mEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送修改管理信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                    Management.TableName, OPERATION_TYPE.UPDATE, mEntity.Datetime);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送修改管理信息的Socket信息完成");
            }
            return bResult;
        }

        /// <summary>
        ///     提交地质构造特有信息
        /// </summary>
        /// <returns></returns>
        private bool submitGeologicStructure()
        {
            _geologicStructureEntity = mineDataEntity.changeToGeologicStructureEntity();
            _geologicStructureEntity.NoPlanStructure = _geologicStructure.geoligicStructureEntity.NoPlanStructure;
            _geologicStructureEntity.PassedStructureRuleInvalid =
                _geologicStructure.geoligicStructureEntity.PassedStructureRuleInvalid;
            _geologicStructureEntity.YellowRuleInvalid = _geologicStructure.geoligicStructureEntity.YellowRuleInvalid;
            _geologicStructureEntity.RoofBroken = _geologicStructure.geoligicStructureEntity.RoofBroken;
            _geologicStructureEntity.CoalSeamSoft = _geologicStructure.geoligicStructureEntity.CoalSeamSoft;
            _geologicStructureEntity.CoalSeamBranch = _geologicStructure.geoligicStructureEntity.CoalSeamBranch;
            _geologicStructureEntity.RoofChange = _geologicStructure.geoligicStructureEntity.RoofChange;
            _geologicStructureEntity.GangueDisappear = _geologicStructure.geoligicStructureEntity.GangueDisappear;
            _geologicStructureEntity.GangueLocationChange =
                _geologicStructure.geoligicStructureEntity.GangueLocationChange;
            bool bResult = false;
            if (Text == new LibPanels(MineDataPanelName.GeologicStructure).panelFormName)
            {
                _geologicStructureEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送添加地址构造信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                     GeologicStructure.TableName, OPERATION_TYPE.ADD, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送添加地址构造信息的Socket信息完成");
            }
            else if (Text == new LibPanels(MineDataPanelName.GeologicStructure_Change).panelFormName)
            {
                _geologicStructureEntity.SaveAndFlush();
                bResult = true;
                Log.Debug("发送修改地址构造信息的Socket信息");
                var msg = new UpdateWarningDataMsg(Const.INVALID_ID, selectTunnelSimple1.SelectedTunnel.TunnelId,
                     GeologicStructure.TableName, OPERATION_TYPE.UPDATE, dtpDateTime.Value);
                SocketUtil.SendMsg2Server(msg);
                Log.Debug("发送修改地址构造信息的Socket信息完成");
            }
            return bResult;
        }

        /// <summary>selectTunnelSimple1.SelectedTunnel.TunnelId
        ///     验证
        /// </summary>
        /// <returns></returns>
        private bool check()
        {
            // 判断巷道信息是否选择
            //矿井名称
            if (selectTunnelSimple1.SelectedTunnel == null)
            {
                Alert.alert("请选择巷道信息");
                return false;
            }
            //坐标X是否为数字
            if (txtCoordinateX.Text != "")
            {
                if (!Check.IsNumeric(txtCoordinateX, "坐标X"))
                {
                    return false;
                }
            }
            //坐标Y是否为数字
            if (txtCoordinateY.Text != "")
            {
                if (!Check.IsNumeric(txtCoordinateY, "坐标Y"))
                {
                    return false;
                }
            }
            //坐标Z是否为数字
            if (txtCoordinateY.Text != "")
            {
                if (!Check.IsNumeric(txtCoordinateY, "坐标Y"))
                {
                    return false;
                }
            }
            //煤层赋存特有检查
            if (_coalExistenceInfoEntering.WindowState != FormWindowState.Minimized)
            {
                if (!_coalExistenceInfoEntering.check())
                {
                    return false;
                }
            }
            //瓦斯数据特有检查
            if (_gasData.WindowState != FormWindowState.Minimized)
            {
                if (!_gasData.check())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     工作制式切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn38_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtn38.Checked)
            {
                cboWorkTime.Text = "";
                DataBindUtil.LoadWorkTime(cboWorkTime, Const_MS.WORK_GROUP_ID_38);
            }
            else
            {
                cboWorkTime.Text = "";
                DataBindUtil.LoadWorkTime(cboWorkTime, Const_MS.WORK_GROUP_ID_46);
            }

            // 设置班次名称
            SetWorkTimeName();
        }

        private void MineDataSimple_Load(object sender, EventArgs e)
        {
            DataBindUtil.LoadTeam(cboTeamName);
            DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, cboTeamName.Text);
            DataBindUtil.LoadWorkTime(cboWorkTime,
                rbtn38.Checked ? Const_MS.WORK_GROUP_ID_38 : Const_MS.WORK_GROUP_ID_46);

            if (WorkingTimeDefault.FindFirst().DefaultWorkTimeGroupId == Const_MS.WORK_GROUP_ID_38)
            {
                rbtn38.Checked = true;
            }
            else
            {
                rbtn46.Checked = true;
            }
            // 设置班次名称
            SetWorkTimeName();

            //窗体绑定到Panel中
            _ventilationInfoEntering.MdiParent = this;
            _ventilationInfoEntering.Parent = panel2;
            _coalExistenceInfoEntering.MdiParent = this;
            _coalExistenceInfoEntering.Parent = panel2;
            _gasData.MdiParent = this;
            _gasData.Parent = panel2;
            _management.MdiParent = this;
            _management.Parent = panel2;
            _geologicStructure.MdiParent = this;
            _geologicStructure.Parent = panel2;

            //panel2绑定窗体
            panel2.Controls.Add(_coalExistenceInfoEntering);
            panel2.Controls.Add(_ventilationInfoEntering);
            panel2.Controls.Add(_gasData);
            panel2.Controls.Add(_management);
            panel2.Controls.Add(_geologicStructure);

            if (Text == new LibPanels(MineDataPanelName.Ventilation_Change).panelFormName)
            {
                viEntity = (Ventilation)_obj;
            }
            if (Text == new LibPanels(MineDataPanelName.CoalExistence_Change).panelFormName)
            {
                _ceEntity = (CoalExistence)_obj;
            }
            if (Text == new LibPanels(MineDataPanelName.GasData_Change).panelFormName)
            {
                gdEntity = (GasData)_obj;
            }
            if (Text == new LibPanels(MineDataPanelName.Management_Change).panelFormName)
            {
                mEntity = (Management)_obj;
            }
            if (Text == new LibPanels(MineDataPanelName.GeologicStructure_Change).panelFormName)
            {
                _geologicStructureEntity = (GeologicStructure)_obj;
            }

            //所有小窗体最小化
            //AllMin();
            //通风
            if (Text == new LibPanels(MineDataPanelName.Ventilation).panelFormName)
            {
                Height = formHeight + _ventilationInfoEntering.Height;
                _ventilationInfoEntering.WindowState = FormWindowState.Maximized;
                _ventilationInfoEntering.Show();
                _ventilationInfoEntering.Activate();
            }
            //煤层赋存
            if (Text == new LibPanels(MineDataPanelName.CoalExistence).panelFormName)
            {
                Height = formHeight + _coalExistenceInfoEntering.Height;
                _coalExistenceInfoEntering.WindowState = FormWindowState.Maximized;
                _coalExistenceInfoEntering.Show();
                _coalExistenceInfoEntering.Activate();
            }
            //瓦斯
            if (Text == new LibPanels(MineDataPanelName.GasData).panelFormName)
            {
                Height = formHeight + _gasData.Height;
                _gasData.WindowState = FormWindowState.Maximized;
                _gasData.Show();
                _gasData.Activate();
            }
            //管理
            if (Text == new LibPanels(MineDataPanelName.Management).panelFormName)
            {
                Height = formHeight + _management.Height;
                _management.WindowState = FormWindowState.Maximized;
                _management.Show();
                _management.Activate();
            }
            //地质构造
            if (Text == new LibPanels(MineDataPanelName.GeologicStructure).panelFormName)
            {
                Height = formHeight + _geologicStructure.Height;
                _geologicStructure.WindowState = FormWindowState.Maximized;
                _geologicStructure.Show();
                _geologicStructure.Activate();
            }

            //绑定通风修改初始信息
            if (Text == new LibPanels(MineDataPanelName.Ventilation_Change).panelFormName)
            {
                Height = formHeight + _ventilationInfoEntering.Height;
                ChangeMineCommonValue(viEntity);

                _ventilationInfoEntering.VentilationEntity = viEntity;

                _ventilationInfoEntering.bindDefaultValue(viEntity);

                _ventilationInfoEntering.WindowState = FormWindowState.Maximized;
                _ventilationInfoEntering.Show();
                _ventilationInfoEntering.Activate();
            }

            //绑定煤层赋存修改初始信息
            if (Text == new LibPanels(MineDataPanelName.CoalExistence_Change).panelFormName)
            {
                Height = formHeight + _coalExistenceInfoEntering.Height;
                ChangeMineCommonValue(_ceEntity);

                _coalExistenceInfoEntering.coalExistenceEntity = _ceEntity;

                _coalExistenceInfoEntering.bindDefaultValue(_ceEntity);

                _coalExistenceInfoEntering.WindowState = FormWindowState.Maximized;
                _coalExistenceInfoEntering.Show();
                _coalExistenceInfoEntering.Activate();
            }

            //绑定瓦斯修改初始信息
            if (Text == new LibPanels(MineDataPanelName.GasData_Change).panelFormName)
            {
                Height = formHeight + _gasData.Height;
                ChangeMineCommonValue(gdEntity);

                _gasData.GasDataEntity = gdEntity;
                _gasData.bindDefaultValue(gdEntity);

                _gasData.WindowState = FormWindowState.Maximized;
                _gasData.Show();
                _gasData.Activate();
            }
            //绑定管理修改初始信息
            if (Text == new LibPanels(MineDataPanelName.Management_Change).panelFormName)
            {
                Height = formHeight + _management.Height;
                ChangeMineCommonValue(mEntity);

                _management.managementEntity = mEntity;

                _management.bindDefaultValue(mEntity);

                _management.WindowState = FormWindowState.Maximized;
                _management.Show();
                _management.Activate();
            }
            //绑定地质构造修改初始数据
            if (Text == new LibPanels(MineDataPanelName.GeologicStructure_Change).panelFormName)
            {
                Height = formHeight + _management.Height;
                ChangeMineCommonValue(_geologicStructureEntity);

                _geologicStructure.geoligicStructureEntity = _geologicStructureEntity;
                _geologicStructure.bindDefaultValue(_geologicStructureEntity);

                _geologicStructure.WindowState = FormWindowState.Maximized;
                _geologicStructure.Show();
                _geologicStructure.Activate();
            }
        }

        /// <summary>
        ///     绑定井下数据通用信息
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeMineCommonValue(object obj)
        {
            mineDataEntity = (LibEntity.MineData)obj;
            txtCoordinateX.Text = mineDataEntity.CoordinateX.ToString(CultureInfo.InvariantCulture);
            txtCoordinateY.Text = mineDataEntity.CoordinateY.ToString(CultureInfo.InvariantCulture);
            txtCoordinateZ.Text = mineDataEntity.CoordinateZ.ToString(CultureInfo.InvariantCulture);

            if (mineDataEntity.WorkStyle == Const_MS.WORK_TIME_38)
            {
                rbtn38.Checked = true;
            }
            if (mineDataEntity.WorkStyle == Const_MS.WORK_TIME_46)
            {
                rbtn46.Checked = true;
            }
            cboWorkTime.Text = mineDataEntity.WorkTime;
            cboTeamName.Text = mineDataEntity.TeamName;
            cboSubmitter.Text = mineDataEntity.Submitter;
            dtpDateTime.Value = mineDataEntity.Datetime;
        }

        /// <summary>
        ///     所有的窗体都最小化
        /// </summary>
        //private void AllMin()
        //{
        //    _coalExistenceInfoEntering.WindowState = FormWindowState.Minimized;
        //    _ventilationInfoEntering.WindowState = FormWindowState.Minimized;
        //    _gasData.WindowState = FormWindowState.Minimized;
        //    _usualForecast.WindowState = FormWindowState.Minimized;
        //    _management.WindowState = FormWindowState.Minimized;
        //    _geologicStructure.WindowState = FormWindowState.Minimized;
        //}

        /// <summary>
        ///     队别选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTeamName_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, cboTeamName.Text);
        }

        private void MineDataSimple_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}