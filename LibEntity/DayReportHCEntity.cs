﻿// ******************************************************************
// 概  述：回采进尺日报数据实体
// 作  者：宋英杰
// 日  期：2014/3/12
// 版本号：V1.0
// 版本信息：
// V1.0 新建
// ******************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEntity
{
    /// <summary>
    /// 回采进尺日报实体
    /// </summary>
    public class DayReportHCEntity : DayReportEntity
    {
        private int isDel;
        /// <summary>
        /// 该条记录是否删除，用于修改进尺信息
        /// </summary>
        public int IsDel
        {
            get { return isDel; }
            set { isDel = value; }
        }
    }
}
