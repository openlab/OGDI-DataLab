using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Ogdi.Data.DataLoader
{
    public class DataLoader : IDataLoader
    {        
        public DataLoader(DataLoaderParams parameters, EntityProducer producer, EntityProcessor processor)
        {
            Params = parameters;
            Producer = producer;
            Processor = processor;
        }

        protected DataLoaderParams Params { get; set; }

        protected EntityProducer Producer { get; set; }

        protected EntityProcessor Processor { get; set; }

        public void Verify()
        {
            Producer.ValidateParams();
            Processor.ValidateParams(Producer.SchemaEntity);
        }

        public virtual void Load(ProgressCallback progressNotifier, OnContinueExceptionCallback exceptionNotifier)
        {
			bool needLoadData = Producer != null;

			if (needLoadData)
                Verify();

        	OnLoadStart();

			if (needLoadData)
			{
				int count = 0;
				int loadThreadsCount = int.Parse(ConfigurationManager.AppSettings["LoadThreadsCount"]);
				var options = new ParallelOptions { MaxDegreeOfParallelism = loadThreadsCount };

				Parallel.ForEach(Producer.GetEntitiesEnumerator(exceptionNotifier), options, i =>
				{
					try
					{
						Processor.ProcessEntity(Params.TableMetadataEntity.EntitySet, i);
					}
					catch (Exception ex)
					{
						if (exceptionNotifier != null)
							exceptionNotifier(new EntityProcessingException(i.ToString(), ex));
					}
					if (progressNotifier != null)
						progressNotifier(Producer.EntityCount, Interlocked.Increment(ref count));
				});
			}

        	Processor.ProcessTableMetadataEntity(DataLoaderConstants.EntitySetTableMetadata, Params.TableMetadataEntity);

			if (needLoadData)
			{
				Processor.ProcessEntityMetadataEntity(DataLoaderConstants.EntitySetEntityMetadata, Producer.SchemaEntity);
			}
        }

        protected virtual void OnLoadStart() 
        { 
        }

    }
}
