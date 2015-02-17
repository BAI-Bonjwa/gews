﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using GIS;
using GIS.HdProc;
using LibBusiness;
using LibBusiness.CommonBLL;
using LibCommon;
using LibCommonControl;
using LibCommonForm;
using LibEntity;
using LibPanels;
using LibSocket;
using Point = System.Drawing.Point;
using TunnelDefaultSelect = LibBusiness.TunnelDefaultSelect;

namespace sys2
{
    public partial class DayReportJJEntering : BaseForm
    {
        #region ******变量声明******

        /**掘进面实体**/

        //各列索引
        private const int C_DATE = 0; // 选择日期
        private const int C_WORK_TIME = 1; // 班次
        private const int C_WORK_CONTENT = 2; // 工作内容
        private const int C_WORK_PROGRESS = 3; // 进尺
        private const int C_COMMENTS = 4; // 备注
        private readonly DayReportJj _dayReportJJEntity = new DayReportJj();
        private readonly DateTimePicker dtp = new DateTimePicker(); //这里实例化一个DateTimePicker控件
        private Rectangle _Rectangle;
        private int[] _arr;
        private WorkingFace workingFace;

        #endregion

        /// <summary>
        ///     构造方法
        /// </summary>
        public DayReportJJEntering()
        {
            InitializeComponent();

            dgrdvDayReportJJ.DataError += delegate { };

            dgrdvDayReportJJ.Controls.Add(dtp); //把时间控件加入DataGridView
            dtp.Visible = false; //先不让它显示
            dtp.Format = DateTimePickerFormat.Custom; //设置日期格式为2010-08-05
            dtp.TextChanged += dtp_TextChange; //为时间控件加入事件dtp_TextChange

            addInfo();
            //设置窗体格式
            FormDefaultPropertiesSetter.SetEnteringFormDefaultProperties(this, Const_MS.DAY_REPORT_JJ_ADD);

            //自定义控件初始化
            //WorkingFaceSelectEntity workingFaceSelectEntity = WorkingFaceSelect.SelectWorkingFace(DayReportHCDbConstNames.TABLE_NAME);

            //if (workingFaceSelectEntity != null)
            //{
            //    _arr = new int[5];
            //    _arr[0] = workingFaceSelectEntity.MineID;
            //    _arr[1] = workingFaceSelectEntity.HorizontalID;
            //    _arr[2] = workingFaceSelectEntity.MiningAreaID;

            //    this.selectWorkingFaceControl1.setCurSelectedID(_arr);
            //}
            //else
            //{

            //this.selectWorkingFaceControl1.loadMineName();
            //}

            //注册事件 
        }

        /// <summary>
        ///     构造方法
        /// </summary>
        /// <param name="array">巷道编号数组</param>
        /// <param name="dayReportHCEntity">回采进尺日报实体</param>
        public DayReportJJEntering(DayReportJj dayReportJJEntity)
        {
            _dayReportJJEntity = dayReportJJEntity;
            InitializeComponent();
            //修改初始化
            changeInfo();
            //设置窗体格式
            FormDefaultPropertiesSetter.SetEnteringFormDefaultProperties(this, Const_MS.DAY_REPORT_JJ_CHANGE);
            dgrdvDayReportJJ.AllowUserToAddRows = false;

            dgrdvDayReportJJ.Controls.Add(dtp); //把时间控件加入DataGridView
            dtp.Visible = false; //先不让它显示
            dtp.Format = DateTimePickerFormat.Custom; //设置日期格式为2010-08-05
            dtp.TextChanged += dtp_TextChange; //为时间控件加入事件dtp_TextChange

            dgrdvDayReportJJ.DataError += delegate { };

            workingFace = WorkingFace.Find(
                Convert.ToInt32(_arr[3]));


            //自定义控件初始化
            //WorkingFaceSelectEntity workingFaceSelectEntity = WorkingFaceSelect.SelectWorkingFace(DayReportHCDbConstNames.TABLE_NAME);
            //if (workingFaceSelectEntity != null)
            //{
            //    _arr = new int[5];
            //    _arr[0] = workingFaceSelectEntity.MineID;
            //    _arr[1] = workingFaceSelectEntity.HorizontalID;
            //    _arr[2] = workingFaceSelectEntity.MiningAreaID;

            //this.selectWorkingFaceControl1.setCurSelectedID(_arr);

            //}
            //else
            //{
            //    this.selectWorkingFaceControl1.loadMineName();
            //}
            //注册事件
        }

