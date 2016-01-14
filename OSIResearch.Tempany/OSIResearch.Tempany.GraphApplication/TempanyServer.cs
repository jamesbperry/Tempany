using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIResearch.Tempany.Generated;
using Trinity;
using Trinity.Core.Lib;
using Trinity.TSL.Lib;
using OSIResearch.Tempany.Core;
using System.Collections.Concurrent;
using System.Threading;

namespace OSIResearch.Tempany.GraphApplication
{
    class TempanyInsertion
    {
        public StreamValueCollectionDTO Request;
        public void Populate(StreamValueCollectionDTOReader requestReader)
        {
            Request = new StreamValueCollectionDTO(Values: requestReader.Values, Timestamp: requestReader.Timestamp, StreamCellId: requestReader.StreamCellId);
        }
    }

    class TempanyServer : TempanyServerBase
    {
        public override void CreateStreamHandler(CreateStreamArgsReader request, CellIdentifierWriter response)
        {
            Timestamp emptySnapshot = new Timestamp(Ticks: 0, Values: 0); //TODO real treatment of the null snapshot
            Global.LocalStorage.SaveTimestamp(emptySnapshot);

            TempanyStream newStream = new TempanyStream(ValueType: request.ValueType, IndexHeight:4, Snapshot: emptySnapshot.CellID);
            if (request.Contains_ValueTypeDiscriminator) newStream.ValueTypeDiscriminator = request.ValueTypeDiscriminator;
            Global.LocalStorage.SaveTempanyStream(newStream);

            response.Id = newStream.CellID;
        }

        public override void GetStreamValuesHandler(GetStreamValuesArgsReader request, StreamValueCollectionsDTOWriter response)
        {
            TimestampPointerDTO? exclusiveUpperBoundCell = GetTimestampCellAfterTime(request.StreamCellId, request.EndTime);
            TimestampPointerDTO? inclusiveUpperBoundCellNullable;
            if (exclusiveUpperBoundCell.HasValue)
            {
                inclusiveUpperBoundCellNullable = GetImmediatePredecessorTimestampCell(exclusiveUpperBoundCell.Value.Id);
            }
            else
            {
                inclusiveUpperBoundCellNullable = GetSnapshotTimestamp(request.StreamCellId);
            }

            if (!inclusiveUpperBoundCellNullable.HasValue) return;
            TimestampPointerDTO inclusiveUpperBoundCell = inclusiveUpperBoundCellNullable.Value;

            foreach (var valueCollection in GetValuesAfterTimestamp(request.StreamCellId, request.StartTime, inclusiveUpperBoundCell))
            {
                response.ValueCollections.Add(valueCollection);
            }
        }


        public override void GetStreamValueHandler(GetStreamValueArgsReader request, StreamValueCollectionDTOWriter response)
        {
            TimestampPointerDTO? cellAfterTime = GetTimestampCellAfterTime(request.StreamCellId, request.Timestamp);
            if (!cellAfterTime.HasValue)
            {
                return;
            }
            
            TimestampPointerDTO? cellBeforeTime = GetImmediatePredecessorTimestampCell(cellAfterTime.Value.Id);
            if (!cellBeforeTime.HasValue)
            {
                return;
            }

            response.Apply(cellBeforeTime.Value, request.StreamCellId);
        }

