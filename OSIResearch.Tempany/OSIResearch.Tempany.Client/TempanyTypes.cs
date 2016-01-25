using OSIResearch.Tempany.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIResearch.Tempany.Client
{

    public class Asset
    {
        internal Asset() { }
        public static Asset New()
        {
            throw new NotImplementedException();
        }

        public static Asset Load(long id)
        {
            throw new NotImplementedException();
        }

        public static List<Asset> Load(IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        public long ID { get; private set; }
        private List<StreamContext> _streamContexts;
        public ReadOnlyCollection<StreamContext> StreamContexts { get { return _streamContexts.AsReadOnly(); } }
    }

    public class StreamContext
    {
        internal StreamContext() { }
        public static StreamContext New()
        {
            StreamContext newContext = new StreamContext();

            throw new NotImplementedException();
        }

        public long ID { get; private set; }
        public Stream Stream { get; private set; }
        private List<Context> _contexts;
        public ReadOnlyCollection<Context> Contexts { get { return _contexts.AsReadOnly(); } }
    }

    public class Context
    {
        public static Context New(string name, ContextSpace space)
        {
            Context newContext = new Context()
            {
                Name = name,
                Space = space
            };

            //Save context and capture ID
            throw new NotImplementedException();
        }

        public static Context Load(long id)
        {
            throw new NotImplementedException();
        }

        public long ID { get; private set; }
        public string Name { get; private set; }
        public ContextSpace Space { get; private set; }
    }

    public class ContextSpace
    {
        public static ContextSpace New(string Name)
        {
            throw new NotImplementedException();
        }

        public static ContextSpace Load(long id)
        {
            throw new NotImplementedException();
        }

        public long ID { get; private set; }
        public string Name { get; private set; }
    }


    public class Stream
    {
        public long ID { get; private set; }
        public StreamType Type { get; private set; }
        public StreamTime Snapshot { get; private set; }

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
