﻿// ******************************************************************
// 概  述：预警信息结果查询声明数据库字段名称
// 作  者：秦凯
// 创建日期：2014/03/15
// 版本号：V1.0
// 版本信息:
// V1.0 新建
// ******************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibBusiness
{
    public class PreWarningResultQueryDBConstNames
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public const string TABLE_NAME = "T_EARLY_WARNING_RESULT";

        /// <summary>
        /// 临时数据库名称
        /// </summary>
        public const string TABLE_NAME_TEMP = "T_EARLY_WARNING_RESULT_TEMP";

        /// <summary>
        /// ID
        /// </summary>
        public const string ID = "id";

        /// <summary>
        /// 巷道ID
        /// </summary>
        public const string TUNNEL_ID = "tunnel_id";

        /// <summary>
        /// 巷道名称
        /// </summary>
        public const string TUNNEL_NAME = "tunnel_name";

        /// <summary>
        /// 班次
        /// </summary>
        public const string DATE_SHIFT = "shift";

        /// <summary>
        /// 日期
        /// </summary>
        public const string DATA_TIME = "date_time";

        /// <summary>
        /// 预警点坐标_X
        /// </summary>
        public const string COORDINATE_X = "COORDINATE_X";

        /// <summary>
        /// 预警点坐标_Y
        /// </summary>
        public const string COORDINATE_Y = "COORDINATE_Y";

        /// <summary>
        /// 预警点坐标_Y
        /// </summary>
        public const string COORDINATE_Z = "COORDINATE_Z";

        /// <summary>
        /// 预警类型
        /// </summary>
        public const string WARNING_TYPE = "warning_type";

        /// <summary>
        /// 预警结果
        /// </summary>
        public const string WARNING_RESULT = "warning_result";

        /// <summary>
        /// 瓦斯
        /// </summary>
        public const string GAS = "gas";

        /// <summary>
        /// 煤层
        /// </summary>
        public const string COAL = "coal";

        /// <summary>
        /// 地质
        /// </summary>
        public const string GEOLOGY = "geology";

        /// <summary>
        /// 通风
        /// </summary>
        public const string VENTILATION = "ventilation";

        /// <summary>
        /// 管理
        /// </summary>
        public const string MANAGEMENT = "management";

        /// <summary>
        /// 描述
        /// </summary>
        public const string DETAIL_INFO = "detail_info";
    }
}