        private TimestampPointerDTO? GetSnapshotTimestamp(long streamCellId)
        {
            long streamSnapshotCellId;
            using (var streamCell = Global.LocalStorage.UseTempanyStream(streamCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                streamSnapshotCellId = streamCell.Snapshot;
            }

            using (var snapshotTimestamp = Global.LocalStorage.UseTimestamp(streamSnapshotCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                return snapshotTimestamp.ToTimestampPointerDTO();
            }
        }

        private IEnumerable<StreamValueCollectionDTO> GetValuesAfterTimestamp(long streamCellId, long inclusiveLowerBoundTimestamp, TimestampPointerDTO inclusiveUpperBoundCell)
        {
            TimestampPointerDTO? currentCell = inclusiveUpperBoundCell;

            while (currentCell.HasValue && currentCell.Value.Ticks >= inclusiveLowerBoundTimestamp)
            {
                List<StreamValueDTO> valuesDTO = DTOFactory.GetValues(currentCell.Value.Values);
                StreamValueCollectionDTO valuesCollectionDTO = new StreamValueCollectionDTO(valuesDTO, currentCell.Value.Ticks, streamCellId);
                yield return valuesCollectionDTO;

                currentCell = GetImmediatePredecessorTimestampCell(currentCell.Value.Id);
            }
        }

        private TimestampPointerDTO? GetTimestampCellAfterTime(long streamCellId, long timestamp)
        {
            TimestampPointerDTO? snapshotNullable = GetSnapshotTimestamp(streamCellId);

            //Empty stream, return empty
            if (!snapshotNullable.HasValue)
            {
                return null;
            }

            //Requested time is after Snapshot time; return Snapshot value
            TimestampPointerDTO snapshot = snapshotNullable.Value;
            if (timestamp >= snapshot.Ticks)
            {
                return snapshot;
            }

            //Request time is historical; search through history
            return GetTimestampCellAfterTime(snapshot, timestamp);
        }

        private TimestampPointerDTO GetTimestampCellAfterTimeRecursive(TimestampPointerDTO currentTimeCellDTO, long timestamp)
        {
            TimestampPointerDTO previousTimeCellDTO = GetPreviousTimestampCellId(currentTimeCellDTO, timestamp);

            if (previousTimeCellDTO.Id == currentTimeCellDTO.Id)
            {
                return currentTimeCellDTO;
            }
            else
            {
                return GetTimestampCellAfterTime(previousTimeCellDTO, timestamp); // logN
            }
        }


        private TimestampPointerDTO GetTimestampCellAfterTime(TimestampPointerDTO currentTimeCellDTO, long timestamp)
        {
            TimestampPointerDTO previousTimeCellDTO;
            while (true)
            {
                previousTimeCellDTO = GetPreviousTimestampCellId(currentTimeCellDTO, timestamp);
                if (previousTimeCellDTO.Id == currentTimeCellDTO.Id)
                    break;
                currentTimeCellDTO = previousTimeCellDTO;
            }

            return currentTimeCellDTO;

        }

        private TimestampPointerDTO? GetImmediatePredecessorTimestampCell(long cellId)
        {
            List<long> previousCells;
            using (var currentCell = Global.LocalStorage.UseTimestamp(cellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                previousCells = currentCell.Previous;
            }

            if (previousCells.Count == 0)
            {
                return null;
            }

            long previousCellId = previousCells[0];
            using (var previousCell = Global.LocalStorage.UseTimestamp(previousCellId, CellAccessOptions.ReturnNullOnCellNotFound))
            {
                if (previousCell == null)
                {
                    return null;
                }
                else
                {
                    return previousCell.ToTimestampPointerDTO();
                }
            }
        }
        
        private TimestampPointerDTO GetPreviousTimestampCellId(TimestampPointerDTO currentCell, long referenceTicks)
        {
            List<long> previousTimeCellIds = currentCell.Previous;
            long previousTimeCellId =  previousTimeCellIds[0];

            for (int i = previousTimeCellIds.Count - 1; i >= 0; i--)
            {
                previousTimeCellId = previousTimeCellIds[i];
                using (var previousTimeCell = Global.LocalStorage.UseTimestamp(previousTimeCellId, CellAccessOptions.ReturnNullOnCellNotFound))
                {
                    if (previousTimeCell == null)
                    {
                        continue;
                    }

                    if (previousTimeCell.Ticks > referenceTicks)
                    {
                        return previousTimeCell.ToTimestampPointerDTO();
                    }
                }
            }

            return currentCell;
        }

        public override void InsertStreamValueHandler(StreamValueCollectionDTOReader request)
        {
            long streamCellId = request.StreamCellId;
            long streamSnapshotCellId;
            StreamType streamType;
            int indexHeight;

            //Stream
            using (var streamCell = Global.LocalStorage.UseTempanyStream(streamCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                indexHeight = streamCell.IndexHeight;
                streamType = (StreamType)streamCell.ValueType;
                streamSnapshotCellId = streamCell.Snapshot;
            }

            //New values
            List<StreamValue> newValueCells = new List<StreamValue>(request.Values.Count);

            foreach (var valuesDto in request.Values)
            {
                StreamValue valueCell = new StreamValue(); //ugly struct hack, unnecessary construction
                valuesDto.CreateCell(ref valueCell);
                Global.LocalStorage.SaveStreamValue(valueCell);
                newValueCells.Add(valueCell);
            }

            List<long> newValueCellsIds = newValueCells.Select(sv => sv.CellID).ToList();

            //New values collection
            StreamValueCollection newValues = new StreamValueCollection(newValueCellsIds);
            Global.LocalStorage.SaveStreamValueCollection(newValues);

            int newLevel = GetRandomLevel(indexHeight);

            //Existing snapshot
            List<long> existingPreviousPointers;
            long existingTimestamp;
            long existingValues;
            using (var streamSnapshotCell = Global.LocalStorage.UseTimestamp(streamSnapshotCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                existingTimestamp = streamSnapshotCell.Ticks;
                existingValues = streamSnapshotCell.Values;
                existingPreviousPointers = streamSnapshotCell.Previous;
            }

            //For now, only support monotonic increasing timestamps
            if (request.Timestamp <= existingTimestamp)
            {
                throw new NotSupportedException("Specified timestamp is earlier than snapshot time. Stream timestamp must be monotonically increasing (CTP limitation).");
            }

            //New timestamp to hold existing snapshot
            List<long> trimmedExistingPreviousPointers = existingPreviousPointers.Take(newLevel).ToList();
            Timestamp shiftedExistingSnapshot = new Timestamp(existingTimestamp, existingValues, trimmedExistingPreviousPointers);
            Global.LocalStorage.SaveTimestamp(shiftedExistingSnapshot);

            //Update existing snapshot
            List<long> newPreviousPointers = Enumerable.Range(0, newLevel).Select(i => shiftedExistingSnapshot.CellID).Concat(existingPreviousPointers.Skip(newLevel)).ToList();
            using (var streamSnapshotCell = Global.LocalStorage.UseTimestamp(streamSnapshotCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
            {
                streamSnapshotCell.Ticks = request.Timestamp;
                streamSnapshotCell.Values = newValues.CellID;
                streamSnapshotCell.Previous = newPreviousPointers;
            }

            //update index height if increased
            if (newLevel > indexHeight)
            {
                using (var streamCell = Global.LocalStorage.UseTempanyStream(streamCellId, CellAccessOptions.ThrowExceptionOnCellNotFound))
                {
                    streamCell.IndexHeight = newLevel;
                }
            }
            
        }

        private int GetRandomLevel(int currentHeight)
        {
            int level = 1;

            int maxLevel = Math.Min(currentHeight, int.MaxValue / 2) * 2;

            const double p = 0.5; //.25;
            Random r = new Random();
            for (level = 1; r.NextDouble() < p && level < maxLevel; level++);
            return level;
        }
    }
}
