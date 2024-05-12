using System.Data;

namespace Ledgers_Server_Main.Classes
{
    public abstract class Element
    {
        public readonly string Id;

        public Element(string id)
        {
            Id = id;
        }

        public Element(DataRow row)
        {
            var id = row["id"] is DBNull ? null : row["id"].ToString();
            if (id is null)
            {
                throw new NoNullAllowedException();
            }
            Id = id;
        }
    }
}
