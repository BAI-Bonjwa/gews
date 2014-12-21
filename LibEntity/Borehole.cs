﻿using Castle.ActiveRecord;

namespace LibEntity
{
    [ActiveRecord("T_BOREHOLE")]
    public class Borehole : ActiveRecordBase<Borehole>
    {
        /// <summary>
        ///     钻孔编号
        /// </summary>
        [PrimaryKey(PrimaryKeyType.Identity, "BOREHOLE_ID")]
        public int BoreholeId { get; set; }

        /// <summary>
        ///     孔号
        /// </summary>
        [Property("BOREHOLE_NUMBER")]
        public string BoreholeNumber { get; set; }

        /// <summary>
        ///     地面标高
        /// </summary>
        [Property("GROUND_ELEVATION")]
        public double GroundElevation { get; set; }

        /** 坐标X **/

        /// <summary>
        ///     坐标X
        /// </summary>
        [Property("COORDINATE_X")]
        public double CoordinateX { get; set; }

        /** 坐标Y **/

        /// <summary>
        ///     坐标Y
        /// </summary>
        [Property("COORDINATE_Y")]
        public double CoordinateY { get; set; }

        /** 坐标Z **/

        /// <summary>
        ///     坐标Z
        /// </summary>
        [Property("COORDINATE_Z")]
        public double CoordinateZ { get; set; }

        /** 煤层结构 **/

        /// <summary>
        ///     煤层结构
        /// </summary>
        [Property("COAL_SEAMS_TEXTURE")]
        public string CoalSeamsTexture { get; set; }

        /** BID **/

        /// <summary>
        ///     BID
        /// </summary>
        [Property("BID")]
        public string BindingId { get; set; }
    }
}