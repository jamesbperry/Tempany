using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Trinity.Storage;
using OSIResearch.Tempany.Generated;
using OSIResearch.Tempany.Core;
using System.Diagnostics;

namespace OSIResearch.Tempany.GraphApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TrinityConfig.CurrentRunningMode = RunningMode.Embedded;
            
            TempanyServer server = new TempanyServer();
            server.Start();

            Demo(server);
        }

        private static void Demo(TempanyServer server)
        {
            int serverId = 0;
            StreamType streamType = StreamType.Numeric;

            CreateStreamArgsWriter createStreamArgs = new CreateStreamArgsWriter((byte)streamType);
            var createStreamResponse = Global.CloudStorage.CreateStreamToTempanyServer(serverId, createStreamArgs);

            long streamCellId = createStreamResponse.Id;

            int minutes = 360000;
            int valuesPerMinute = 60;
            
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset startTime = now - TimeSpan.FromMinutes(minutes);

            Random rand = new Random();

            //Write values
            Stopwatch writeStopwatch = Stopwatch.StartNew();
            int numValues = minutes * valuesPerMinute;
            for (int i = 0; i <= minutes; i++)
            {
                //Console.WriteLine("Minute {0}", i);
                for (int j = 0; j < valuesPerMinute; j++)
                {
                    double innerValue = i + 1.0 * j / valuesPerMinute * rand.NextDouble();
                    StreamValueDTO valueDTO = new StreamValueDTO(NumericValue: innerValue);
                    List<StreamValueDTO> valueDTOs = new List<StreamValueDTO>() { valueDTO };
                    long timestamp = (startTime + TimeSpan.FromMinutes(i + 1.0 * j / valuesPerMinute)).UtcTicks;
                    StreamValueCollectionDTOWriter valueCollectionWriter = new StreamValueCollectionDTOWriter(valueDTOs, timestamp, streamCellId);
                    Global.CloudStorage.InsertStreamValueToTempanyServer(serverId, valueCollectionWriter);
                }
            }
            long writeTime = writeStopwatch.ElapsedMilliseconds;
            double timePerEvent = 1.0 * writeTime / numValues;
            Console.WriteLine("Finished writing after {0}ms. Average of {1}ms/event", writeTime, timePerEvent);

            //Read values
            //Stopwatch readStopwatch = Stopwatch.StartNew();
            //GetStreamValuesArgsWriter getValuesArgs = new GetStreamValuesArgsWriter(streamCellId, startTime.UtcTicks, now.UtcTicks, 0);
            //var valuesReader = Global.CloudStorage.GetStreamValuesToTempanyServer(serverId, getValuesArgs);
            //long readTime = readStopwatch.ElapsedMilliseconds;
            //Console.WriteLine("Read elapsed: {0}ms", readTime);

            //List<string> output = new List<string>(valuesReader.ValueCollections.Count);
            //foreach (var valueCollection in valuesReader.ValueCollections)
            //{
            //    output.Add(ValueCollectionToPrettyString(valueCollection));
            //}

            //long fullReadTime = readStopwatch.ElapsedMilliseconds;
            //Console.WriteLine("Output elapsed: {0}ms", fullReadTime);

            Console.WriteLine("Random access");
            for (int i = 0; i < 50; i++)
            {
                DateTimeOffset timestamp = startTime + TimeSpan.FromMinutes(minutes * rand.NextDouble());
                GetStreamValueArgsWriter getValueArgs = new GetStreamValueArgsWriter(streamCellId, timestamp.UtcTicks);
                Stopwatch sw = Stopwatch.StartNew();
                var randomSeekValueReader = Global.CloudStorage.GetStreamValueToTempanyServer(0, getValueArgs);
                long swElapsed = sw.ElapsedMilliseconds;
                Console.WriteLine("{0}ms - {1}", swElapsed, ValueCollectionToPrettyString(randomSeekValueReader));
            }

            Console.ReadKey();

        }

        private static string ValueCollectionToPrettyString(StreamValueCollectionDTO_Accessor valueCollection)
        {
            var valuesToStrings = valueCollection.Values.Select(v => v.NumericValue.ToString());
            var valuesToString = string.Join("; ", valuesToStrings);
            return string.Format("{0} {1}", new DateTimeOffset(valueCollection.Timestamp, TimeSpan.Zero), valuesToString);
        }

    }
}
