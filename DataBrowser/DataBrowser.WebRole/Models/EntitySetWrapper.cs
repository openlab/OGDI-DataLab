
namespace Ogdi.InteractiveSdk.Mvc.Models
{
    public class EntitySetWrapper
    {
        public EntitySet EntitySet { get; set; }
        public int Rating 
        {
            get
            {
                return PositiveVotes - NegativeVotes;
            }
        }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
        public int Views { get; set; }
    }
}
