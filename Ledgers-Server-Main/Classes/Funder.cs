using System.Data;
using System.Dynamic;
using System.Text.Json;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Funder : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();

        public Funder(MySQL mySQL, string id) : base(id)
        {
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            var data = mySQL.Read(Config.Tables.FUNDERS, where);
            if (!(data.Rows.Count > 0))
            {
                throw new KeyNotFoundException(id);
            }
            Init(data.Rows[0]);
        }

        public Funder(DataRow row) : base(row)
        {
            Init(row);
        }

        private void Init(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                _data[column.ToString()] = row[column] is DBNull ? null : row[column].ToString();
            }
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.FUNDERS, Id);
                scope.Complete();
            }
        }

        public dynamic GetJSON()
        {
            var expando = new ExpandoObject() as IDictionary<string, object?>;
            foreach (var pair in _data)
            {
                expando[new string(pair.Key)] = new string(pair.Value);
            }
            return expando;
        }

        public static List<Funder> ReadAll(MySQL mySQL)
        {
            var funders = new List<Funder>();
            foreach (DataRow funder in mySQL.Read(Config.Tables.FUNDERS).Rows)
            {
                funders.Add(new Funder(funder));
            }
            return funders;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(GetJSON());
        }
    }
}
