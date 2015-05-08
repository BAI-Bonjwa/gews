﻿using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;

namespace LibEntity
{
    [ActiveRecord("T_VALUE_K1")]
    public class K1Value : ActiveRecordBase<K1Value>
    {
        public const String TABLE_NAME = "T_VALUE_K1";

        /// <summary>
        ///     主键ID
        /// </summary>
        [PrimaryKey(PrimaryKeyType.Identity)]
        public int Id { get; set; }

        /// <summary>
        ///     K1值分组ID
        /// </summary>
        [Property("VALUE_K1_ID")]
        public int K1ValueId { get; set; }

        /// <summary>
        ///     坐标X
        /// </summary>
        [Property("COORDINATE_X")]
        public double CoordinateX { get; set; }

        /// <summary>
        ///     坐标Y
        /// </summary>
        [Property("COORDINATE_Y")]
        public double CoordinateY { get; set; }

        /// <summary>
        ///     坐标Z
        /// </summary>
        [Property("COORDINATE_Z")]
        public double CoordinateZ { get; set; }

        /// <summary>
        ///     K1值
        /// </summary>
        [Property("VALUE_K1_DRY")]
        public double ValueK1Dry { get; set; }

        /// <summary>
        ///     湿煤K1值
        /// </summary>
        [Property("VALUE_K1_WET")]
        public double ValueK1Wet { get; set; }

        /// <summary>
        ///     Sv值
        /// </summary>
        [Property("VALUE_SV")]
        public double Sv { get; set; }

        /// <summary>
        ///     Sg值
        /// </summary>
        [Property("VALUE_SG")]
        public double Sg { get; set; }

        /// <summary>
        ///     Q值
        /// </summary>
        [Property("VALUE_Q")]
        public double Q { get; set; }

        /// <summary>
        ///     孔深
        /// </summary>
        [Property("BOREHOLE_DEEP")]
        public double BoreholeDeep { get; set; }

        /// <summary>
        ///     记录时间
        /// </summary>
        [Property("TIME")]
        public DateTime Time { get; set; }

        /// <summary>
        ///     录入时间
        /// </summary>
        [Property("TYPE_IN_TIME")]
        public DateTime TypeInTime { get; set; }

        /// <summary>
        ///     绑定巷道ID
        /// </summary>
        [BelongsTo("TUNNEL_ID")]
        public Tunnel Tunnel { get; set; }

        public static int GetMaxGroupId()
        {
            var obj = FindFirst(new Order("Id", false));
            return obj == null ? 0 : obj.K1ValueId;
        }

        public static K1Value[] FindAllByK1ValueId(int k1ValueId)
        {
            return FindAllByProperty("K1ValueId", k1ValueId);
        }
    }
}