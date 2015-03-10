﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using LibEntity;

namespace LibBusiness
{
    public class DataBindUtil
    {
        private static void DataBindListControl(ListControl lc,
            ICollection<object> dataSource, string displayMember,
            string valueMember, String selectedText = "")
        {
            if (dataSource.Count <= 0) lc.DataSource = null;
            lc.DataSource = dataSource;
            lc.DisplayMember = displayMember;
            lc.ValueMember = valueMember;
            lc.Text = selectedText;
        }

        private static void DataBindListControl(DataGridView dgv,
            ICollection<object> dataSource)
        {
            if (dataSource.Count <= 0) return;
            dgv.AutoGenerateColumns = false;
            dgv.DataSource = dataSource;
        }

        public static void LoadMineName(ListControl lb, String selectedText
            = "")
        {
            var mines = Mine.FindAll();
            if (mines != null) DataBindListControl(lb, mines, "MineName",
                "MineId", selectedText);
        }

        public static void LoadMineName(DataGridView dgv, String
            selectedText = "")
        {
            var mines = Mine.FindAll();
            DataBindListControl(dgv, mines);
        }

        public static void LoadHorizontalName(ListControl lb, int mineId,
            String selectedText = "")
        {
            var horizontals = Horizontal.FindAllByMineId(mineId);
            if (horizontals != null)
                DataBindListControl(lb, horizontals, "HorizontalName",
                    "HorizontalId", selectedText);
        }

        public static void LoadHorizontalName(DataGridView dgv, int mineId)
        {
            var horizontals = Horizontal.FindAllByMineId(mineId);
            if (horizontals != null) DataBindListControl(dgv, horizontals);
        }

        public static void LoadMiningAreaName(ListControl lb, int
            horizontalId, String selectedText = "")
        {
            var miningAreas =
                MiningArea.FindAllByHorizontalId(horizontalId);
            if (miningAreas != null)
                DataBindListControl(lb, miningAreas, "MiningAreaName",
                    "MiningAreaId", selectedText);
        }

        public static void LoadMiningAreaName(DataGridView dgv, int
            horizontalId)
        {
            var miningAreas =
                MiningArea.FindAllByHorizontalId(horizontalId);
            if (miningAreas != null) DataBindListControl(dgv, miningAreas);
        }



        public static void LoadWorkingFaceName(ListControl lb, int
            miningAreaId, String selectedText = "")
        {
            var workingFaces =
                WorkingFace.FindAllByMiningAreaId(miningAreaId);
            if (workingFaces != null)
                DataBindListControl(lb, workingFaces, "WorkingFaceName",
                    "WorkingFaceId", selectedText);
        }

        public static void LoadTunnelName(ListControl lb, int workingFaceId,
            String selectedText = "")
        {
            var tunnels = Tunnel.FindAllByWorkingFaceId(workingFaceId);
            if (tunnels != null)
                DataBindListControl(lb, tunnels, "TunnelName", "TunnelId",
                    selectedText);
        }

        //public static void LoadWorkingFaceName(DataGridView dgv, int miningAreaId)
        //{
        //    var workingFaces = WorkingFace.FindAllByMiningAreaId(miningAreaId);
        //    if (workingFaces != null) DataBindListControl(dgv, workingFaces);
        //}

        public static void LoadCoalSeamsName(ListControl lb, String
            selectedText = "")
        {
            var coalSeams = CoalSeams.FindAll();
            if (coalSeams != null) DataBindListControl(lb, coalSeams,
                "CoalSeamsName", "CoalSeamsId", selectedText);
        }

        public static void LoadCoalSeamsName(DataGridView dgv)
        {
            var coalSeams = CoalSeams.FindAll();
            if (coalSeams != null) DataBindListControl(dgv, coalSeams);
        }

        public static void LoadLithology(ListControl lb, String
            selectedText = "")
        {
            var lithologys = Lithology.FindAll();
            if (lithologys != null) DataBindListControl(lb, lithologys,
                "LithologyName", "LithologyId", selectedText);
        }

        public static void LoadTeam(ListControl lb, String selectedText = "")
        {
            var teams = Team.FindAll();
            if (teams != null)
                DataBindListControl(lb, teams,
                    "TeamName", "TeamId", selectedText);
        }

        public static void LoadTeamMemberByTeamName(ListControl lb, string
            teamName, String selectedText = "")
        {
            var team = Team.FindOneByTeamName(teamName);
            if (team != null)
            {
                var teamMembers = team.TeamMember.Split(',');
                DataBindListControl(lb, teamMembers, null, null,
                    selectedText);
            }
        }

        public static void LoadWorkTime(ListControl lb, int timeGroupId,
            String selectedText = "")
        {
            var workingTimes =
                WorkingTime.FindAllByWorkTimeGroupId(timeGroupId);
            if (workingTimes != null)
            {
                DataBindListControl(lb, workingTimes, "WorkTimeName",
                    "WorkTimeName", selectedText);
            }
        }


        public static void LoadWorkTime(DataGridViewComboBoxColumn dgvcbc
            , int timeGroupId, String selectedText = "")
        {
            var workingTimes =
                WorkingTime.FindAllByWorkTimeGroupId(timeGroupId);
            foreach (var t in workingTimes)
            {
                dgvcbc.Items.Add(t.WorkTimeName);
            }
        }

        public static string JudgeWorkTimeNow(string workStyle)
        {
            //获取班次
            WorkingTime[] workingTimes;
            workingTimes = workStyle == "三八制" ?
                WorkingTime.FindAllBy38Times() :
                WorkingTime.FindAllBy46Times();
            //小时
            int hour = DateTime.Now.Hour;
            string workTime = "";
            for (int i = 0; i < workingTimes.Length; i++)
            {
                //对比小时
                if (hour >
                    Convert.ToInt32(workingTimes[i].WorkTimeFrom.ToString().Remove(2))
                    &&
                    hour <=
                        Convert.ToInt32(workingTimes[i].WorkTimeTo.ToString().Remove(2)))
                {
                    //获取当前时间对应班次
                    workTime = workingTimes[i].WorkTimeName;
                }
            }
            return workTime;
        }

        public static DataTable ToDataTable(IList list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys =
                    list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }
    }
}
