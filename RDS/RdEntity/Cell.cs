using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RdEntity.Reader;

namespace RdEntity
{
    public class Cell
    {
        public int Id { get; set; }
        /// <summary>
        /// 使用的ADP
        /// </summary>
        public int UsedAdp { get; set; }
        /// <summary>
        /// 是否已加样
        /// </summary>
        public bool IsAdded { get; set; }
        /// <summary>
        /// 样本编号
        /// </summary>
        public int SampleId { get; set; }
        /// <summary>
        /// 样本名称
        /// </summary>
        public string SampleName { get; set; }
        /// <summary>
        /// 是否急诊
        /// </summary>
        public bool IsEmergency { get; set; }
        /// <summary>
        /// 样本类型
        /// </summary>
        public string SampleCategory { get; set; }
        /// <summary>
        /// 项目编号
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 孔内液量
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 加酶时间
        /// </summary>
        public DateTime EnzymeTime { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        public Result Result { get; set; }
        
    }
}
