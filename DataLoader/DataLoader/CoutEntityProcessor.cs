using System;

namespace Ogdi.Data.DataLoader
{
    internal class CoutEntityProcessor : EntityProcessor
    {
        public override void ProcessEntity(string entitySetName, Entity entity)
        {
            Console.WriteLine("{0} Entity Start", entitySetName);

            foreach (Property p in entity)
            {
                Console.WriteLine("\t{0} | {1} | {2}", p.Name, p.Value.GetType(), p.Value);
            }

            Console.WriteLine("Entity End");
            Console.WriteLine();
        }
    }
}