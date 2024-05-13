using System.Data;
using System.Dynamic;
using System.Linq;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Parameter : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();

        public Parameter(DataRow row) : base(row)
        {
            row.Table.Columns.Cast<DataColumn>().ToList()
                .ForEach(column => _data[column.ToString()] = row[column]?.ToString());
        }

        public dynamic GetJSON()
        {
            return _data;
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.PARAMETERS, Id);
                scope.Complete();
            }
        }

        public static List<Parameter> ReadAllKeys(MySQL mySQL)
        {
            var where = new Dictionary<string, string[]> { { "type", new string[] { "key" } } };
            return mySQL.Read(Config.Tables.PARAMETERS, where).AsEnumerable()
                .Select<DataRow, Parameter>(parameter => new Parameter(parameter))
                .ToList();
        }
    }
}