        private void NameChangeEvent(object sender, WorkingFaceEventArgs e)
        {
            workingFace = WorkingFace.Find(
                selectWorkingfaceSimple1.IWorkingfaceId);
        }


        private void DayReportJJEntering_Load(object sender, EventArgs e)
        {
            selectWorkingfaceSimple1.WorkingfaceNameChanged += NameChangeEvent;
            if (workingFace != null)
            {
                var ws = new WorkingfaceSimple(workingFace.WorkingFaceId, workingFace.WorkingFaceName,
                    workingFace.WorkingfaceTypeEnum);
                selectWorkingfaceSimple1.SelectTunnelItemWithoutHistory(ws);
            }
        }

        /// <summary>
        ///     添加时加载初始化设置
        /// </summary>
        private void addInfo()
        {
            //绑定队别名称
            bindTeamInfo();
            ////初始化班次
            //this.bindWorkTimeFirstTime();
            //设置为默认工作制式
            if (WorkingTimeDefault.FindFirst().DefaultWorkTimeGroupId == Const_MS.WORK_GROUP_ID_38)
            {
                rbtn38.Checked = true;
            }
            else
            {
                rbtn46.Checked = true;
            }
            // 设置班次名称
            setWorkTimeName();

            ////设置班次为当前时间对应的班次
            //dgrdvDayReportJJ[0, 0].Value = WorkTime.returnSysWorkTime(rbtn38.Checked ? Const_MS.WORK_TIME_38 : Const_MS.WORK_TIME_46);

            dgrdvDayReportJJ[C_WORK_CONTENT, 0].Value = Const_MS.JJ;
        }

        /// <summary>
        ///     设置班次名称
        /// </summary>
        private void setWorkTimeName()
        {
            string strWorkTimeName = "";
            string sysDateTime = DateTime.Now.ToLongTimeString();
            if (rbtn38.Checked)
            {
                strWorkTimeName = MineDataSimpleBLL.selectWorkTimeNameByWorkTimeGroupIdAndSysTime(1, sysDateTime);
            }
            else
            {
                strWorkTimeName = MineDataSimpleBLL.selectWorkTimeNameByWorkTimeGroupIdAndSysTime(2, sysDateTime);
            }

            if (strWorkTimeName != null && strWorkTimeName != "")
            {
                dgrdvDayReportJJ[C_WORK_TIME, 0].Value = strWorkTimeName;
            }
        }

        /// <summary>
        ///     修改时加载初始化设置
        /// </summary>
        private void changeInfo()
        {
            //绑定默认信息
            addInfo();
            //绑定修改数据
            bindInfo();
        }

        /// <summary>
        ///     datagridview绑定信息
        /// </summary>
        private void bindInfo()
        {
            //工作制式
            if (_dayReportJJEntity.WorkTimeStyle == Const_MS.WORK_TIME_38)
            {
                rbtn38.Checked = true;
            }
            if (_dayReportJJEntity.WorkTimeStyle == Const_MS.WORK_TIME_46)
            {
                rbtn46.Checked = true;
            }

            //队别
            cboTeamName.Text = Team.Find(_dayReportJJEntity.Team.TeamId).TeamName;

            //绑定队别成员
            DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, cboTeamName.Text);

