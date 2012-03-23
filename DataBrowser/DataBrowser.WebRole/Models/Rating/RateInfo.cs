using System.Web;
using Ogdi.InteractiveSdk.Mvc.Repository;

namespace Ogdi.InteractiveSdk.Mvc.Models.Rating
{
    public class RateInfo
    {
        public RateInfo(string itemKey)
        {
            ItemKey = itemKey;
        }

        public RateInfo(string itemKey, int positiveRates, int negativeRates)
            : this(itemKey)
        {
            this.positiveRates = positiveRates;
            this.negativeRates = negativeRates;
            this.ReadOnly = true;
        }

        public string ItemKey { get; set; }
        public bool ReadOnly { get; set; }
        private int? positiveRates;
        public int PositiveRates 
        {
            get
            {
                if (!positiveRates.HasValue)
                {
                    RefreshRating();
                }
                return positiveRates.Value;
            }
        }
        private int? negativeRates;
        public int NegativeRates 
        {
            get
            {
                if (!negativeRates.HasValue)
                {
                    RefreshRating();
                }
                return negativeRates.Value;
            }
        }

        private void RefreshRating()
        {
            VoteResults vr = RatingRepository.GetVoteResults(this.ItemKey);
            positiveRates = vr.Positive;
            negativeRates = vr.Negative;
        }

        public bool HasUserVoted(HttpRequest req)
        {
            return RatingRepository.HasUserVoted(this.ItemKey, GetCurrentUser(req));
        }

        private string GetCurrentUser(HttpRequest req)
        {
            if (!req.IsAuthenticated)
            {
                return req.UserHostAddress;
            }
            else
            {
                return req.LogonUserIdentity.Name;
            }
        }
    }
}
