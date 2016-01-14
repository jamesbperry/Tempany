using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIResearch.Tempany.Generated;
using OSIResearch.Tempany.Core;

namespace OSIResearch.Tempany.Client
{
    public class TempanyClient
    {



    }

    public class Asset
    {
        public static Asset Load(long id)
        {
            throw new NotImplementedException();
        }

        public static List<Asset> Load(IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        public long ID { get; set; }
        public Dictionary<string, FacetInstance> Facets { get; set; }
    }

    public class FacetDefinition
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public Dictionary<string, AttributeDefinition> AttributeDefinitions { get; set; }
    }

    public class FacetInstance
    {
        public long ID { get; set; }
        public FacetDefinition Definition { get; set; }
        public Dictionary<string, AttributeInstance> Attributes { get; set; }
    }

    public class AttributeDefinition
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public bool IsIdentifier { get; set; }
        public StreamType Type { get; set; }
    }

    public class AttributeInstance
    {
        public long ID { get; set; }
        public AttributeDefinition Definition { get; set; }
        public Stream Stream { get; set; }
    }

    public class Stream
    {
        public long ID { get; set; }
        public StreamType Type { get; set; }
        public StreamTime Snapshot { get; set; }

        public void RefreshSnapshot()
        {
            throw new NotImplementedException();
        }

        public static void RefreshSnapshot(IEnumerable<Stream> streams)
        {
            //bulk method
            throw new NotImplementedException();
        }

        public StreamTime GetValue(DateTimeOffset timestamp)
        {
            throw new NotImplementedException();
        }

        public List<StreamTime> GetValues(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            throw new NotImplementedException();
        }

        public void InsertValue(StreamTime value)
        {
            throw new NotImplementedException();
        }

        public void InsertValues(IEnumerable<StreamTime> values)
        {
            throw new NotImplementedException();
        }
    }

    public class StreamTime
    {
        public DateTimeOffset Timestamp { get; set; }
        public List<StreamValue> Values { get; set; }
    }

    public class StreamValue
    {
        public object Value;
        public bool IsGood
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

}