            //填报人
            cboSubmitter.Text = _dayReportJJEntity.Submitter;
            dgrdvDayReportJJ.Rows.Add();
            dgrdvDayReportJJ[C_DATE, 0].Value = Convert.ToString(_dayReportJJEntity.DateTime);
            dgrdvDayReportJJ[C_WORK_TIME, 0].Value = _dayReportJJEntity.WorkTime;
            dgrdvDayReportJJ[C_WORK_CONTENT, 0].Value = _dayReportJJEntity.WorkInfo;
            dgrdvDayReportJJ[C_WORK_PROGRESS, 0].Value = _dayReportJJEntity.JinChi;
            dgrdvDayReportJJ[C_COMMENTS, 0].Value = _dayReportJJEntity.Remarks;
        }

        /// <summary>
        ///     绑定队别名称
        /// </summary>
        private void bindTeamInfo()
        {
            cboTeamName.Items.Clear();
            Team[] team = Team.FindAll();
            foreach (Team t in team)
            {
                cboTeamName.Items.Add(t.TeamName);
            }
        }


        /// <summary>
        ///     添加队别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTeamInfo_Click(object sender, EventArgs e)
        {
            TeamInfoEntering teamInfoForm;
            if (cboTeamName.Text == "")
            {
                teamInfoForm = new TeamInfoEntering();
            }
            else
            {
                var teamEntity = new Team();
                teamEntity.TeamId = Convert.ToInt32(cboTeamName.SelectedValue);
                teamEntity = Team.Find(teamEntity.TeamId);
                teamInfoForm = new TeamInfoEntering(teamEntity);
            }

            if (DialogResult.OK == teamInfoForm.ShowDialog())
            {
                bindTeamInfo();
                cboTeamName.Text = teamInfoForm.returnTeamName();
                var ds = new DataSet();

                DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, teamInfoForm.returnTeamName());
            }
        }

        /// <summary>
        ///     添加队别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTeamMemberInfo_Click(object sender, EventArgs e)
        {
            var t = new TeamInfoEntering();
            if (DialogResult.OK == t.ShowDialog())
            {
                DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, cboTeamName.Text);
                cboSubmitter.Text = t.returnTeamName();
            }
        }

        /// <summary>
        ///     队别选择事件（根据队别绑定队员）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTeamName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.bindTeamMember();
        }

        /// <summary>
        ///     取消按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            //关闭窗体
            Close();
        }

        /// <summary>
        ///     提交按钮事件
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
            //巷道掘进的参数
            if (Text == Const_MS.DAY_REPORT_JJ_ADD)
            {
                TunnelDefaultSelect.InsertDefaultTunnel(DayReportJj.TableName, workingFace.WorkingFaceId);
                insertDayReportJJInfo();
            }
            if (Text == Const_MS.DAY_REPORT_JJ_CHANGE)
            {
                DayReportJj oldDayReportJJEntity = _dayReportJJEntity; //修改之前的实体
                TunnelDefaultSelect.UpdateDefaultTunnel(DayReportJj.TableName, workingFace.WorkingFaceId);
                DayReportJj newDayReportJJEntity = _dayReportJJEntity; //修改后的掘进信息实体               
            }
        }

        //添加巷道掘进
        private void addHdJc(string hdid, double jjcd, string bid, double hdwid)
        {
            Dictionary<string, List<GeoStruct>> geostructsinfos = Global.cons.DrawJJCD(hdid, bid, hdwid, null, jjcd, 0,
                Global.searchlen, Global.sxjl, 0);
            //修改工作面信息表中对应的X Y Z坐标信息
            List<GeoStruct> pos = geostructsinfos.Last().Value;
            var workfacepos = pos[0].geo as IPoint;
            if (workfacepos != null)
            {
                workingFace.Coordinate = new Coordinate(workfacepos.X, workfacepos.Y, 0.0);
                workingFace.Save();
            }
            //查询地质结构信息
            geostructsinfos.Remove("LAST");
            GeologySpaceBll.DeleteGeologySpaceEntityInfos(workingFace.WorkingFaceId); //删除工作面ID对应的地质构造信息
            foreach (string key in geostructsinfos.Keys)
            {
                List<GeoStruct> geoinfos = geostructsinfos[key];
                string geo_type = key;
                for (int i = 0; i < geoinfos.Count; i++)
                {
                    GeoStruct tmp = geoinfos[i];

                    var geologyspaceEntity = new GeologySpace();
                    geologyspaceEntity.WorkingFace = workingFace;
                    geologyspaceEntity.TectonicType = Convert.ToInt32(key);
                    geologyspaceEntity.TectonicId = tmp.geoinfos[GIS_Const.FIELD_BID];
                    geologyspaceEntity.Distance = tmp.dist;
                    geologyspaceEntity.OnDateTime = DateTime.Now.ToShortDateString();

                    geologyspaceEntity.Save();
                }
            }
        }

        //修改巷道
        private void UpdateHdJc(string hdid, string bid, double jjcd)
        {
            Dictionary<string, string> deltas = Global.cons.UpdateJJCD(hdid, bid, jjcd, 0, Global.searchlen, Global.sxjl,
                0);
            string xydeltas = deltas[bid];
            double xdelta = Convert.ToDouble(xydeltas.Split('|')[0]);
            double ydelta = Convert.ToDouble(xydeltas.Split('|')[1]);

            //更新地质结构信息表
            IPoint pnt = new PointClass();
            pnt.X = workingFace.Coordinate.X + xdelta;
            pnt.Y = workingFace.Coordinate.Y + ydelta;
            pnt.Z = workingFace.Coordinate.Z;
            pnt.SpatialReference = Global.spatialref;

            //修改工作面信息表中对应的X Y Z坐标信息
            workingFace.Coordinate = new Coordinate(pnt.X, pnt.Y, 0.0);
            workingFace.Save();

            //查询地质结构信息
            var hd_ids = new List<int>();
            hd_ids.Add(Convert.ToInt16(hdid));
            Dictionary<string, List<GeoStruct>> geostructsinfos = Global.commonclss.GetStructsInfos(pnt, hd_ids);
            GeologySpaceBll.DeleteGeologySpaceEntityInfos(workingFace.WorkingFaceId); //删除对应工作面ID的地质构造信息
            foreach (string key in geostructsinfos.Keys)
            {
                List<GeoStruct> geoinfos = geostructsinfos[key];
                string geo_type = key;
                for (int i = 0; i < geoinfos.Count; i++)
                {
                    GeoStruct tmp = geoinfos[i];

                    var geologyspaceEntity = new GeologySpace();
                    geologyspaceEntity.WorkingFace = workingFace;
                    geologyspaceEntity.TectonicType = Convert.ToInt32(key);
                    geologyspaceEntity.TectonicId = tmp.geoinfos[GIS_Const.FIELD_BID];
                    geologyspaceEntity.Distance = tmp.dist;
                    geologyspaceEntity.OnDateTime = DateTime.Now.ToShortDateString();

                    geologyspaceEntity.Save();
                }
            }
        }

        /// <summary>
        ///     添加掘进日报
        /// </summary>
        private void insertDayReportJJInfo()
        {
            var dayReportJJEntityList = new List<DayReportJj>();

            for (int i = 0; i < dgrdvDayReportJJ.RowCount; i++)
            {
                var _dayReportJJEntity = new DayReportJj();
                // 最后一行为空行时，跳出循环
                if (i == dgrdvDayReportJJ.RowCount - 1)
                {
                    break;
                }

                /**回采日报实体赋值**/
                //队别名称
                _dayReportJJEntity.Team.TeamId = Convert.ToInt32(cboTeamName.SelectedValue);
                //绑定巷道编号
                _dayReportJJEntity.WorkingFace.WorkingFaceId = workingFace.WorkingFaceId;

                DataGridViewCellCollection cells = dgrdvDayReportJJ.Rows[i].Cells;
                //日期
                if (cells[C_DATE].Value != null)
                {
                    _dayReportJJEntity.DateTime = Convert.ToDateTime(cells[C_DATE].Value.ToString());
                }

                //填报人
                _dayReportJJEntity.Submitter = cboSubmitter.Text;
                //工作制式
                if (rbtn38.Checked)
                {
                    _dayReportJJEntity.WorkTimeStyle = rbtn38.Text;
                }
                if (rbtn46.Checked)
                {
                    _dayReportJJEntity.WorkTimeStyle = rbtn46.Text;
                }
                //班次
                if (cells[C_WORK_TIME].Value != null)
                {
                    _dayReportJJEntity.WorkTime = cells[C_WORK_TIME].Value.ToString();
                }
                //工作内容
                if (cells[C_WORK_CONTENT].Value != null)
                {
                    _dayReportJJEntity.WorkInfo = cells[C_WORK_CONTENT].Value.ToString();
                }
                //掘进进尺
                if (cells[C_WORK_PROGRESS].Value != null)
                {
                    _dayReportJJEntity.JinChi = Convert.ToDouble(cells[C_WORK_PROGRESS].Value);
                }
                //备注
                if (cells[C_COMMENTS].Value != null)
                {
                    _dayReportJJEntity.Remarks = cells[C_COMMENTS].Value.ToString();
                }
                //BID
                _dayReportJJEntity.BindingId = IDGenerator.NewBindingID();

                //添加到dayReportHCEntityList中
                dayReportJJEntityList.Add(_dayReportJJEntity);
            }

            Tunnel tunnel = Tunnel.FindFirstByWorkingFaceId(workingFace.WorkingFaceId);

            //循环添加
            foreach (DayReportJj dayReportJJEntity in dayReportJJEntityList)
            {
                //添加回采进尺日报
                dayReportJJEntity.SaveAndFlush();

                //巷道掘进绘图
                double dist = dayReportJJEntity.JinChi;

                // 巷道id                
                string hdid = tunnel.TunnelId.ToString();
                string bid = dayReportJJEntity.BindingId;

                addHdJc(hdid, dist, bid, tunnel.TunnelWid);
            }

            Log.Debug("添加进尺数据发送Socket消息");
            // 通知服务器掘进进尺已经更新
            var msg = new UpdateWarningDataMsg(workingFace.WorkingFaceId, tunnel.TunnelId,
                DayReportJj.TableName, OPERATION_TYPE.ADD, DateTime.Now);
            MainForm.SendMsg2Server(msg);
            Log.Debug("添加进尺数据Socket消息发送完成");
        }

        /// <summary>
        ///     三八制选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtn38_CheckedChanged(object sender, EventArgs e)
        {
            //选择三八制
            if (rbtn38.Checked)
            {
                cboWorkTime.Items.Clear();
                //清空班次下拉框中选项
                DataBindUtil.LoadWorkTime(cboWorkTime, Const_MS.WORK_GROUP_ID_38);
            }
            //选择四六制
            else
            {
                cboWorkTime.Items.Clear();
                DataBindUtil.LoadWorkTime(cboWorkTime, Const_MS.WORK_GROUP_ID_46);
            }

            // 设置班次名称
            setWorkTimeName();

            for (int i = 0; i < dgrdvDayReportJJ.RowCount; i++)
            {
                dgrdvDayReportJJ[C_WORK_TIME, i].Value = dgrdvDayReportJJ[C_WORK_TIME, 0].Value;
            }
        }

        /// <summary>
        ///     验证
        /// </summary>
        /// <returns></returns>
        private bool check()
        {
            //巷道是否选择
            if (selectWorkingfaceSimple1.IWorkingfaceId <= 0)
            {
                Alert.alert(Const.MSG_PLEASE_CHOOSE + Const_MS.TUNNEL + Const.SIGN_EXCLAMATION_MARK);
                return false;
            }
            //队别为空
            if (Validator.IsEmpty(cboTeamName.Text))
            {
                Alert.alert(Const.MSG_PLEASE_TYPE_IN + Const_MS.TEAM_NAME + Const.SIGN_EXCLAMATION_MARK);
                return false;
            }
            //队别名称是否有特殊字符
            if (Validator.checkSpecialCharacters(cboTeamName.Text))
            {
                Alert.alert(Const_MS.TEAM_NAME + Const.MSG_SP_CHAR + Const.SIGN_EXCLAMATION_MARK);
                return false;
            }
            //填报人是否有特殊字符
            if (Validator.checkSpecialCharacters(cboSubmitter.Text))
            {
                Alert.alert(Const_MS.SUBMITTER + Const.MSG_SP_CHAR + Const.SIGN_EXCLAMATION_MARK);
                return false;
            }
            //datagridview验证
            //只有一条数据时
            if (dgrdvDayReportJJ.Rows.Count - 1 == 0)
            {
                //添加时判断为未录入进尺
                if (Text == Const_MS.DAY_REPORT_JJ_ADD)
                {
                    Alert.alert(Const.MSG_PLEASE_TYPE_IN + Const_MS.JC + Const.SIGN_EXCLAMATION_MARK);
                    return false;
                }
                //修改时
                bool bResult = false;
                //为空返回false，不数据时跳出循环
                for (int i = 0; i < dgrdvDayReportJJ.ColumnCount; i++)
                {
                    if (dgrdvDayReportJJ[i, 0].Value == null)
                    {
                        bResult = false;
                    }
                    else
                    {
                        bResult = true;
                        break;
                    }
                }
                if (!bResult)
                {
                    Alert.alert(Const.MSG_PLEASE_TYPE_IN + Const_MS.JC);
                    return bResult;
                }
            }
            for (int i = 0; i < dgrdvDayReportJJ.RowCount; i++)
            {
                // 最后一行为空行时，跳出循环
                if (i == dgrdvDayReportJJ.RowCount - 1)
                {
                    break;
                }

                var cell = dgrdvDayReportJJ.Rows[i].Cells[C_WORK_PROGRESS] as DataGridViewTextBoxCell;
                //进尺为空
                if (cell.Value == null)
                {
                    if (Const_MS.JJ == dgrdvDayReportJJ[C_WORK_PROGRESS, i].Value.ToString())
                    {
                        cell.Style.BackColor = Const.ERROR_FIELD_COLOR;
                        Alert.alert(Const_MS.JC + Const.MSG_NOT_NULL + Const.SIGN_EXCLAMATION_MARK);
                        return false;
                    }
                    cell.Style.BackColor = Const.NO_ERROR_FIELD_COLOR;
                }
                else
                {
                    cell.Style.BackColor = Const.NO_ERROR_FIELD_COLOR;
                }
                //进尺不为数字
                if (cell.Value != null && !Validator.IsNumeric(cell.Value.ToString()))
                {
                    cell.Style.BackColor = Const.ERROR_FIELD_COLOR;
                    Alert.alert(Const_MS.JC + Const.MSG_MUST_NUMBER + Const.SIGN_EXCLAMATION_MARK);
                    return false;
                }
                cell.Style.BackColor = Const.NO_ERROR_FIELD_COLOR;

                cell = dgrdvDayReportJJ.Rows[i].Cells[C_COMMENTS] as DataGridViewTextBoxCell;
                //备注不能含特殊字符
                if (cell.Value != null)
                {
                    if (Validator.checkSpecialCharacters(cell.Value.ToString()))
                    {
                        cell.Style.BackColor = Const.ERROR_FIELD_COLOR;
                        Alert.alert(Const_MS.OTHER + Const.MSG_SP_CHAR + Const.SIGN_EXCLAMATION_MARK);
                        return false;
                    }
                    cell.Style.BackColor = Const.NO_ERROR_FIELD_COLOR;
                }
            }
            //验证成功
            return true;
        }

        private void cboTeamName_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cboTeamName.Items.Count; i++)
            {
                if (cboTeamName.Text == cboTeamName.GetItemText(cboTeamName.Items[i]))
                {
                    DataBindUtil.LoadTeamMemberByTeamName(cboSubmitter, cboTeamName.Text);
                    break;
                }
                cboSubmitter.Items.Clear();
                cboSubmitter.Text = "";
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgrdvDayReportJJ_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //第一行时
            if (e.RowIndex == 0)
            {
                //进尺单元格
                if (e.ColumnIndex == C_WORK_PROGRESS)
                {
                    //非空
                    if (dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value != null)
                    {
                        //验证输入是否为数字
                        if (!Validator.IsNumeric(dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value.ToString()))
                        {
                            Alert.alert(Const_MS.JC + Const.MSG_MUST_NUMBER + Const_MS.SIGN_EXCLAMATION_MARK);
                            //错误处理
                            //添加时处理方式
                            if (Text == Const_MS.DAY_REPORT_JJ_ADD)
                            {
                                dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value = null;
                            }
                            //修改时处理方式
                            else
                            {
                                dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value = _dayReportJJEntity.JinChi;
                            }
                        }
                    }
                }
            }
            //非第一行时
            else
            {
                //进尺单元格
                if (e.ColumnIndex == C_WORK_PROGRESS)
                {
                    //非空
                    if (dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value != null)
                    {
                        //验证输入是否为数字
                        if (!Validator.IsNumeric(dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value.ToString()))
                        {
                            Alert.alert(Const_MS.JC + Const.MSG_MUST_NUMBER + Const_MS.SIGN_EXCLAMATION_MARK);
                            //错误处理
                            //添加时处理方式
                            if (Text == Const_MS.DAY_REPORT_HC_ADD)
                            {
                                dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value = null;
                            }
                        }
                    }
                }
            }
        }

        private void dgrdvDayReportJJ_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgrdvDayReportJJ[C_WORK_CONTENT, e.RowIndex].Value = "掘进";
        }

        private void dgrdvDayReportJJ_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (dgrdvDayReportJJ[C_DATE, e.RowIndex].Value == null &&
                dgrdvDayReportJJ[C_WORK_TIME, e.RowIndex].Value == null &&
                dgrdvDayReportJJ[C_WORK_CONTENT, e.RowIndex].Value == null &&
                dgrdvDayReportJJ[C_COMMENTS, e.RowIndex].Value == null)
            {
                dgrdvDayReportJJ[C_WORK_PROGRESS, e.RowIndex].Value = null;
            }
        }

        private void dgrdvDayReportJJ_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            dgrdvDayReportJJ.EndEdit();
        }

        /*************时间控件选择时间时****************/

        private void dtp_TextChange(object sender, EventArgs e)
        {
            dgrdvDayReportJJ.CurrentCell.Value = dtp.Text; //时间控件选择时间时，就把时间赋给所在的单元格
        }

        /****************单元格被单击，判断是否是放时间控件的那一列*******************/

        private void dgrdvDayReportHC_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == C_DATE)
            {
                _Rectangle = dgrdvDayReportJJ.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true); //得到所在单元格位置和大小
                dtp.Size = new Size(_Rectangle.Width, _Rectangle.Height); //把单元格大小赋给时间控件
                dtp.Location = new Point(_Rectangle.X, _Rectangle.Y); //把单元格位置赋给时间控件
                dtp.Visible = true; //可以显示控件了
            }
            //else
            //    dtp.Visible = false;
        }

        /***********当列的宽度变化时，时间控件先隐藏起来，不然单元格变大时间控件无法跟着变大哦***********/

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            //dtp.Visible = false;
        }

        /***********滚动条滚动时，单元格位置发生变化，也得隐藏时间控件，不然时间控件位置不动就乱了********/

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            //dtp.Visible = false;
        }

        private void dgrdvDayReportJJ_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            //日期自动填充
            if (e.ColumnIndex == C_DATE)
            {
                //datetimepicker控件位置大小
                Rectangle rect = dgrdvDayReportJJ.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                dtp.Visible = true;
                dtp.Top = rect.Top;
                dtp.Left = rect.Left;
                dtp.Height = rect.Height;
                dtp.Width = rect.Width;
                //datetimepicker赋值
                if (dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value != null)
                {
                    dtp.Value = Convert.ToDateTime(dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value.ToString());
                }
                //默认填充系统时间
                else
                {
                    dtp.Value = DateTime.Now;
                    dgrdvDayReportJJ[e.ColumnIndex, e.RowIndex].Value = dtp.Text;
                }
            }
        }
    }
}