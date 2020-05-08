using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbrella.Models
{
    public class ChartModels
    {

        #region Base
        public class BaseChartModel
        {
            public BaseChartModel()
            {
                this.datasets = new List<BaseDataset>();
            }

            public string[] labels { get; set; }
            public List<BaseDataset> datasets { get; set; }
        }

        public class BaseDataset
        {
            public string label { get; set; }
            public string backgroundColor { get; set; }
            public decimal[] data { get; set; }
        }

        #endregion

        #region MultiBar
        public class MultiBarChartModel
        {
            public MultiBarChartModel()
            {
                this.datasets = new List<MultiBarDataset>();
            }
            public string[] labels { get; set; }
            public List<MultiBarDataset> datasets { get; set; }
        }

        public class MultiBarDataset
        {
            public string label { get; set; }
            public string borderWidth { get; set; }            
            public string backgroundColor { get; set; }
            public string hoverBorderColor { get; set; }            
            public decimal[] data { get; set; }
        }
        #endregion

        #region TimeScale

        public class TimeScaleChartModel : BaseChartModel
        {
            public TimeScaleChartModel()
            {
                this.declarationsDataset = new BaseDataset();
                this.purchaseOrdersDataset = new BaseDataset();
                this.datasets.Add(declarationsDataset);
                this.datasets.Add(purchaseOrdersDataset);
            }

            public BaseDataset declarationsDataset { get; set; }
            public BaseDataset purchaseOrdersDataset { get; set; }
            
        }

        #endregion









    }
}