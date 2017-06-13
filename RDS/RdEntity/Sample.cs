using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RdCore;
using Sias.Core.Interfaces;

namespace RdEntity
{
    public enum Sex
    {
        Male,
        Female
    }

    public class Sample
    {
        private ProcessWorkbench workbench = ProcessWorkbench.Workbench;
        private int _smpPos;
        private ICavity _cavity;
        private double _volume;

        /// <summary>
        /// 样本存储Id(数据库)
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 样本ID
        /// </summary>
        public int SmpId { get; set; }

        /// <summary>
        /// 样本位置
        /// </summary>
        public int SmpPos
        {
            get
            {
                return _smpPos;
            }
            set
            {
                _smpPos = value;
            }
        }

        /// <summary>
        /// 是否做项目1
        /// 0：不做        1：做
        /// </summary>
        public int IsDoItem1 { get; set; }

        /// <summary>
        /// 是否做项目2
        /// 0：不做        1：做
        /// </summary>
        public int IsDoItem2 { get; set; }

        /// <summary>
        /// 是否做项目3
        /// 0：不做        1：做
        /// </summary>
        public int IsDoItem3 { get; set; }

        /// <summary>
        /// 是否做项目4
        /// 0：不做        1：做
        /// </summary>
        public int IsDoItem4 { get; set; }

		private int _maxItemCount;
		/// <summary>
		/// 当前样本所做项目数
		/// </summary>
		public int MaxItemCount
		{
			get {
				_maxItemCount = 0;
				if (IsDoItem1 == 1)
				{
					_maxItemCount += 1;
				}
				if (IsDoItem2 == 1)
				{
					_maxItemCount += 1;
				}
				if (IsDoItem3 == 1)
				{
					_maxItemCount += 1;
				}
				if (IsDoItem4 == 1)
				{
					_maxItemCount += 1;
				}

				return _maxItemCount; }
			private set
			{
				
			}
		}

		/// <summary>
		/// 是否急诊
		/// </summary>
		public int IsEmergency { get; set; }

        /// <summary>
        /// 是否等待取样
        /// 0：样本已取用     1：样本待取用
        /// </summary>
        public int IsWaitingForTake { get; set; }

        /// <summary>
        /// 样本条码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 孔
        /// </summary>
        public ICavity Cavity
        {
            get
            {
                return _cavity;
            }
            set
            {
                string name = "";
                if (_smpPos >= 1 && _smpPos <= 80)
                {
                    name = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1", _smpPos);
                    _cavity = workbench.Layout.GetCavityByName(name);
                }
                else if (_smpPos >= 81 && _smpPos <= 88)
                {
                    name = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1", _smpPos - 80);
                    _cavity = workbench.Layout.GetCavityByName(name);
                }
                
            }
        }

        /// <summary>
        /// 当前液量
        /// </summary>
        public double Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = Cavity.Liquid.Volume;
            }
        }

        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 样本类别
        /// </summary>
        public string SampleType { get; set; }

        /// <summary>
        /// 采样日期
        /// </summary>
        public DateTime SamplingDate { get; set; }

        ///// <summary>
        ///// 样本测量的项目
        ///// </summary>
        //public List<ReagentItem> MeasureItems { get; set; }



        public Sample()
        {
            //MeasureItems = new List<ReagentItem>();
        }

        public Sample(int smpPos)
        {
            _smpPos = smpPos;
           // MeasureItems = new List<ReagentItem>();
        }
    
}
}
