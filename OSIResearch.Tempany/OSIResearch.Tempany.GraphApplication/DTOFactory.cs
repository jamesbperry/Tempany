using OSIResearch.Tempany.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Trinity.Core.Lib;
using Trinity.TSL.Lib;

namespace OSIResearch.Tempany.GraphApplication
{
    public static class DTOFactory
    {
        public static TimestampPointerDTO ToTimestampPointerDTO(this Timestamp_Accessor tsa)
        {
            return new TimestampPointerDTO(Ticks: tsa.Ticks, Values: tsa.Values, Previous: tsa.Previous, Id: tsa.CellID.Value);
        }

        public static StreamValueDTO ToStreamValueDTO(this StreamValue_Accessor sva)
        {
            StreamValueDTO dto = new StreamValueDTO();

            if (sva.Contains_CellValue) dto.CellValue = sva.CellValue;
            if (sva.Contains_DigitalValue) dto.DigitalValue = sva.DigitalValue;
            if (sva.Contains_NumericValue) dto.NumericValue = sva.NumericValue;
            if (sva.Contains_StringValue) dto.StringValue = sva.StringValue;

            return dto;
        }

        public static void CreateCell(this StreamValueDTO_Accessor svda, ref StreamValue sv)
        {
            if (svda.Contains_CellValue) sv = new StreamValue(CellValue: svda.CellValue);
            if (svda.Contains_DigitalValue) sv = new StreamValue(DigitalValue: svda.DigitalValue);
            if (svda.Contains_NumericValue) sv = new StreamValue(NumericValue: svda.NumericValue);
            if (svda.Contains_StringValue) sv = new StreamValue(StringValue: svda.StringValue);
        }

        public static void CreateCell(this StreamValueDTO svd, ref StreamValue sv)
        {
            if (svd.CellValue.HasValue) sv = new StreamValue(CellValue: svd.CellValue);
            if (svd.DigitalValue.HasValue) sv = new StreamValue(DigitalValue: svd.DigitalValue);
            if (svd.NumericValue.HasValue) sv = new StreamValue(NumericValue: svd.NumericValue);
            if (svd.StringValue != null) sv = new StreamValue(StringValue: svd.StringValue);
        }

        public static List<StreamValueDTO> GetValues(long valueCollectionCellId)
        {
            List<long> valueCells;
            using (var valueCollectionCell = Global.LocalStorage.UseStreamValueCollection(valueCollectionCellId))
            {
                valueCells = valueCollectionCell.Values;
            }

            return GetValues(valueCells);
        }

        public static List<StreamValueDTO> GetValues(List<long> valueCellIds)
        {
            List<StreamValueDTO> values = new List<StreamValueDTO>(valueCellIds.Count);
            foreach (var valueCellId in valueCellIds)
            {
                using (var valueCell = Global.LocalStorage.UseStreamValue(valueCellId))
                {
                    values.Add(valueCell.ToStreamValueDTO());
                }
            }
            return values;
        }

        
        public static void Apply(this StreamValueCollectionDTOWriter response, TimestampPointerDTO timestamp, long streamCellId)
        {
            response.Timestamp = timestamp.Ticks;
            response.StreamCellId = streamCellId;

            List<long> valueCellIds;
            using (var valuesCollection = Global.LocalStorage.UseStreamValueCollection(timestamp.Values))
            {
                valueCellIds = valuesCollection.Values;
            }

            foreach (var valueCellId in valueCellIds)
            {
                using (var value = Global.LocalStorage.UseStreamValue(valueCellId))
                {
                    StreamValueDTO valueDTO = value.ToStreamValueDTO();
                    response.Values.Add(valueDTO);
                }
            }
        }

    }
}
