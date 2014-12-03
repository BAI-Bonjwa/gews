﻿// ******************************************************************
// 概  述：预警结果实体（初步计算出的原始结果，未经任何处理）
// 作  者：杨小颖  
// 创建日期：2013/12/21
// 版本号：1.0
// ******************************************************************
using System;


namespace LibEntity
{

    /// <summary>
    /// 注意各个枚举值的大小，值越大，越危险
    /// </summary>
    public enum WARNING_LEVEL_RESULT
    {
        NODATA = 0,//无数据
        GREEN = 1,//绿色
        YELLOW = 2,//黄色
        RED = 3,//红色
    }

    /// <summary>
    /// 预警结果实体
    /// 目前只存储预警规则实体，日后扩展时再添加
    /// </summary>
    public class PreWarningResultEntity
    {
        //预警规则实体
        private PreWarningRulesEntity _preWarningRulesEntity;

        public PreWarningRulesEntity PreWarningRulesEntity
        {
            get { return _preWarningRulesEntity; }
            set { _preWarningRulesEntity = value; }
        }

        //预警数据通用信息实体
        private PreWarningDataCommonInfoEntity _warningDataCommonInfoEnt;

        public PreWarningDataCommonInfoEntity WarningDataCommonInfoEnt
        {
            get { return _warningDataCommonInfoEnt; }
            set { _warningDataCommonInfoEnt = value; }
        }

    }
}
