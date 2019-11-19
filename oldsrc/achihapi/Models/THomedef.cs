using Microsoft.AspNet.OData.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace achihapi.Models
{
    public partial class THomedef : BaseModel
    {
        public THomedef()
        {
            TEvent = new HashSet<TEvent>();
            TEventHabit = new HashSet<TEventHabit>();
            TEventRecur = new HashSet<TEventRecur>();
            TFinAccount = new HashSet<TFinAccount>();
            TFinAccountCtgy = new HashSet<TFinAccountCtgy>();
            TFinAssetCtgy = new HashSet<TFinAssetCtgy>();
            TFinControlcenter = new HashSet<TFinControlcenter>();
            TFinDocType = new HashSet<TFinDocType>();
            TFinDocument = new HashSet<TFinDocument>();
            TFinOrder = new HashSet<TFinOrder>();
            TFinPlan = new HashSet<TFinPlan>();
            TFinTmpdocDp = new HashSet<TFinTmpdocDp>();
            TFinTmpdocLoan = new HashSet<TFinTmpdocLoan>();
            TFinTranType = new HashSet<TFinTranType>();
            THomemem = new HashSet<THomemem>();
            THomemsg = new HashSet<THomemsg>();
            TLearnCtgy = new HashSet<TLearnCtgy>();
            TLearnEnsent = new HashSet<TLearnEnsent>();
            TLearnEnword = new HashSet<TLearnEnword>();
            TLearnHist = new HashSet<TLearnHist>();
            TLearnObj = new HashSet<TLearnObj>();
            TLearnQtnBank = new HashSet<TLearnQtnBank>();
            TLibBook = new HashSet<TLibBook>();
            TLibBookCtgy = new HashSet<TLibBookCtgy>();
            TLibLocation = new HashSet<TLibLocation>();
            TLibPerson = new HashSet<TLibPerson>();
        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Details { get; set; }
        public string Host { get; set; }
        public string Basecurr { get; set; }

        public ICollection<TEvent> TEvent { get; set; }
        public ICollection<TEventHabit> TEventHabit { get; set; }
        public ICollection<TEventRecur> TEventRecur { get; set; }
        public ICollection<TFinAccount> TFinAccount { get; set; }
        public ICollection<TFinAccountCtgy> TFinAccountCtgy { get; set; }
        public ICollection<TFinAssetCtgy> TFinAssetCtgy { get; set; }
        public ICollection<TFinControlcenter> TFinControlcenter { get; set; }
        public ICollection<TFinDocType> TFinDocType { get; set; }
        public ICollection<TFinDocument> TFinDocument { get; set; }
        public ICollection<TFinOrder> TFinOrder { get; set; }
        public ICollection<TFinPlan> TFinPlan { get; set; }
        public ICollection<TFinTmpdocDp> TFinTmpdocDp { get; set; }
        public ICollection<TFinTmpdocLoan> TFinTmpdocLoan { get; set; }
        public ICollection<TFinTranType> TFinTranType { get; set; }
        public ICollection<THomemem> THomemem { get; set; }
        public ICollection<THomemsg> THomemsg { get; set; }
        public ICollection<TLearnCtgy> TLearnCtgy { get; set; }
        public ICollection<TLearnEnsent> TLearnEnsent { get; set; }
        public ICollection<TLearnEnword> TLearnEnword { get; set; }
        public ICollection<TLearnHist> TLearnHist { get; set; }
        public ICollection<TLearnObj> TLearnObj { get; set; }
        public ICollection<TLearnQtnBank> TLearnQtnBank { get; set; }
        public ICollection<TLibBook> TLibBook { get; set; }
        public ICollection<TLibBookCtgy> TLibBookCtgy { get; set; }
        public ICollection<TLibLocation> TLibLocation { get; set; }
        public ICollection<TLibPerson> TLibPerson { get; set; }
    }
}
