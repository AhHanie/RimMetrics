using RimWorld;

namespace RimMetrics.CalculatedStats
{
    public class ColonistRecordTotalStatProvider : CalculatedStatProvider
    {
        public override int CalculateInt(string statId)
        {
            var recordDefName = StatRegistry.GetMeta(statId).RecordDefName;

            var total = 0;
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                total += RecordValueReader.GetRecordValueInt(colonist, recordDefName);
            }

            return total;
        }

        public override float CalculateFloat(string statId)
        {
            var recordDefName = StatRegistry.GetMeta(statId).RecordDefName;
           
            var total = 0f;
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                total += RecordValueReader.GetRecordValueFloat(colonist, recordDefName);
            }

            return total;
        }
    }
}
