using System;

namespace ELOR.Laney.DataModels {
    public class TwoStringTuple : Tuple<string, string> {
        public TwoStringTuple(string item1, string item2) : base(item1, item2) {}
    }

    public class Entity : Tuple<long, Uri, string, string, Command> {
        public Entity(long item1, Uri item2, string item3, string item4, Command item5) : base(item1, item2, item3, item4, item5) {}
    }
}