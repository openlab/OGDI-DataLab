namespace Ogdi.Data.DataLoader
{
    public abstract class EntityProcessor
    {
        public abstract void ProcessEntity(string entitySetName, Entity entity);

        public virtual void ProcessTableMetadataEntity(string entitySetName, Entity entity)
        {
            ProcessEntity(entitySetName, entity);
        }

        public virtual void ProcessEntityMetadataEntity(string entitySetName, Entity entity)
        {
            ProcessEntity(entitySetName, entity);
        }

        public virtual void ValidateParams(Entity schemaEntity)
        {
        }
    }
}